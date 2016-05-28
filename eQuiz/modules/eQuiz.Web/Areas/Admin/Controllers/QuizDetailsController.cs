﻿using eQuiz.Entities;
using eQuiz.Repositories.Abstract;
using eQuiz.Web.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eQuiz.Web.Areas.Admin.Controllers
{
    public class QuizDetailsController : BaseController
    {
        #region Fields

        private readonly IRepository _repository;

        #endregion

        #region Constructors

        public QuizDetailsController(IRepository repository)
        {
            this._repository = repository;
        }

        #endregion

        #region Web actions

        [HttpGet]
        public JsonResult GetQuizPasses(int id)
        {
            var result = new List<object>();

 

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetQuiz(int id)
        {
            var result = new List<object>();

            var quizzPasses = _repository.Get<QuizPass>();
            var quizPassScore = _repository.Get<QuizPassScore>();
            var quiz = _repository.Get<Quiz>();            
            var ugroup = _repository.Get<UserGroup>();
            var qblock = _repository.Get<QuizBlock>();

            var query = from q in quiz                        
                        join ug in ugroup on q.GroupId equals ug.Id                        
                        join qp in quizzPasses on q.Id equals qp.QuizId
                        join qps in quizPassScore on qp.Id equals qps.Id
                        where q.Id == id
                        select new
                        {
                            quizName = q.Name,
                            groupName = ug.Name,
                            quizScore = qps.PassScore                           
                        };

            foreach(var item in query)
            {
                result.Add(item);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}