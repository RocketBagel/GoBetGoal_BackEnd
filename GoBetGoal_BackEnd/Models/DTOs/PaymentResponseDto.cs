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
        public string Status { get; set; }     //交易結果狀態 (SUCCESS/其他錯誤碼)
        public string MerchantID { get; set; } //商店編號
        public string Version { get; set; }    //API版本
        public string TradeInfo { get; set; }  //加密過的交易資訊
        public string TradeSha { get; set; }   //驗證用雜湊值 (用來驗 TradeInfo 正確性)
    }



    public class TradeInfoResponseDto
    {
        public string Status { get; set; } //交易結果狀態

        public string Message { get; set; } //交易訊息("授權成功"或"錯誤訊息")

        public TradeInfoResponseDetail Result { get; set; }
    }


    public class TradeInfoResponseDetail
    {
        public string MerchantID { get; set; } //商店編號
        public string Amt { get; set; } //金額
        public string TradeNo { get; set; } //交易序號
        public string MerchantOrderNo { get; set; }  //訂單編號

        public string RespondType { get; set; }
        public string IP { get; set; } // 付款端 IP
        public string EscrowBank { get; set; } //代收銀行代碼
        public string PaymentType { get; set; } //支付方式 (收款管道/付款工具)
        public string RespondCode { get; set; } //授權碼（00 代表成功，其他為錯誤碼）
        public string Auth { get; set; } //信用卡授權碼
        public string Card6No { get; set; } //卡號前六碼
        public string Card4No { get; set; } //卡號後四碼
        public string Exp { get; set; } //信用卡有效期 (YYMM)
        public string TokenUseStatus { get; set; } //是否使用信用卡快速付款功能: Token 機制 (0=未使用, 1=使用)
        public string InstFirst { get; set; } //首期金額
        public string InstEach { get; set; } //每期金額
        public string Inst { get; set; } //刷卡分期期數
        public string ECI { get; set; } //3D 驗證 ECI 值
        public string PayTime { get; set; } //付款完成時間
        public string PaymentMethod { get; set; } //付款方式描述 (CREDIT=信用卡, WEBATM, VACC...)
        public string AuthBank { get; set; }     // 授權銀行代碼
        public string ExpireDate { get; set; }   // 繳費截止日 (ATM/超商繳費會用到)
    }

}
