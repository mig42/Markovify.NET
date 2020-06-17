using System;

namespace Markovify.NET.Tests
{
    class CustomRandom : IRandom
    {
        internal CustomRandom(int seed)
        {
            _random = new Random(seed);
        }

        public int NextNatural(int max)
        {
            return _random.Next(max);
        }

        readonly Random _random;
    }
}
