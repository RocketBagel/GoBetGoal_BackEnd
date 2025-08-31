using GoBetGoal_BackEnd.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class TrialResultInfoDto
    {
        public int TrialId { get; set; }
        public List<string> TrialCategory { get; set; }
        public string TrialName { get; set; }
        public string TrialTitle { get; set; }
        public string TrialDescription { get; set; }
        public int TrialFrequency { get; set; }
        public int StageCount { get; set; }
        public int TotalDays { get; set; } // 試煉總天數
        public int TotalParticipants { get; set; } // 試煉總人數
        public Status TrialStatus { get; set; }
        public DateTime EndTime { get; set; }

    }
}