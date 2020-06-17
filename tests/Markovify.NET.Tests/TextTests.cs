using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Markovify.NET.Tests
{
    public class TextTests
    {
        public class Build
        {
            [Fact]
            public void EmptyText_ShouldReturnEmpty()
            {
                var text = Text.Build(string.Empty);
                text.Should().Be(Text.Empty);
            }

            [Theory]
            [InlineData(-2)]
            [InlineData(-1)]
            [InlineData(0)]
            public void StateSizeLessThan1_ShouldReturnEmpty(int stateSize)
            {
                var text = Text.Build("Sample sentence.", stateSize);
                text.Should().Be(Text.Empty);
            }

            [Fact]
            public void StandardBuild_ShouldHaveCorrectSourceAndDefaultReject()
            {
                var text = Text.Build("Sample sentence.");
                text.Should().NotBe(Text.Empty);
                text.RejectExpression.Should().Be(Text.DefaultRejectExpression);
                text.SourceText.Should().Be("Sample sentence.");
                text.IsWellFormed.Should().BeTrue();
            }

            [Fact]
            public void RejectExpression_ShouldBeSet()
            {
                Regex regex = new Regex(".*");
                var text = Text.Build("Sample sentence.", rejectExpression: regex);
                text.Should().NotBe(Text.Empty);
                text.RejectExpression.Should().Be(regex);
                text.SourceText.Should().Be(string.Empty);
                text.IsWellFormed.Should().BeTrue();
            }

            [Fact]
            public void IsWellFormed_ShouldSkipReject()
            {
                Regex regex = new Regex(".*");
                var text = Text.Build("Sample sentence.", rejectExpression: regex, isWellFormed: false);
                text.Should().NotBe(Text.Empty);
                text.RejectExpression.Should().Be(regex);
                text.SourceText.Should().Be("Sample sentence.");
                text.IsWellFormed.Should().BeFalse();
            }

            [Fact]
            public void RetainOriginal_ShouldAffectSourceText()
            {
                var text = Text.Build("Sample sentence.", retainOriginal: false);
                text.Should().NotBe(Text.Empty);
                text.RejectExpression.Should().Be(Text.DefaultRejectExpression);
                text.SourceText.Should().Be(string.Empty);
                text.IsWellFormed.Should().BeTrue();
            }
        }

        public class MakeSentence
        {
            public static IEnumerable<object[]> RandomSentencesData()
            {
                yield return new object[]
                {
                    23057,
                    "It was a cracked edge, where a few days I was informed by the sunlight; " +
                    "but since your shaving is less illuminated than the Assizes.",
                };
                yield return new object[]
                {
                    2782232,
                    "You must also put in the morning when the facts came out, it would be in " +
                    "his right shirt-sleeve, but he is right."
                };
                yield return new object[]
                {
                    83599,
                    "Ferguson appeared to be spent in an angle of the average story-teller."
                };
                yield return new object[]
                {
                    2020061721,
                    "I keep it only to be too limp to get to him to step in."
                };
                yield return new object[]
                {
                    883727773,
                    "I know I ought to be something out of the most difficult to part from them."
                };
            }

            [Theory]
            [MemberData(nameof(RandomSentencesData))]
            public void SampleTextAndDefaultOptions_ShouldReturnSentence(
                int seed, string expected)
            {
                var text = Text.Build(Data.GetSherlock());

                text.Should().NotBe(Text.Empty);

                text.Chain.SetRandom(new CustomRandom((seed)));

                var actual = text.MakeSentence(new MakeSentenceOptions());
                actual.Should().NotBeNullOrEmpty();
                actual.Should().NotStartWith(" ");
                actual.Should().NotEndWith(" ");
                actual.Should().Be(expected);
            }

            [Theory]
            [InlineData(10)]
            [InlineData(15)]
            [InlineData(20)]
            public void SampleTextWithWordLimit_ShouldReturnThatAmountOfWords(int wordCount)
            {
                var text = Text.Build(Data.GetSherlock());

                text.Should().NotBe(Text.Empty);

                var sentence = text.MakeSentence(new MakeSentenceOptions
                {
                    MaxWords = wordCount,
                    Tries = 50,
                });
                sentence.Should().NotBeNullOrEmpty();
                sentence!.Split(' ').Length.Should().BeLessOrEqualTo(wordCount);
            }

            [Fact]
            public void SimplisticText_ShouldNotMakeSentence()
            {
                var text = Text.Build(Data.GetSingleTransitionsOnly());
                text.Should().NotBe(Text.Empty);

                var sentence = text.MakeSentence(new MakeSentenceOptions());
                sentence.Should().BeNull();
            }

            [Fact]
            public void SimplisticTextWithoutTestOutput_ShouldMakeSentence()
            {
                var text = Text.Build(Data.GetSingleTransitionsOnly());
                text.Should().NotBe(Text.Empty);

                var sentence = text.MakeSentence(new MakeSentenceOptions
                {
                    TestOutput = false,
                    Tries = 1,
                });
                sentence.Should().NotBeNullOrEmpty();
            }
        }
    }
}
