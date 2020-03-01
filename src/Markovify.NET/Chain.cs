using System;
using System.Collections.Generic;

namespace Markovify.NET
{
    public class Chain
    {
        public Chain(IEnumerable<IEnumerable<string>> parsedSentences, int stateSize)
        {
            mParsedSentences = parsedSentences;
            mStateSize = stateSize;
        }

        internal Chain Compile(bool inPlace)
        {
            throw new NotImplementedException();
        }

        readonly IEnumerable<IEnumerable<string>> mParsedSentences;
        readonly int mStateSize;
    }
}
