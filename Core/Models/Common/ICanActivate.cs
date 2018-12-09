using System.ComponentModel;

namespace GoldRush.Core.Models.Common
{
    public interface ICanActivate
    {
        [DefaultValue(true)]
        bool Active { get; set; }
    }
}