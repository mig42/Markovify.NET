using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Markovify.NET
{
    /// <summary>
    /// A source text to process using Markov chains.
    /// </summary>
    /// <param name="inputText">
    ///   The source text.
    /// </param>
    /// <param name="stateSize">
    ///   The number of words in the model's state
    /// </param>
    /// <param name="chain">
    ///   A trained <see cref="Chain" /> for this text, if pre-processed.
    /// </param>
    /// <param name="parsedSentences">
    ///   A list of lists, where each outer list is a "run" of the process (i.e. a single sentence),
    ///   and each inner list contains the steps (i.e. words) in the run.
    ///   If you want to simulate an infinite process, you can come very close by passing just one,
    ///   very long run.
    /// </param>
    /// <param name="retainOriginal">
    ///   Whether to keep the original corpus.
    /// </param>
    /// <param name="isWellFormed">
    ///   Indicates whether sentences should be well-formed, preventing unmatched quotes,
    ///   parentheses by default, or a custom regular expression can be provided.
    /// </param>
    /// <param name="rejectExpression">
    ///   If <paramref name="isWellFormed" /> is <c>true</c>, this can be provided to override
    ///   the standard rejection pattern.
    /// </param>
    public class Text
    {
        [DefaultValue(2)]
        [JsonProperty("stateSize")]
        public int StateSize { get; set; } = 2;

        [JsonProperty("chain")]
        public Chain Chain { get; set; }

        [JsonProperty("sourceText")]
        public string SourceText { get; set; }

        [JsonProperty("isWellFormed")]
        public bool IsWellFormed { get; set; }

        [JsonProperty("rejectExpression")]
        public Regex RejectExpression { get; set; } = DefaultRejectExpression;

        public static Text Build(
            string inputText,
            int stateSize = 2,
            bool retainOriginal = true,
            bool isWellFormed = true,
            Regex rejectExpression = null)
        {
            if (rejectExpression == null && isWellFormed)
                rejectExpression = DefaultRejectExpression;

            IEnumerable<IEnumerable<string>> corpus = GenerateCorpus(
                inputText, isWellFormed ? rejectExpression : null);

            return new Text
            {
                Chain = new Chain(corpus, stateSize),
                StateSize = stateSize,
                IsWellFormed = isWellFormed,
                RejectExpression = rejectExpression,
                SourceText = retainOriginal && corpus.Count() > 0 && !string.IsNullOrEmpty(inputText)
                    ? string.Join(" ", corpus.Select(words => string.Join(" ", words)))
                    : null
            };
        }

        /// <summary>
        ///   Compiles the text model for improved text generation speed and reduced size.
        /// </summary>
        /// <param name="inPlace">
        ///   Whether to compile the current instance or create a new instance.
        /// </param>
        /// <returns>A compiled instance of <see cref="Text" /></returns>
        public Text Compile(bool inPlace = false)
        {
            if (inPlace)
            {
                Chain.Compile(true);
                return this;
            }

            return new Text
            {
                Chain = Chain.Compile(false),
                StateSize = StateSize,
                SourceText = SourceText,
                IsWellFormed = IsWellFormed,
                RejectExpression = RejectExpression,
            };
        }

        /// <summary>
        ///   Given a text string, returns a list of lists. That is, a list of "sentences",
        ///   each of which is a list of words. Before splitting into words, the sentences
        ///   are filtered through <see cref="TestSentenceInput" />
        /// </summary>
        /// <param name="inputText">The sourcce text</param>
        /// <returns>The corpus as a list of lists.</returns>
        static IEnumerable<IEnumerable<string>> GenerateCorpus(
            string inputText, Regex rejectExpression)
        {
            return Split.IntoSentences(inputText)
                .Where(sentence => TestSentenceInput(sentence, rejectExpression))
                .Select(Split.IntoWords);
        }

        /// <summary>
        ///   A basic sentence filter. It will reject empty or whitespace strings
        ///   that (optionally) match a given regular expression.
        /// </summary>
        /// <param name="sentence">
        ///   The sentence to test.
        /// </param>
        /// <param name="rejectExpression">
        ///   The expression which will reject sentences that match it.
        /// </param>
        /// <returns><c>true</c> if the sentence is suitable for the model, <c>false</c> otherwise.</returns>
        static bool TestSentenceInput(string sentence, Regex rejectExpression)
        {
            if (sentence.Trim().Length == 0)
                return false;

            return rejectExpression == null || !rejectExpression.IsMatch(sentence);
        }

        static readonly Regex DefaultRejectExpression = new Regex(
            @"(^')|('$)|\s'|'\s|[""(\(\)\[\])]", RegexOptions.Compiled);
    }
}
