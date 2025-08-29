using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Enums
{
    public enum NotificationType
    {
        announcement=0,
        friend_request=1,
        friend_request_accept = 2,
        trial_invite=3,
        trial_invite_accept = 4,
        post_liked=5,
        trial_count_down=6,
        trial_close=7,
        trial_start=8
    }
}