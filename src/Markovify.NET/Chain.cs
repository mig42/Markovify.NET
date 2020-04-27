using System;
using System.Collections.Generic;
using System.Linq;

namespace Markovify.NET
{
    public class Chain
    {
        public static Chain FromSentences(IEnumerable<IEnumerable<string>> sentences, int stateSize)
        {
            return new Chain(stateSize).Compile(sentences);
        }
        
        Chain(int stateSize)
        {
            _stateSize = stateSize;
        }

        Chain Compile(IEnumerable<IEnumerable<string>> parsedSentences)
        {
            foreach (var sentence in parsedSentences)
            {
                Queue<string> indexWords = new Queue<string>(Enumerable.Repeat(Tokens.Begin, _stateSize));
                foreach (var word in sentence)
                {
                    Index index = new Index(indexWords.ToArray());
                    if (!_model.TryGetValue(index, out var nextValues))
                    {
                        nextValues = new HashSet<string>();
                        _model.Add(index, nextValues);
                    };
                    nextValues.Add(word);
                }
            }

            return this;
        }

        readonly Dictionary<Index, HashSet<string>> _model = new Dictionary<Index, HashSet<string>>();
        readonly int _stateSize;

        public static readonly Chain Empty = new Chain(0);

        static class Tokens
        {
            internal const string Begin = "_BEGIN_";
            internal const string End = "_END_";
        }

        class Index : IEquatable<Index>
        {
            internal Index(string[] words)
            {
                _words = words ?? new string[0];

                if (_words.Length == 0)
                {
                    _hashCode = EmptyHashCode;
                    return;
                }

                _hashCode = _words.Aggregate(
                    _hashCode,
                    (accumulate, current) => accumulate ^ current.GetHashCode());
            }
            
            bool IEquatable<Index>.Equals(Index other)
            {
                if (_words.Length != other._words.Length)
                    return false;

                return _words.SequenceEqual(other._words);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            readonly string[] _words;
            readonly int _hashCode;

            const int EmptyHashCode = -1;
        }
    }
}
