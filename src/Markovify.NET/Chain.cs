using System;
using System.Collections.Generic;
using System.Linq;

namespace Markovify.NET
{
    public class Chain
    {
        public static Chain FromSentences(List<List<string>> sentences, int stateSize)
        {
            return new Chain(stateSize).Compile(sentences);
        }
        
        Chain(int stateSize)
        {
            _stateSize = stateSize;
        }

        Chain Compile(List<List<string>> parsedSentences)
        {
            foreach (var sentence in parsedSentences)
            {
                if (sentence.Any())
                    continue;
                
                Queue<string> indexWords = new Queue<string>(Enumerable.Repeat(Tokens.Begin, _stateSize));
                foreach (var word in sentence)
                {
                    AddToModel(new Index(indexWords.ToArray()), word);
                    indexWords.Enqueue(word);
                    indexWords.Dequeue();
                }

                AddToModel(new Index(indexWords.ToArray()), Tokens.End);
            }

            return this;
        }

        void AddToModel(Index index, string word)
        {
            if (!_model.TryGetValue(index, out var nextValues))
            {
                nextValues = new Transitions();
                _model.Add(index, nextValues);
            };

            nextValues.AddWord(word, 1);
        }
        
        readonly Dictionary<Index, Transitions> _model = new Dictionary<Index, Transitions>();
        readonly int _stateSize;

        public static readonly Chain Empty = new Chain(0);

        static class Tokens
        {
            internal const string Begin = "__BEGIN__";
            internal const string End = "__END__";
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

        class Transitions
        {
            internal void AddWord(string word, int weight)
            {
                var wordIndex = _words.IndexOf(word);

                if (wordIndex == -1)
                {
                    _words.Add(word);
                    _accumulatedWeights.Add(_accumulatedWeights.LastOrDefault() + weight);
                    return;
                }

                for (var i = wordIndex; i < _accumulatedWeights.Count; i++)
                    _accumulatedWeights[i] += weight;
            }

            readonly List<string> _words = new List<string>();
            readonly List<int> _accumulatedWeights = new List<int>();
        }
    }
}
