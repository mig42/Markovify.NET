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
    public class Text
    {
        [JsonProperty("chain")]
        public Chain Chain { get; set; }

        [JsonProperty("sourceText")]
        public string SourceText { get; set; }

        [JsonProperty("isWellFormed")]
        public bool IsWellFormed { get; set; }

        [JsonProperty("rejectExpression")]
        public Regex RejectExpression { get; set; } = DefaultRejectExpression;

        /// <summary>
        /// Build the text from its source.
        /// </summary>
        /// <param name="inputText">
        ///   The source text.
        /// </param>
        /// <param name="stateSize">
        ///   The number of words in the model's state
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
        /// <returns></returns>
        public static Text Build(
            string inputText,
            int stateSize = 2,
            bool retainOriginal = true,
            bool isWellFormed = true,
            Regex rejectExpression = null)
        {
            if (stateSize < 1)
                return Empty;
            
            if (rejectExpression == null && isWellFormed)
                rejectExpression = DefaultRejectExpression;

            var corpus = GenerateCorpus(
                inputText, isWellFormed ? rejectExpression : null);

            var parsedSentences = corpus.ToList();
            return new Text
            {
                Chain = Chain.FromSentences(parsedSentences, stateSize),
                IsWellFormed = isWellFormed,
                RejectExpression = rejectExpression,
                SourceText = retainOriginal && parsedSentences.Any() && !string.IsNullOrEmpty(inputText)
                    ? string.Join(" ", parsedSentences.Select(words => string.Join(" ", words)))
                    : string.Empty
            };
        }

        /// <summary>
        ///   Given a text string, returns a list of lists. That is, a list of "sentences",
        ///   each of which is a list of words. Before splitting into words, the sentences
        ///   are filtered through <see cref="TestSentenceInput" />
        /// </summary>
        /// <param name="inputText">The source text</param>
        /// <param name="rejectExpression">The expression to reject</param>
        /// <returns>The corpus as a list of lists.</returns>
        static List<List<string>> GenerateCorpus(
            string inputText, Regex rejectExpression)
        {
            return Split.IntoSentences(inputText)
                .Where(sentence => TestSentenceInput(sentence, rejectExpression))
                .Select(Split.IntoWords)
                .ToList();
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

        static readonly Text Empty = new Text
        {
            Chain = Chain.Empty,
            RejectExpression = DefaultRejectExpression,
            SourceText = string.Empty
        };
    }
}
