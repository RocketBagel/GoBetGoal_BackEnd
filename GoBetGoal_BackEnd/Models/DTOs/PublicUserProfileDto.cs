using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    // 參與者的基本公開資訊 (我們可以複用之前建立的 UserProfileDto，或建立一個新的)
    // 待修改: 那些要公開
    public class PublicUserProfileDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string PlayerId { get; set; }
        public string NickName { get; set; }

        // 我們可以額外加入一個欄位，方便前端直接顯示使用者「目前」的頭像
        public int? CurrentAvatarId { get; set; }
        public string CurrentAvatarUrl { get; set; }
    }
}