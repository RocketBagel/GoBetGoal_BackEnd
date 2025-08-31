using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace GoBetGoal_BackEnd.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductType
    {
       
        Bagel =0,

        [EnumMember(Value = "cheat_blanket")]
        CheatBlanket =1,

        [EnumMember(Value = "challenge")]
        TrialTemplate =2,

 
        Avatar =3
    }
}