using GoBetGoal_BackEnd.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }

        // 加上發送者的公開資訊
        public PublicUserProfileDto SenderInfo { get; set; }

        public NotificationType NotificationType { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public int? ReferenceId_Int { get; set; }
        public Guid? ReferenceId_Guid { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}