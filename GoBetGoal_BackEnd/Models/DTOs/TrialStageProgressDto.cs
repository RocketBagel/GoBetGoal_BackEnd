using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    // 代表某位使用者在某一關卡的進度
    public class TrialStageProgressDto
    {
        public int StageIndex { get; set; }
        public string Status { get; set; } // 例如 "pending", "completed"
        public List<string> UploadImageUrls { get; set; }

        public DateTime? UploadAt { get; set; }
        public int ChanceRemain { get; set; }
    }
}