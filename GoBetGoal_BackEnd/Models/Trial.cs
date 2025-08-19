using GoBetGoal_BackEnd.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class Trial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int TrialTemplateId { get; set; }
        public virtual TrialTemplate TrialTemplate { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime StartTime { get; set; }


        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime EndTime { get; set; }


        [Required]
        public int TrialDeposit { get; set; }

        [Required]
        [StringLength(100)]
        public string TrialName { get; set; }

        [Required]
        public Status TrialStatus { get; set; } = 0; //預設: pending

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<UserStage> UserStages { get; set; } = new List<UserStage>();

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        public virtual ICollection<TrialLike> TrialLikes { get; set; } = new List<TrialLike>();


        //public virtual ICollection<UserTrial> UserTrials { get; set; } = new List<UserTrial>();


        //public virtual ICollection<TrialWatcher> TrialWatchers { get; set; } = new List<TrialWatcher>();
    }
}