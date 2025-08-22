using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class UseCheatBlanketResponseDto
    {
        public string Message { get; set; }

        /// <summary>
        /// 使用後剩餘的遮羞布數量
        /// </summary>
        public int RemainingCheatBlanketCount { get; set; }
    }
}