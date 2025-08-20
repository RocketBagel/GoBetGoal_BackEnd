using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    // 代表一位參與者，以及他的所有關卡進度
    public class TrialParticipantDto
    {
        // 參與者的基本公開資訊 (我們可以複用之前建立的 UserProfileDto，或建立一個新的)
        public UserProfileDto UserInfo { get; set; }
        public DateTime JoinedAt { get; set; } // 加入試煉的時間，用於排序

        public int PassCount { get; set; }
        public int CheatBlanketCount { get; set; }
        
        public int FailCount{ get; set; }

        // 這位參與者所有關卡的進度列表
        public List<TrialStageProgressDto> Stages { get; set; }

        // --- 個人化欄位 ---
        // 只有在「已登入使用者」查看時，這個欄位才可能有值
        public string FriendState { get; set; } // 例如 "pending", "accepted", "not_friend"
    }
}