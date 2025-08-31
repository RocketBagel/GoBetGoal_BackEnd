using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public PublicUserProfileDto UserInfo { get; set; }
        public int CompleteStageCount { get; set; }
        public int CheatBlanketUsedCount { get; set; }

    }
}