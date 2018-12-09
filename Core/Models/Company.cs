using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Models
{
    public class Company : GuidCompanyGuidBaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}