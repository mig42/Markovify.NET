using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Markovify.NET.Tests
{
    public class ChainTests
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
                var text = Chain.FromSentences(new List<List<string>>(), stateSize);
                text.Should().Be(Text.Empty);
            }

            [Theory]
            [InlineData(-2)]
            [InlineData(-1)]
            [InlineData(0)]
            public void StateSizeLessThan1_ShouldReturnEmpty(int stateSize)
            {
                var corpus = new List<List<string>> { new List<string> { "Sample", "sentence." } };
                var text = Chain.FromSentences(corpus, stateSize);

                text.Should().Be(Text.Empty);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(2)]
            public void ValidData_ShouldReturnValidChain(int stateSize)
            {
                var corpus = new List<List<string>> { new List<string> { "Sample", "sentence." } };
                var text = Chain.FromSentences(corpus, stateSize);

                text.Should().NotBe(Text.Empty);
            }
        }

        public class GenerateSentence
        {
        }
    }
}
