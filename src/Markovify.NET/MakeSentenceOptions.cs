namespace Markovify.NET
{
    public class MakeSentenceOptions
    {
        public int Tries { get; set; } = 10;
        public double MaxOverlapRatio { get; set; } = 0.7;
        public int MaxOverlapWords { get; set; } = 15;
        public bool TestOutput { get; set; } = true;
        public int? MaxWords { get; set; } = null;
    }
}
