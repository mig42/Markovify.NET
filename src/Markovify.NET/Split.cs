using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Markovify.NET
{
    static class Split
    {
        internal static IEnumerable<string> IntoSentences(string inputText)
        {
            IEnumerable<int> endIndices = RegularExpressions.EndPatterns.Matches(inputText)
                .OfType<Match>()
                .Where(match => IsSentenceEnder(match.Groups[1].Value))
                .Select(match => match.Index + match.Groups[1].Length + match.Groups[2].Length);

            return endIndices.Prepend(0).Zip(
                endIndices.Append(inputText.Length),
                (start, end) => inputText.Substring(start, end - start));
        }

        internal static IEnumerable<string> IntoWords(string sentence)
        {
            return RegularExpressions.WordSplit.Split(sentence);
        }

        static bool IsSentenceEnder(string word)
        {
            if (Abbreviations.Exceptions.Contains(word))
                return false;

            char lastChar = word.Last();
            if (lastChar == '?' || lastChar == '!')
                return true;

            if (RegularExpressions.NotStartUppercase.Replace(word, string.Empty).Length > 1)
                return true;

            return lastChar == '.' && !IsAbbreviation(word);
        }

        static bool IsAbbreviation(string word)
        {
            string clipped = word.Substring(0, word.Length - 1);
            if (Abbreviations.AsciiUppercase.Contains(clipped[0]))
                return Abbreviations.Capped.Contains(clipped.ToLowerInvariant());

            return Abbreviations.Lowercase.Contains(clipped);
        }

        static class RegularExpressions
        {
            internal static readonly Regex EndPatterns = new Regex(
                string.Concat(
                    @"([\w\.'’&\]\)]+[\.\?!])", // a word that ends with punctuation
                    @"([‘’“”'""\)\]]*)", // Followed by optional quote/parens/etc
                    @"(\s+(?![a-z\-–—]))" // Followed by whitespace + non-(lowercase or dash)
                ),
                RegexOptions.Compiled);

            internal static readonly Regex NotStartUppercase = new Regex(
                @"[^A-Z]", RegexOptions.Compiled);

            internal static readonly Regex WordSplit = new Regex(
                @"\s+", RegexOptions.Compiled);
        }

        static class Abbreviations
        {
            internal static HashSet<string> Exceptions = new HashSet<string>
            {
                "U.S.",
                "U.N.",
                "E.U.",
                "F.B.I.",
                "C.I.A."
            };

            internal const string AsciiLowercase = "abcdefghijklmnnopqrstuvwxyz";
            internal static readonly string AsciiUppercase = AsciiLowercase.ToUpperInvariant();

            internal static readonly HashSet<string> Lowercase = new HashSet<string>
            {
                "etc", "v", "vs", "viz", "al", "pct"
            };

            internal static readonly HashSet<string> Capped = new HashSet<string>(
                string.Join("|", STATES, TITLES, STREETS, MONTHS).Split('|')
            );

            // States w/ with thanks to https://github.com/unitedstates/python-us
            const string STATES =
                "ala|ariz|ark|calif|colo|conn|del|fla|ga|ill|ind|kan|ky|la|md|mass|mich|minn|miss|mo|mont|neb|nev|okla|ore|pa|tenn|vt|va|wash|wis|wyo";
            // Titles w/ thanks to https://github.com/nytimes/emphasis and @donohoe
            const string TITLES =
                "mr|ms|mrs|msr|dr|gov|pres|sen|sens|rep|reps|prof|gen|messrs|col|sr|jf|sgt|mgr|fr|rev|jr|snr|atty|supt";
            const string STREETS = "ave|blvd|st|rd|hwy";
            const string MONTHS = "jan|feb|mar|apr|jun|jul|aug|sep|sept|oct|nov|dec";
        }
    }
}
