using GoBetGoal_BackEnd.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }
        public virtual User Receiver { get; set; }

        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public bool IsRead { get; set; } = false;

        public Guid? UserId { get; set; }
        public virtual User User { get; set; }

        // trialId & postId 
        public int? ReferenceId { get; set; }


        // 顯示 user avatar
        public Guid? SenderId { get; set; }
        public virtual User Sender { get; set; }


        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;    //建立時間

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }    //狀態更新時間


    }
}