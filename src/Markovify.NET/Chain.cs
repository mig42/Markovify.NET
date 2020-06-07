using System;
using System.Collections.Generic;
using System.Linq;

namespace Markovify.NET
{
    public class Chain
    {
        public int ModelSize => _model.Count;

        public static Chain FromSentences(List<List<string>> sentences, int stateSize)
        {
            if (sentences == null ||
                sentences.Count == 0 ||
                stateSize < 1 ||
                !sentences.Any(words => words.Any()))
            {
                return Empty;
            }
            return new Chain(stateSize).Compile(sentences);
        }

        Chain(int stateSize)
        {
            _stateSize = stateSize;
        }

        public List<string> GenerateSentence()
        {
            Queue<string> state = new Queue<string>(Enumerable.Repeat(Tokens.Begin, _stateSize));
            List<string> result = new List<string>();
            while (true)
            {
                string nextWord = GetNextWord(state);
                if (nextWord == Tokens.End)
                    return result;

                state.Dequeue();
                state.Enqueue(nextWord);
                result.Add(nextWord);
            }
        }

        string GetNextWord(IEnumerable<string> fromWords)
        {
            Index index = new Index(fromWords.ToArray());
            return _model.TryGetValue(index, out var transitions)
                ? transitions.GetRandom()
                : Tokens.End;
        }

        Chain Compile(List<List<string>> parsedSentences)
        {
            string[] start = Enumerable.Repeat(Tokens.Begin, _stateSize).ToArray();
            if (!parsedSentences.Any())
            {
                AddToModel(new Index(start), Tokens.End);
                return this;
            }

            foreach (var sentence in parsedSentences)
            {
                if (!sentence.Any())
                    continue;

                Queue<string> indexWords = new Queue<string>(start);
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
            }

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
                return _words.Length == other._words.Length && _words.SequenceEqual(other._words);
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

            public string GetRandom()
            {
                Random random = new Random();
                var targetWeight = random.Next(1, _accumulatedWeights.LastOrDefault());
                var index = _accumulatedWeights.FindIndex(accWeight => targetWeight <= accWeight);

                return index != -1
                    ? _words[index]
                    : Tokens.End;
            }

            readonly List<string> _words = new List<string>();
            readonly List<int> _accumulatedWeights = new List<int>();
        }
    }
}
