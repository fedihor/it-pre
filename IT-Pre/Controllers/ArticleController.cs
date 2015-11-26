using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IT_Pre.Models;
using System.Text;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace IT_Pre.Controllers
{
    public class ArticleController : Controller
    {
        private ItpreEntities db = new ItpreEntities();

        // GET: Article
        public ActionResult Index()
        {
            return View(db.Articles.ToList());
        }

        // GET: Article/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Article article = db.Articles.Find(id);

            if (article == null)
            {
                return HttpNotFound();
            }

            // Проверяем необходимость увеличения количества просмотров статьи при ее открытии и увеличиваем если,
            // пользователь не админ и если пользователь открыл статью первый раз за сессию
            string currentUser = HttpContext.User.Identity.GetUserId() == null ? "___guest" : HttpContext.User.Identity.GetUserId();
            if (currentUser.CompareTo(article.Userid) != 0 && !User.IsInRole("admin")
                && !HttpContext.Request.Cookies.AllKeys.Contains("a_" + article.Id.ToString()))
            {
                HttpCookie viewArticleCookie = new HttpCookie("a_" + article.Id.ToString(), "+");
                HttpContext.Response.Cookies.Set(viewArticleCookie);
                article.Viewcounter++;
                db.SaveChanges();
            }

            article.Articletext = ReplaceSpecialTags(EncodeString(article.Articletext));

            return View(article);
        }

        // GET: Article/Create
        [Authorize]
        public ActionResult Create()
        {
            List<Dictionary<string, object>> langs = new List<Dictionary<string, object>>();

            foreach (var lang in db.Proglangs)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Proglang", lang);
                dict.Add("IsSelected", false);
                langs.Add(dict);
            }

            ViewBag.Proglangs = langs;

            ViewBag.Asubject1 = new SelectList(db.ArticleSubjects.OrderBy(s => s.Id), "Id", "Asubject", 1);

            return View();
        }

        // POST: Article/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "Id,Title,Articletext,Userid,Adate,Asubject1,ArticleSubject,Articles_Proglangs")] Article article)
        {
            bool validError = false;

            if (string.IsNullOrEmpty(article.Asubject1.ToString()))
            {
                validError = true;
                ModelState.AddModelError("Asubject1", "Укажите тему статьи.");
            }
            if (ModelState.IsValid)
            {
                db.Articles.Add(article);
                /*
                List<Article_Proglang> proglang = new List<Article_Proglang>();

                foreach (var lang in Request["Proglang_Id"].Split(','))
                {
                    if (lang != "false")
                    {
                        proglang.Add(new Article_Proglang()
                        {
                            Article = article,
                            Proglang = db.Proglangs.Find(int.Parse(lang))
                        });
                    }
                }

                db.Articles_Proglangs.AddRange(proglang);
                */
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                List<Dictionary<string, object>> langs = new List<Dictionary<string, object>>();

                var currentLangs = Request["Proglang_Id"].Split(',').ToList();

                foreach (var lang in db.Proglangs)
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("Proglang", lang);
                    if (currentLangs.Contains(lang.Id.ToString()))
                    {
                        dict.Add("IsSelected", true);
                    }
                    else
                    {
                        dict.Add("IsSelected", false);
                    }
                    langs.Add(dict);
                }
                if (!validError)
                {
                    ViewBag.Error = "Произошел сбой при сохранении. Данные не сохранены. Попробуйте еще раз.";
                }
                ViewBag.Proglangs = langs;
                ViewBag.Asubject1 = new SelectList(db.ArticleSubjects.OrderBy(s => s.Id), "Id", "Asubject", 1);

                return View(article);
            }
        }

        // GET: Article/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Article article = db.Articles.Find(id);

            if (article == null)
            {
                return HttpNotFound();
            }

            ViewBag.Proglangs = db.Proglangs.ToList();

            StringBuilder sb = new StringBuilder(HttpUtility.HtmlDecode(article.Articletext));
            article.Articletext = sb.ToString();

            ViewBag.Asubject1 = new SelectList(db.ArticleSubjects.OrderBy(s => s.Id), "Id", "Asubject", article.Asubject1);
/*
            List<Dictionary<string, object>> langs = new List<Dictionary<string, object>>();

            List<Proglang> currentLangs = new List<Proglang>();
            
            foreach (var rticleProglang in article.Articles_Proglangs)
            {
                currentLangs.Add(rticleProglang.Proglang);
            }

            foreach (var lang in db.Proglangs)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Proglang", lang);
                if (currentLangs.Contains(lang))
                {
                    dict.Add("IsSelected", true);
                }
                else
                {
                    dict.Add("IsSelected", false);
                }
                langs.Add(dict);
            }
            ViewBag.Proglangs = langs;*/

            return View(article);
        }

        // POST: Article/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "Id,Title,Articletext,Userid,Adate,Asubject1,Proglangs")] Article article, int[] selectedProglangs)
        {
            if (ModelState.IsValid)
            {


                Article newAricle = db.Articles.Find(article.Id);
                newAricle.Title = article.Title;
                newAricle.Articletext = article.Articletext;
                newAricle.Asubject1 = article.Asubject1;
                newAricle.Proglangs.Clear();

                if (selectedProglangs != null)
                {
                    foreach (var lang in db.Proglangs.Where(l => selectedProglangs.Contains(l.Id)))
                    {
                        newAricle.Proglangs.Add(lang);
                    }
                }

                db.Entry(newAricle).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction("Index");







                /*
                List<Article_Proglang> proglang = new List<Article_Proglang>();

                foreach (var lang in Request["Proglang_Id"].Split(','))
                {
                    if (lang != "false")
                    {
                        proglang.Add(new Article_Proglang()
                        {
                            Article = article,
                            Proglang = db.Proglangs.Find(int.Parse(lang))
                        });
                    }
                }
                var delItems = db.Articles_Proglangs.Where(art => art.Article.Id == article.Id);

                db.Articles_Proglangs.RemoveRange(delItems);

                db.Articles_Proglangs.AddRange(proglang);
                */

            }
            else
            {

                List<Dictionary<string, object>> langs = new List<Dictionary<string, object>>();

                var currentLangs = Request["Proglang_Id"].Split(',').ToList();

                foreach (var lang in db.Proglangs)
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("Proglang", lang);
                    if (currentLangs.Contains(lang.Id.ToString()))
                    {
                        dict.Add("IsSelected", true);
                    }
                    else
                    {
                        dict.Add("IsSelected", false);
                    }
                    langs.Add(dict);
                }
                ViewBag.Proglangs = langs;

                ViewBag.Error = "Произошел сбой при сохранении. Данные не сохранены. Попробуйте еще раз.";

                ViewBag.Asubject1 = new SelectList(db.ArticleSubjects.OrderBy(s => s.Id), "Id", "Asubject", 1);
                
                article.Articletext = EncodeString(article.Articletext);
            }

            return View(article);
        }

        // GET: Article/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Articles.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }


            return View(article);
        }

        // POST: Article/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            Article article = db.Articles.Find(id);
            db.Articles.Remove(article);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private string EncodeString(string stringToEncode)
        {
            StringBuilder sb = new StringBuilder(HttpUtility.HtmlEncode(stringToEncode));
            sb.Replace("&lt;к&gt;", "<к>");
            sb.Replace("&lt;/к&gt;", "</к>");
            sb.Replace("&lt;ж&gt;", "<ж>");
            sb.Replace("&lt;/ж&gt;", "</ж>");
            sb.Replace("&lt;н&gt;", "<н>");
            sb.Replace("&lt;/н&gt;", "</н>");
            sb.Replace("&lt;п&gt;", "<п>");
            sb.Replace("&lt;/п&gt;", "</п>");

            return sb.ToString();
        }

        private string ReplaceSpecialTags(string stringToReplace)
        {
            StringBuilder sb = new StringBuilder(stringToReplace);

            sb.Replace("<к>", @"<pre class=""prettyprint linenums: 1"">");
            sb.Replace("</к>", @"</pre>");

            sb.Replace("<ж>", @"<b>");
            sb.Replace("</ж>", @"</b>");

            sb.Replace("<н>", @"<i>");
            sb.Replace("</н>", @"</i>");

            sb.Replace("<п>", @"<u>");
            sb.Replace("</п>", @"</u>");

            return sb.ToString();
        }
    }
}
