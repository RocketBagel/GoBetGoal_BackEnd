using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class TrialDetailDto
    {
        // 試煉本身的資訊
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string TrialStatus { get; set; }
        public int Deposit { get; set; }
        public Guid CreatorId { get; set; } // 創建者的 User ID

        // 試煉模板的資訊 (ChallengeSupa 對應的後端 DTO)
        public TrialTemplateInfoDto TrialTemplateInfo { get; set; }

        // 所有參與者的詳細資訊列表
        public List<TrialParticipantDto> Participants { get; set; }
    }
}