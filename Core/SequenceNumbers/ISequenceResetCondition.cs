namespace GoldRush.Core.SequenceNumbers
{
    public interface ISequenceResetCondition
    {
        int Reset(int startingValue, int endCyclePosition, int resetValue);
    }
}
