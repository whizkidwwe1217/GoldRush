namespace GoldRush.Core.Audit
{
    public class AuditDelta
    {
        public string PropertyName { get; set; }
        public string PreviousValue { get; set; }
        public string CurrentValue { get; set; }
    }
}