using System.IO;

namespace Markovify.NET.Tests
{
    static class Data
    {
        internal static string GetSherlock()
        {
            return s_sherlock ??= File.ReadAllText(Files.Sherlock);
        }

        internal static string GetSingleTransitionsOnly()
        {
            return s_singleTransitionsOnly ??= File.ReadAllText(Files.SingleTransitionsOnly);
        }

        static class Files
        {
            internal static readonly string SingleTransitionsOnly = Path.Combine(
                "texts", "single_transitions_only.txt");

            internal static readonly string Sherlock = Path.Combine(
                "texts", "sherlock.txt");
        }

        static string? s_sherlock;
        static string? s_singleTransitionsOnly;
    }
}
