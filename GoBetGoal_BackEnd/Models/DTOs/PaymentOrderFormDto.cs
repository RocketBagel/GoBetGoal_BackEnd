using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class PaymentOrderFormDto
    {
        public string MerchantID { get; set; }
        public string MerchantOrderNo { get; set; }
        public string TradeInfo { get; set; }
        public string TradeSha { get; set; }
        public string Version { get; set; }
        public string PayGateWay { get; set; }
    }
}
