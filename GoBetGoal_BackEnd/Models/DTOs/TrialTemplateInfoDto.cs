using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class TrialTemplateInfoDto
    {
        public int Id { get; set; }


        public string TrialTitle { get; set; }


        public string TrialDescription { get; set; }

        public int TrialFrequency { get; set; }


        public string TrialCategory { get; set; }


        public string TrialSuitFor { get; set; }


        public string TrialNoSuitFor { get; set; }


        public string TrialRule { get; set; }


        public string TrialCaution { get; set; }


        public string TrialEffect { get; set; }


        public int StageCount { get; set; }


        public int MaxUser { get; set; }

        public bool IsAi { get; set; }

        public int TrialTemplatePrice { get; set; }

        //public ProductType ProductType { get; set; }


        public string CardImagePath { get; set; }


        public string CardColor { get; set; }
    }
}