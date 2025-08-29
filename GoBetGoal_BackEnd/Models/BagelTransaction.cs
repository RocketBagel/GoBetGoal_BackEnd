using GoBetGoal_BackEnd.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class BagelTransaction
    {
        [Key] public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        public ProductType ProductType { get; set; }

        public int? ReferenceId { get; set; } //Avatar與TrialTemplate會有存值(對應的Id值)，其他商品則為 null
        //關聯欄位(可能 null)
        //public int? AvatarId { get; set; }
        //public virtual Avatar Avatar { get; set; }

        //public int? TrialTemplateId { get; set; }
        //public virtual TrialTemplate TrialTemplate { get; set; }

        //public int? PaymentTransactionId { get; set; }
        //public virtual PaymentTransaction PaymentTransaction { get; set; }

        [Required]
        [StringLength(100)]
        public string ItemName { get; set; }

        [Required]
        public int Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public int Amount { get; set; } //Amount = Price * Quantity

        [Required]
        public int BalanceBefore { get; set; }

        [Required]
        public int BalanceAfter { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}