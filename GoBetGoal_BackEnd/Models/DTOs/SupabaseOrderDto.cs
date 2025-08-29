using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class SupabaseOrderDto
    {
        public Guid id { get; set; }   //接收用
        public Guid user_id { get; set; }
        public int get_bagel { get; set; }
        public int deposit_money { get; set; }
        public string status { get; set; }
        public string order_no { get; set; }
    }
}