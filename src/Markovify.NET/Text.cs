using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
    class Text
    {
        public Text(
            string inputText,
            int stateSize = 2,
            Chain chain = null,
            IEnumerable<IEnumerable<string>> parsedSentences = null,
            bool retainOriginal = false,
            bool isWellFormed = true,
            Regex rejectExpression = null)
        {
            mbIsWellFormed = isWellFormed;
            mRejectExpression = mbIsWellFormed && rejectExpression != null
                ? rejectExpression
                : new Regex(@"(^')|('$)|\s'|'\s|[""(\(\)\[\])]", RegexOptions.Compiled);

            bool canMakeSentences = parsedSentences != null || !string.IsNullOrEmpty(inputText);
            mbRetainOriginal = retainOriginal && canMakeSentences;
            mbStateSize = stateSize;

            if (mbRetainOriginal)
            {
                mParsedSentences = parsedSentences ?? GenerateCorpus(inputText);

                // Rejoined text lets us assess the novelty of generated sentences
                mRejoinedText = string.Join(
                    " ", mParsedSentences.Select(words => string.Join(" ", words)));
                mChain = chain ?? new Chain(mParsedSentences, stateSize);
                return;
            }

            mChain = chain ?? new Chain(
                parsedSentences ?? GenerateCorpus(inputText), stateSize);
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
                mChain.Compile(true);
                return this;
            }

            return new Text(
                null,
                mbStateSize,
                mChain.Compile(false),
                mParsedSentences,
                mbRetainOriginal,
                mbIsWellFormed,
                mRejectExpression);
        }

        /// <summary>
        ///   Given a text string, returns a list of lists. That is, a list of "sentences",
        ///   each of which is a list of words. Before splitting into words, the sentences
        ///   are filtered through <see cref="TestSentenceInput" />
        /// </summary>
        /// <param name="inputText">The sourcce text</param>
        /// <returns>The corpus as a list of lists.</returns>
        IEnumerable<IEnumerable<string>> GenerateCorpus(string inputText)
        {
            return Split.IntoSentences(inputText)
                .Where(TestSentenceInput)
                .Select(sentence => mWordSplitPattern.Split(sentence));
        }

        /// <summary>
        ///   A basic sentence filter. The default rejects sentences that contain
        ///   the type of punctuation that would look strange on its own in a randomly
        ///   generated sentence.
        /// </summary>
        /// <param name="sentence">The sentence to test.</param>
        /// <returns><c>true</c> if the sentence is suitable for the model, <c>false</c> otherwise.</returns>
        bool TestSentenceInput(string sentence)
        {
            if (sentence.Trim().Length == 0)
                return false;

            return !mbIsWellFormed || !mRejectExpression.IsMatch(sentence);
        }

        readonly bool mbIsWellFormed;
        readonly Regex mRejectExpression;
        readonly bool mbRetainOriginal;
        readonly int mbStateSize;
        readonly IEnumerable<IEnumerable<string>> mParsedSentences;
        readonly string mRejoinedText;
        readonly Chain mChain;

        static readonly Regex mWordSplitPattern = new Regex(
            @"\s+", RegexOptions.Compiled);
    }
}
