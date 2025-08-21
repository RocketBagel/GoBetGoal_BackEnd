using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class UserAchievement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int AchievementId { get; set; }
        public virtual Achievement Achievement { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime AcquiredAt { get; set; } = DateTime.Now;

        public int? TrialId { get; set; }
        public virtual Trial Trial { get; set; }

    }
}