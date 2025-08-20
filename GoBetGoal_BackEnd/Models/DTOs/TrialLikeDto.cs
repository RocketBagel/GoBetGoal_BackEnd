using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class TrialLikeDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string PlayerId { get; set; }
        public string NickName { get; set; }
        public int BagelCount { get; set; }
        public int CheatBlanketCount { get; set; }
        public string CurrentAvatarUrl { get; set; }
    }
}