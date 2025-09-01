using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class FriendInfoDto
    {
        public Guid Id { get; set; }

        public string NickName { get; set; }
       
        public string AvatarUrl { get; set; }

        public int TrialCount { get; set; } 

        public int PostCount { get; set; }
    }
}