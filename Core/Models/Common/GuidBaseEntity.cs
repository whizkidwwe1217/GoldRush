using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldRush.Core.Models.Common
{
    public abstract class GuidBaseEntity : IBaseEntity<Guid>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public byte[] ConcurrencyStamp { get; set; }
        public DateTime? ConcurrencyTimeStamp { get; set; }
    }
}