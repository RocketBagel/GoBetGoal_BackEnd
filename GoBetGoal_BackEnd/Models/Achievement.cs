using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class Achievement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AchievementTitle { get; set; }

        [Required]
        [StringLength(200)]
        public string AchievementImagePath { get; set; }

        [Required]
        [StringLength(200)]
        public string AchievementDescription { get; set; }

        [Required]
        public int SortOrder { get; set; }

       

        public virtual ICollection<UserAchievement> UserAchievements { get; set; }
    }
}