namespace GoldRush.Core.SequenceNumbers
{
    public class SequenceNumberGenerator
    {
        public SequenceNumberGenerator() { }

        private int resetValue = 0;
        public int StartingValue { get; set; } = 0;
        private int lastValue;
        public int LastValue => lastValue;
        public int EndCyclePosition { get; set; }
        public bool CycleSequence { get; set; }
        public string Prefix { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        public int LeftPadding { get; set; } = 0;
        public int RightPadding { get; set; } = 0;
        public char LeftPadChar { get; set; } = '0';
        public char RightPadChar { get; set; } = '0';

        public ISequenceIncrementer SequenceIncrementer { get; set; }
        public ISequenceResetCondition ResetCondition { get; set; }
        public int ResetValue { get => resetValue; }

        #region Chained Properties
        public SequenceNumberGenerator SetSequencer(ISequenceIncrementer sequenceIncrementer)
        {
            SequenceIncrementer = sequenceIncrementer;
            return this;
        }

        public SequenceNumberGenerator SetResetCondition(ISequenceResetCondition condition)
        {
            ResetCondition = condition;
            return this;
        }

        public SequenceNumberGenerator SetEndCyclePosition(int position, bool cycle = true)
        {
            CycleSequence = cycle;
            EndCyclePosition = position;
            return this;
        }

        public SequenceNumberGenerator SetPrefix(string prefix)
        {
            Prefix = prefix;
            return this;
        }

        public SequenceNumberGenerator SetSuffix(string suffix)
        {
            Suffix = suffix;
            return this;
        }

        public SequenceNumberGenerator SetRightPadding(int pad, char padChar = '0')
        {
            RightPadding = pad;
            RightPadChar = padChar;
            return this;
        }

        public SequenceNumberGenerator SetLeftPadding(int pad, char padChar = '0')
        {
            LeftPadding = pad;
            LeftPadChar = padChar;
            return this;
        }

        public SequenceNumberGenerator SetStartingValue(int startingValue)
        {
            StartingValue = startingValue;
            lastValue = startingValue;
            return this;
        }

        public SequenceNumberGenerator SetResetValue(int resetValue)
        {
            this.resetValue = resetValue;
            return this;
        }

        #endregion

        public int NextSequenceValue()
        {
            var sequenceValue = StartingValue;

            if (SequenceIncrementer != null)
                sequenceValue = SequenceIncrementer.Increment(sequenceValue);
            else
                sequenceValue++;

            if (CycleSequence)
            {
                if (ResetCondition != null)
                {
                    sequenceValue = ResetCondition.Reset(sequenceValue, EndCyclePosition, ResetValue);
                }
                else
                {
                    if (sequenceValue > EndCyclePosition)
                        sequenceValue = ResetValue;
                }
            }

            return sequenceValue;
        }

        private string ValueToSequenceNumber(int value)
        {
            var padded = value
                .ToString()
                .PadLeft(LeftPadding, LeftPadChar)
                .PadRight(RightPadding+LeftPadding-1, RightPadChar);
            return $"{Prefix}{padded}{Suffix}";
        }

        public string GenerateSequenceNumber()
        {
            var sequence = ValueToSequenceNumber(StartingValue);
            lastValue = StartingValue;
            StartingValue = NextSequenceValue();
            return sequence;
        }

        public string NextSequenceNumber()
        {
            var sequence = ValueToSequenceNumber(StartingValue);
            return sequence;
        }

        public string LastSequenceNumber()
        {
            var sequence = ValueToSequenceNumber(lastValue);
            return sequence;
        }
    }
}
