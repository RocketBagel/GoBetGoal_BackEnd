using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class InviteFriendDto
    {

        [Required]
        public string InviteeId { get; set; }

      
        public string Note { get; set; }

       
    }
}