using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoBetGoal_BackEnd.Models
{
    public class Stage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StageIndex { get; set; }

        //[Required]
        [StringLength(50)]
        public string VerificationMode { get; set; }

        [StringLength(50)]
        public string ChallengeType { get; set; }


        [StringLength(500)]
        public string StageSampleImagePath { get; set; }
        
        [Required]
        [StringLength(300)]
        public string StageDescription { get; set; }

        //[Required]
        //[StringLength(200)]
        //public string StageTitle { get; set; }

        //[Required]
        //[StringLength(200)]
        //public string StageContent { get; set; }

        [Required]
        public int TrialTemplateId { get; set; }
        public virtual TrialTemplate TrialTemplate { get; set; }

        //[Required]
        //public bool IsActive { get; set; } = true;

        //[Required]
        //[Column(TypeName = "datetime2")]
        //public DateTime CreatedAt { get; set; } = DateTime.Now;


        public virtual ICollection<UserStage> UserStages { get; set; } = new List<UserStage>();
    }
}