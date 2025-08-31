using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class MyPersonalResultDto
    {
        public PublicUserProfileDto MyUserInfo {  get; set; }
        public int MyCompleteStageCount { get; set; }
        public int MyCheatBlanketUsedCount { get; set; }
        public List<AchievementDto> MyAchievements { get; set; }
        public List<string> MyAllApprovedPhotos { get; set; }
        public int RewardAmount { get; set; }

    }
}