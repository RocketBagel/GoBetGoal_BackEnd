using GoBetGoal_BackEnd.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class PaymentTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public decimal Amount { get; set; }      //支付金額 (NTD)

        [Required]
        public Method Method { get; set; }  //支付方式

        [Required]
        public Status Status { get; set; }  //狀態(Pending,Success,Failed)

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;    //建立時間
    }
}