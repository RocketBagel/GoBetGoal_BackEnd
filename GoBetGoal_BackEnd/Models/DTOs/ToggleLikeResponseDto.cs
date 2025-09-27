using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class ToggleLikeResponseDto
    {
        public string Message { get; set; }

        /// <summary>
        /// 操作後，使用者是否處於「喜歡」的狀態
        /// </summary>
        public bool IsLiked { get; set; }

        /// <summary>
        /// 更新後的總圍觀人數
        /// </summary>
        public int NewLikeCount { get; set; }
    }
}