using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Markovify.NET.Tests
{
    public static class ChainTests
    {
        public class Build
        {
            [Theory]
            [InlineData(-2)]
            [InlineData(-1)]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(2)]
            public void EmptyList_ShouldReturnEmpty(int stateSize)
            {
                var chain = Chain.FromSentences(new List<List<string>>(), stateSize);
                chain.Should().Be(Chain.Empty);
                chain.ModelSize.Should().Be(0);
            }

            [Theory]
            [InlineData(-2)]
            [InlineData(-1)]
            [InlineData(0)]
            public void StateSizeLessThan1_ShouldReturnEmpty(int stateSize)
            {
                var corpus = new List<List<string>> { new List<string> { "Sample", "sentence." } };
                var chain = Chain.FromSentences(corpus, stateSize);

                chain.Should().Be(Chain.Empty);
                chain.ModelSize.Should().Be(0);
            }

            [Theory]
            [InlineData(1)]
            [InlineData(2)]
            public void ValidData_ShouldReturnValidChain(int stateSize)
            {
                var corpus = new List<List<string>> { new List<string> { "Sample", "sentence." } };
                var chain = Chain.FromSentences(corpus, stateSize);

                chain.Should().NotBe(Chain.Empty);
                chain.ModelSize.Should().Be(3);
            }
        }

        public class GenerateSentence
        {
            [Theory]
            [InlineData(1)]
            [InlineData(2)]
            [InlineData(3)]
            public void SingleSentence_ShouldReturnTheSame(int stateSize)
            {
                List<string> expectedSentence = new List<string> {"This", "is", "a", "sentence."};
                Chain chain = Chain.FromSentences(
                    new List<List<string>> { expectedSentence },
                    stateSize);

                List<string> actualSentence = chain.GenerateSentence();

                chain.ModelSize.Should().Be(expectedSentence.Count + 1);
                actualSentence.Should().ContainInOrder(expectedSentence);
            }

            [Theory]
            [InlineData(3, "This is a sentence.")]
            [InlineData(100, "This is a sample.")]
            public void TwoSentencesWithDifferentLastWord_ShouldBuildRandom(int seed, string expected)
            {
                List<string> sentence1 = new List<string> {"This", "is", "a", "sentence."};
                List<string> sentence2 = sentence1.SkipLast(1).Append("sample.").ToList();

                Chain chain = Chain.FromSentences(
                    new List<List<string>> { sentence1, sentence2 },
                    1);

                chain.SetRandom(new CustomRandom(seed));
                string actual = string.Join(' ', chain.GenerateSentence());

                chain.ModelSize.Should().Be(sentence1.Count + 2);
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(3, "This is a sentence.")]
            [InlineData(100, "That is a sentence.")]
            public void TwoSentencesWithDifferentFirstWord_ShouldBuildRandom(int seed, string expected)
            {
                List<string> sentence1 = new List<string> {"This", "is", "a", "sentence."};
                List<string> sentence2 =
                    Enumerable.Repeat("That", 1).Concat(sentence1.Skip(1)).ToList();

                Chain chain = Chain.FromSentences(
                    new List<List<string>> { sentence1, sentence2 },
                    1);

                chain.SetRandom(new CustomRandom(seed));
                string actual = string.Join(' ', chain.GenerateSentence());

                chain.ModelSize.Should().Be(sentence1.Count + 2);
                actual.Should().Be(expected);
            }
        }
    }
}
