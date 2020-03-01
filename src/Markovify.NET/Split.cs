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
            IEnumerable<int> endIndices = mEndPatterns.Matches(inputText)
                .OfType<Match>()
                .Where(match => IsSentenceEnder(match.Groups[1].Value))
                .Select(match => match.Index + match.Groups[1].Length + match.Groups[2].Length);

            return endIndices.Prepend(0).Zip(
                endIndices.Append(inputText.Length),
                (start, end) => inputText.Substring(start, end - start));
        }

        static bool IsSentenceEnder(string word)
        {
            if (mExceptions.Contains(word))
                return false;

            char lastChar = word.Last();
            if (lastChar == '?' || lastChar == '!')
                return true;

            if (mNotStartUppercase.Replace(word, string.Empty).Length > 1)
                return true;

            return lastChar == '.' && !IsAbbreviation(word);
        }

        static bool IsAbbreviation(string word)
        {
            string clipped = word.Substring(0, word.Length - 1);
            if (ASCII_UPPERCASE.Contains(clipped[0]))
                return mAbbrCapped.Contains(clipped.ToLowerInvariant());

            return mAbbrLowercase.Contains(clipped);
        }

        static readonly Regex mEndPatterns = new Regex(
            string.Concat(
                @"([\w\.'’&\]\)]+[\.\?!])", // a word that ends with punctuation
                @"([‘’“”'""\)\]]*)", // Followed by optional quote/parens/etc
                @"(\s+(?![a-z\-–—]))" // Followed by whitespace + non-(lowercase or dash)
            ),
            RegexOptions.Compiled);

        static readonly Regex mNotStartUppercase = new Regex(@"[^A-Z]", RegexOptions.Compiled);

        static HashSet<string> mExceptions = new HashSet<string>
        {
            "U.S.",
            "U.N.",
            "E.U.",
            "F.B.I.",
            "C.I.A."
        };

        const string ASCII_LOWERCASE = "abcdefghijklmnnopqrstuvwxyz";
        static readonly string ASCII_UPPERCASE = ASCII_LOWERCASE.ToUpperInvariant();

        static readonly HashSet<string> mAbbrLowercase = new HashSet<string>
        {
            "etc", "v", "vs", "viz", "al", "pct"
        };

        // States w/ with thanks to https://github.com/unitedstates/python-us
        const string STATES =
            "ala|ariz|ark|calif|colo|conn|del|fla|ga|ill|ind|kan|ky|la|md|mass|mich|minn|miss|mo|mont|neb|nev|okla|ore|pa|tenn|vt|va|wash|wis|wyo";
        // Titles w/ thanks to https://github.com/nytimes/emphasis and @donohoe
        const string TITLES =
            "mr|ms|mrs|msr|dr|gov|pres|sen|sens|rep|reps|prof|gen|messrs|col|sr|jf|sgt|mgr|fr|rev|jr|snr|atty|supt";
        const string STREETS = "ave|blvd|st|rd|hwy";
        const string MONTHS = "jan|feb|mar|apr|jun|jul|aug|sep|sept|oct|nov|dec";
        static readonly HashSet<string> mAbbrCapped = new HashSet<string>(
            string.Join("|", STATES, TITLES, STREETS, MONTHS).Split('|')
        );
    }
}
