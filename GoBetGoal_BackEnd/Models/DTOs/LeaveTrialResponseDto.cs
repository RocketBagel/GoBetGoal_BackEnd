using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class LeaveTrialResponseDto
    {
        public string Message { get; set; }

        /// <summary>
        /// 退出試煉並退還押金後，使用者剩餘的貝果總數
        /// </summary>
        public int NewBagelCount { get; set; }
    }
}