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
            [Fact]
            public void SampleTextAndDefaultOptions_ShouldReturnSentence()
            {
                var text = Text.Build(Data.GetSherlock());

                text.Should().NotBe(Text.Empty);

                var sentence = text.MakeSentence(new MakeSentenceOptions());
                sentence.Should().NotBeNullOrEmpty();
                sentence!.Remove(1).Should().NotBeNullOrWhiteSpace();
                sentence.Substring(sentence.Length - 1).Should().NotBeNullOrWhiteSpace();
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
