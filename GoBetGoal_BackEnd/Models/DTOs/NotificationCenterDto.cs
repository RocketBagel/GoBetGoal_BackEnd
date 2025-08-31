using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class NotificationCenterDto
    {
        /// <summary>
        /// 公告類型的通知
        /// </summary>
        public List<NotificationDto> Announcements { get; set; }

        /// <summary>
        /// 未讀的個人通知
        /// </summary>
        public List<NotificationDto> Unread { get; set; }

        /// <summary>
        /// 已讀的個人通知
        /// </summary>
        public List<NotificationDto> Read { get; set; }
    }
}