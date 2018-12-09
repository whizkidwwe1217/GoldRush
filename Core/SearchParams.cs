namespace GoldRush.Core
{
    public class SearchParams
    {
        public int? CurrentPage { get; set; } = 1;
        public int? PageSize { get; set; } = 100;
        public string Filter { get; set; } = string.Empty;
        public string Sort { get; set; } = string.Empty;
        public string Fields { get; set; } = string.Empty;
    }
}
