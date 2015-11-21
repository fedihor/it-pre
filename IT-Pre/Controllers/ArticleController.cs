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

            var f = article.ArticleSubject.Asubject;
            var fd = article.Asubject1;

            article.Articletext = ReplaceSpecialTags(EncodeString(article.Articletext));

            ViewBag.Articletext = article.Articletext;

            return View(article);
        }

        // GET: Article/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.Asubject1 = new SelectList(db.ArticleSubjects, "Id", "Asubject", 1);
            return View();
        }

        // POST: Article/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "Id,Title,Articletext,Userid,Adate,Asubject1")] Article article)
        {
            if (ModelState.IsValid)
            {
                db.Articles.Add(article);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(article);
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

            StringBuilder sb = new StringBuilder(HttpUtility.HtmlDecode(article.Articletext));
            article.Articletext = sb.ToString();

            return View(article);
        }

        // POST: Article/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "Id,Title,Articletext,Userid,Adate,Asubject1")] Article article)
        {
            if (ModelState.IsValid)
            {
                db.Entry(article).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            article.Articletext = EncodeString(article.Articletext);

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
