namespace GoldRush.Core.SequenceNumbers
{
    public interface ISequenceIncrementer
    {
        int Increment(int startingValue);
    }
}
