using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    /// <summary>
    /// 通用的成功回應
    /// </summary>
    public class PurchaseSuccessDto
    {
        public string Message { get; set; }
        public int RemainingBagelCount { get; set; }
    }
}