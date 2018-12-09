using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldRush.Core.Models.Common
{
    public abstract class IntBaseEntity : IBaseEntity<int>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public byte[] ConcurrencyStamp { get; set; }
        public DateTime? ConcurrencyTimeStamp { get; set; }
    }
}