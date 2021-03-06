﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eQuiz.Web.Areas.Admin.Models
{
    public class QuestionBase : IQuestion
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public byte MaxScore { get; set; }
        public double? UserScore { get; set; }
        public string QuestionText { get; set; }
        public short? Order { get; set; }
        public bool WasChanged { get; set; }
        public bool WasNull { get; set; }
        public string Status { get; set; }

        // Base constructor
        public QuestionBase (int id, byte maxScore, double? userScore, string questionText, short? order, bool wasChanged, bool wasNull)
        {
            Id = id;
            MaxScore = maxScore;
            UserScore = userScore;
            QuestionText = questionText;
            Order = order;
            WasChanged = WasChanged;
            WasNull = wasNull;
            if (userScore != null)
            {
                if (userScore > 0)
                {
                    Status = "Passed";
                }
                else
                {
                    Status = "Not Passed";
                }
            }
            else
            {
                Status = "In Verification";
            }
        }
    }
}