﻿using eQuiz.Entities;
using eQuiz.Repositories.Abstract;
using eQuiz.Repositories.Concrete;
using eQuiz.Web.Areas.Moderator.Models;
using eQuiz.Web.Code;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace eQuiz.Web.Areas.Moderator.Controllers
{
    public class QuizController : BaseController
    {
        #region Fields

        private readonly IRepository _repository;

        #endregion

        #region Constructors

        public QuizController(IRepository repository)
        {
            this._repository = repository;
        }

        #endregion

        #region Web Actions

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return RedirectToAction("Edit");
        }

        public ActionResult Edit()
        {
            return View();
        }

        [HttpGet]
        public ActionResult IsNameUnique(string name, int? id)
        {
            var result = ValidateQuizName(name, id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Get(int id)
        {
            Quiz quiz = _repository.GetSingle<Quiz>(q => q.Id == id, r => r.UserGroup, s => s.QuizState);
            quiz.UserGroup.Quizs = null;
            quiz.QuizState.Quizs = null;
            QuizBlock block = _repository.GetSingle<QuizBlock>(b => b.QuizId == id);

            var data = JsonConvert.SerializeObject(new { quiz = quiz, block = block }, Formatting.None,
                                                    new JsonSerializerSettings()
                                                    {
                                                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                    });

            return Content(data, "application/json");
        }

        public ActionResult GetQuizzesForCopy()
        {
            IEnumerable<Quiz> quizzes = _repository.Get<Quiz>(q => q.QuizState.Name != "Draft", q => q.QuizState);

            var data = JsonConvert.SerializeObject(quizzes, Formatting.None,
                                                   new JsonSerializerSettings()
                                                   {
                                                       ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                   });

            return Content(data, "application/json");
        }

        public ActionResult GetStates()
        {
            var states = new List<QuizState>();

            states.Add(_repository.GetSingle<QuizState>(s => s.Name == "Draft"));
            states.Add(_repository.GetSingle<QuizState>(s => s.Name == "Opened"));
            states.Add(_repository.GetSingle<QuizState>(s => s.Name == "Scheduled"));
            states.Add(_repository.GetSingle<QuizState>(s => s.Name == "Archived"));

            var data = JsonConvert.SerializeObject(states, Formatting.None,
                                                  new JsonSerializerSettings()
                                                  {
                                                      ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                  });

            return Content(data, "application/json");
        }

        [HttpGet]
        public ActionResult GetQuizzesPage(int currentPage = 1, int quizzesPerPage = 3, string predicate = "Name",
                                            bool reverse = false, string searchText = null)
        {
            IEnumerable<QuizListModel> quizzesList = null;
            var quizzesTotal = 0;

            using (var context = new eQuizEntities(System.Configuration.ConfigurationManager.ConnectionStrings["eQuizDB"].ConnectionString))
            {
                //TODO : edit using repositories
                quizzesList = (from quiz in context.Quizs
                               join quizBlock in context.QuizBlocks on quiz.Id equals quizBlock.QuizId
                               join quizState in context.QuizStates on quiz.QuizStateId equals quizState.Id
                               select
                                     new QuizListModel
                                     {
                                         Id = quiz.Id,
                                         Name = quiz.Name,
                                         CountOfQuestions = quizBlock.QuestionCount,
                                         StartDate = quiz.StartDate,
                                         Duration = quiz.TimeLimitMinutes,
                                         StateName = quizState.Name
                                     }).Where(item => (searchText == null || item.Name.Contains(searchText)) &&
                                             (item.StateName == "Opened" || item.StateName == "Draft" || item.StateName == "Scheduled"))
                                             .OrderBy(q => q.Name);

                quizzesTotal = quizzesList.Count();

                switch (predicate)
                {
                    case "Name":
                        quizzesList = reverse ? quizzesList.OrderByDescending(q => q.Name) : quizzesList.OrderBy(q => q.Name);
                        break;
                    case "CountOfQuestions":
                        quizzesList = reverse ? quizzesList.OrderByDescending(q => q.CountOfQuestions) : quizzesList.OrderBy(q => q.CountOfQuestions);
                        break;
                    case "StartDate":
                        quizzesList = reverse ? quizzesList.OrderByDescending(q => q.StartDate) : quizzesList.OrderBy(q => q.StartDate);
                        break;
                    case "StateName":
                        quizzesList = reverse ? quizzesList.OrderByDescending(q => q.StateName) : quizzesList.OrderBy(q => q.StateName);
                        break;
                    case "Duration":
                        quizzesList = reverse ? quizzesList.OrderByDescending(q => q.Duration) : quizzesList.OrderBy(q => q.Duration);
                        break;
                    default:
                        quizzesList = reverse ? quizzesList.OrderByDescending(q => q.Name) : quizzesList.OrderBy(q => q.Name);
                        break;
                }

                quizzesList = quizzesList.Skip((currentPage - 1) * quizzesPerPage).Take(quizzesPerPage).ToList();
            }
            return Json(new { Quizzes = quizzesList, QuizzesTotal = quizzesTotal }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(Quiz quiz, QuizBlock block)
        {
            var errors = ValidateQuiz(quiz, block);
            if (errors != null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid data");
            }

            if (quiz.Id != 0)
            {
                quiz.QuizStateId = quiz.QuizState.Id;
                quiz.QuizState = null;
                if (quiz.UserGroup != null)
                {
                    quiz.GroupId = quiz.UserGroup.Id;
                    quiz.UserGroup = null;
                }
                _repository.Update<Quiz>(quiz);
                _repository.Update<QuizBlock>(block);
            }
            else
            {
                quiz.QuizStateId = quiz.QuizState.Id;
                quiz.QuizState = null;
                quiz.GroupId = 1; // UPDATE DB
                //
                _repository.Insert<Quiz>(quiz);
                block.TopicId = 1;
                block.QuizId = quiz.Id;
                _repository.Insert<QuizBlock>(block);
                _repository.Insert<QuizVariant>(new QuizVariant() { QuizId = quiz.Id });
            }
            var data = JsonConvert.SerializeObject(new { quiz = quiz, block = block }, Formatting.None,
                                                    new JsonSerializerSettings()
                                                    {
                                                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                    });

            return Content(data, "application/json");
        }

        public ActionResult DeleteQuizById(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var quizBlocks = _repository.Get<QuizBlock>(qb => qb.QuizId == id);

            if (!quizBlocks.Any())
            {
                return HttpNotFound();
            }

            foreach (var quizBlock in quizBlocks)
            {
                var quizQuestions = _repository.Get<QuizQuestion>(qq => qq.QuizBlockId == quizBlock.Id);
                foreach (var quizQuestion in quizQuestions)
                {
                    _repository.Delete<int, QuizQuestion>("Id", quizQuestion.Id);
                }

                var quizPassQuestions = _repository.Get<QuizPassQuestion>(qpq => qpq.QuizBlockId == quizBlock.Id);

                foreach (var quizPassQuestion in quizPassQuestions)
                {
                    var userAnswers =
                        _repository.Get<UserAnswer>(ua => ua.QuizPassQuestionId == quizPassQuestion.Id);
                    foreach (var userAnswer in userAnswers)
                    {
                        _repository.Delete<int, UserAnswer>("Id", userAnswer.Id);
                    }

                    _repository.Delete<int, QuizPassQuestion>("Id", quizPassQuestion.Id);
                }

                _repository.Delete<int?, QuizBlock>("Id", quizBlock.Id);
            }

            var quizPasses = _repository.Get<QuizPass>(qp => qp.QuizId == id);

            foreach (var quizPass in quizPasses)
            {
                var quizPassQuestions = _repository.Get<QuizPassQuestion>(qpq => qpq.QuizBlockId == quizPass.Id);
                foreach (var quizPassQuestion in quizPassQuestions)
                {
                    var userAnswers = _repository.Get<UserAnswer>(ua => ua.QuizPassQuestionId == quizPassQuestion.Id);

                    foreach (var userAnswer in userAnswers)
                    {
                        _repository.Delete<int, UserAnswer>("Id", userAnswer.Id);
                    }

                    _repository.Delete<int, QuizPassQuestion>("Id", quizPassQuestion.Id);
                }

                _repository.Delete<int?, QuizPass>("Id", quizPass.Id);
            }

            var quizVariants = _repository.Get<QuizVariant>(qv => qv.QuizId == id);

            foreach (var quizVariant in quizVariants)
            {
                _repository.Delete<int?, QuizVariant>("Id", quizVariant.Id);
            }

            _repository.Delete<int?, Quiz>("Id", id);

            return RedirectToAction("Index", "Quiz");
        }

        private bool ValidateQuizName(string name, int? id)
        {
            bool exists = true;

            if (id != null)
            {
                var quiz = _repository.GetSingle<Quiz>(q => q.Name == name);
                if (quiz == null)
                {
                    exists = false;
                }
                else if (quiz.Id == (int)id)
                {
                    exists = false;
                }
            }
            else
            {
                exists = _repository.Exists<Quiz>(q => q.Name == name);
            }

            return !exists;
        }

        [NonAction]
        private IEnumerable<string> ValidateQuiz(Quiz quiz, QuizBlock block)
        {
            var errorMessages = new List<string>();

            if (quiz.Name == null)
            {
                errorMessages.Add("There is no quiz name");
            }

            if (!ValidateQuizName(quiz.Name, quiz.Id))
            {
                errorMessages.Add("Quiz name is not unique");
            }

            if (block.QuestionCount == null)
            {
                errorMessages.Add("There is no question quantity");
            }
            else if (block.QuestionCount <= 0)
            {
                errorMessages.Add("Question quantity should be greater then 0");
            }

            if (!_repository.Exists<QuizType>(q => q.Id == quiz.QuizTypeId))
            {
                errorMessages.Add("There is no such quiz type in database");
            }

            if (quiz.QuizState == null)
            {
                if (quiz.StartDate != null)
                {
                    errorMessages.Add("There is start date but state isnt Scheduled");
                }
                if (quiz.EndDate != null)
                {
                    errorMessages.Add("There is end date but state isnt Scheduled");
                }
                if (quiz.TimeLimitMinutes != null)
                {
                    errorMessages.Add("There is time limit but state isnt Scheduled");
                }
                //if (quiz.UserGroup != null) UPDATE DB
                //{
                //    errorMessages.Add("There is user group selected but state isnt Scheduled");
                //}
            }
            else if (quiz.QuizState.Name == "Scheduled")
            {
                if (quiz.StartDate == null)
                {
                    errorMessages.Add("There is no start date");
                }

                if (quiz.StartDate <= DateTime.Now)
                {
                    errorMessages.Add("Start date should be greater then current date");
                }

                if (quiz.EndDate == null)
                {
                    errorMessages.Add("There is no end date");
                }

                if (quiz.EndDate <= DateTime.Now)
                {
                    errorMessages.Add("End date should be greater then current date");
                }

                if (quiz.TimeLimitMinutes == null)
                {
                    errorMessages.Add("There is no time limit");
                }
                else if (quiz.TimeLimitMinutes <= 0)
                {
                    errorMessages.Add("Time limit should be greater then 0");
                }

                if (quiz.UserGroup == null)
                {
                    errorMessages.Add("There is no user group selected");
                }

                if (!_repository.Exists<UserGroup>(q => q.Id == quiz.UserGroup.Id))
                {
                    errorMessages.Add("There is no such user group in the database");
                }
            }

            return errorMessages.Count > 0 ? errorMessages : null;
        }

        [HttpGet]
        public ActionResult QuizInfo(int id = 1)
        {
            var quiz = _repository.GetByKey<int, Quiz>("Id", id);
            var userGroup = _repository.GetSingle<UserGroup>(ug => ug.Id == quiz.GroupId);
            var quizType = _repository.GetSingle<QuizType>(qt => qt.Id == quiz.QuizTypeId);
            var quizState = _repository.GetSingle<QuizState>(qs => qs.Id == quiz.QuizStateId);

            var quizInfoModel = new QuizInfoModel();

            quizInfoModel.Id = quiz.Id;
            quizInfoModel.Type = quizType.TypeName;
            quizInfoModel.Name = quiz.Name;
            quizInfoModel.Descriprtion = quiz.Description;
            quizInfoModel.StartDate = quiz.StartDate;
            quizInfoModel.EndDate = quiz.EndDate;
            quizInfoModel.TimeLimitMinutes = quiz.TimeLimitMinutes;
            quizInfoModel.InternetAccess = quiz.InternetAccess;
            quizInfoModel.Group = userGroup.Name;
            quizInfoModel.State = quizState.Name;

            var quizBlock = _repository.GetSingle<QuizBlock>(qb => qb.QuizId == id);
            var quizTopic = _repository.GetSingle<Topic>(t => t.Id == quizBlock.TopicId);

            quizInfoModel.Topic = quizTopic.Name;
            quizInfoModel.IsRandom = quizBlock.IsRandom;
            quizInfoModel.QuestionMinComplexity = quizBlock.QuestionMinComplexity;
            quizInfoModel.QuestionMaxComplexity = quizBlock.QuestionMaxComplexity;
            quizInfoModel.QuestionCount = quizBlock.QuestionCount;

            var quizQuestions = _repository.Get<QuizQuestion>(qq => qq.QuizBlockId == quizBlock.Id);

            quizInfoModel.Questions = new List<QuestionInfo>();

            foreach (var quizQuestion in quizQuestions)
            {
                var question = _repository.GetSingle<Question>(q => q.Id == quizQuestion.QuestionId);

                var questionType = _repository.GetSingle<QuestionType>(qt => qt.Id == question.QuestionTypeId);

                var questionAnswers = _repository.Get<QuestionAnswer>(qa => qa.QuestionId == question.Id);

                var answers = questionAnswers.Select(questionAnswer => _repository.GetSingle<Answer>(a => a.Id == questionAnswer.AnswerId)).ToList();

                var questionInfo = new QuestionInfo
                {
                    Answers = answers,
                    QuestionComplexity = question.QuestionComplexity,
                    QuestionScore = quizQuestion.QuestionScore,
                    QuestionOrder = quizQuestion.QuestionOrder,
                    Type = questionType.TypeName,
                    Text = question.QuestionText
                };

                quizInfoModel.Questions.Add(questionInfo);
            }

            return View(quizInfoModel);
        }

        #endregion

    }
}
