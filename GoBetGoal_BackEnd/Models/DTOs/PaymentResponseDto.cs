using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    //用來接收藍新金流 ReturnURL / NotifyURL 的回傳結果

    public class PaymentResponseDto
    {
        public string Status { get; set; }
        public PaymentResponseDetail Result { get; set; }
    }

    public class PaymentResponseDetail
    {
        public string MerchantOrderNo { get; set; } //訂單編號
        public decimal Amt { get; set; } //金額
        public string TradeNo { get; set; } //交易編號
        public string PaymentType { get; set; } //支付方式
        public DateTime? PayTime { get; set; } //付款完成時間
    }

}
