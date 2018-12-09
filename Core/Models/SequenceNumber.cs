using System.Collections.Generic;
using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Models
{
    public class SequenceNumber : GuidCompanyIntBaseEntity
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public int StartingValue { get; set; }
        public int ResetValue { get; set; }
        public int LeftPadding { get; set; }
        public int RightPadding { get; set; }
        public char LeftPaddingChar { get; set; }
        public char RightPaddingChar { get; set; }
        public bool CycleSequence { get; set; }
        public int EndCyclePosition { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
    }
}