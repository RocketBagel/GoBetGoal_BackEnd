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

        // --- 接收者 (必填) ---
        [Required]
        public Guid ReceiverId { get; set; }
        public virtual User Receiver { get; set; }

        // --- 發送者 (可選) ---
        public Guid? SenderId { get; set; }
        public virtual User Sender { get; set; }

        // --- 通知內容 ---
        [Required]
        public NotificationType NotificationType { get; set; }

        [Required]
        [StringLength(150)]
        public string Content { get; set; }

        [Required]
        public bool IsRead { get; set; } = false;

        // --- 關聯物件 ID (分離欄位法) ---
        // 指向 Post, Trial, FriendsRelationship 等 int 主鍵的表
        public int? ReferenceId_Int { get; set; }

        // 指向 User 等 Guid 主鍵的表 (例如當通知本身是關於某個使用者時)
        public Guid? ReferenceId_Guid { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }


    }
}