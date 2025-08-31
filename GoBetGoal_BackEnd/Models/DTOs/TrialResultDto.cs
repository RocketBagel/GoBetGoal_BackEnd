using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static GoBetGoal_BackEnd.Controllers.TrialController;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class TrialResultDto
    {
        public TrialResultInfoDto TrialInfo { get; set; }
        public List<LeaderboardEntryDto> Leaderboard { get; set; }
        public MyPersonalResultDto MyResult { get; set; }
    }
}