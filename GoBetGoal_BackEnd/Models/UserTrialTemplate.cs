using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class UserTrialTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public int TrialTemplateId { get; set; }
        public virtual TrialTemplate TrialTemplate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime AcquiredAt { get; set; } = DateTime.Now;
    }
}