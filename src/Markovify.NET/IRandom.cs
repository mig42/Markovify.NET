using System;

namespace Markovify.NET
{
    public interface IRandom
    {
        int NextNatural(int max);
    }

    class DefaultRandom : IRandom
    {
        int IRandom.NextNatural(int max)
        {
            return _random.Next(max);
        }

        readonly Random _random = new Random();
    }
}
