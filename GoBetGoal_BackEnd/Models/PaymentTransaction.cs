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
        [StringLength(50)]
        public string Status { get; set; }  //狀態

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;    //建立時間

        [Required]
        [StringLength(100)]
        public string OrderNo { get; set; } //訂單編號

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }    //狀態更新時間






    }
}