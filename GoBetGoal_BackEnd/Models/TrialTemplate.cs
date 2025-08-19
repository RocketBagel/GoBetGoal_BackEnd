using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoBetGoal_BackEnd.Models
{
    public class TrialTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TrialTitle { get; set; }

        [Required]
        [StringLength(200)]
        public string TrialDescription { get; set; }

        [Required]
        public int TrialFrequency { get; set; }

        [Required]
        [StringLength(50)]
        public string TrialCategory { get; set; }

        [Required]
        [StringLength(100)]
        public string TrialSuitFor { get; set; }

        [Required]
        [StringLength(100)]
        public string TrialNoSuitFor { get; set; }

        [Required]
        [StringLength(200)]
        public string TrialRule { get; set; }

        [Required]
        [StringLength(200)]
        public string TrialCaution { get; set; }

        [Required]
        [StringLength(200)]
        public string TrialEffect { get; set; }

        [Required]
        public int StageCount { get; set; }

        [Required]
        public int MaxUser {  get; set; }

        [Required]
        public bool IsAi { get; set; }

        [Required]
        public int TrialTemplatePrice { get; set; }

        //public ProductType ProductType { get; set; }

        [Required]
        [StringLength(200)]
        public string CardImagePath { get; set; }

        [Required]
        [StringLength(50)]
        public string CardColor { get; set; }

        //[Required]
        //public bool IsActive { get; set; } = true;

        //[Column(TypeName = "datetime2")]
        //public DateTime CreatedAt { get; set; }

        public virtual ICollection<Stage> Stages { get; set; } = new List<Stage>();
        public virtual ICollection<Trial> Trials { get; set; }=new List<Trial>();
        public virtual ICollection<UserTrialTemplate> UserTrialTemplates { get; set; } = new List<UserTrialTemplate>();



    }
}