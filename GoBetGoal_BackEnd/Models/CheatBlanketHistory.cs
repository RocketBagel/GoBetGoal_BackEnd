using GoBetGoal_BackEnd.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class CheatBlanketHistory
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public int? UserStageId { get; set; }
        public virtual UserStage UserStage { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public int BalanceBefore { get; set; }

        [Required]
        public int BalanceAfter { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}