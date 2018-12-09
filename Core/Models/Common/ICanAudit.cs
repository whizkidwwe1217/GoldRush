using System;

namespace GoldRush.Core.Models.Common
{
    public interface ICanAudit
    {
        DateTime? DateCreated { get; set; }
        DateTime? DateModified { get; set; }
        DateTime? DateDeleted { get; set; }
    }
}