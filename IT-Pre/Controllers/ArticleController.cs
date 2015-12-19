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
using System.IO;

namespace IT_Pre.Controllers
{
    public class ArticleController : Controller
    {
        private ItpreEntities db = new ItpreEntities();

        // GET: Article
        public ActionResult Index()
        {
            HttpCookie c = new HttpCookie("TIME");
            //c.Value = "3TIME";
            c.Expires = DateTime.Now.AddDays(-6d);
            Response.Cookies.Add(c);

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
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Create/" + new Random().Next(10000, 99999));
            }

            Article article = new Article()
            {
                Id = (int)id
            };

            ArticleAdditionData articleAdditionData = new ArticleAdditionData();

            GetCurrentArticleData(article, articleAdditionData);

            ViewBag.Asubject1 = articleAdditionData.Asubjects;

            ViewBag.articleAdditionData = articleAdditionData;

            return View(article);
        }

        private void GetCurrentArticleData(Article article, ArticleAdditionData articleAdditionData)
        {
            HttpCookie titleCookie = HttpContext.Request.Cookies.Get("Title" + article.Id);
            HttpCookie articletextCookie = HttpContext.Request.Cookies.Get("Articletext" + article.Id);
            HttpCookie asubject1Cookie = HttpContext.Request.Cookies.Get("Asubject1" + article.Id);
            HttpCookie proglangsCookie = HttpContext.Request.Cookies.Get("selectedProglangs" + article.Id);

            if (titleCookie != null) article.Title = titleCookie.Value.Replace("(&lt)", "<").Replace("(&gt)", ">");
            if (articletextCookie != null) article.Articletext = articletextCookie.Value.Replace("(&lt)", "<").Replace("(&gt)", ">");

            if (asubject1Cookie != null)
            {
                int subjectId;
                if (int.TryParse(asubject1Cookie.Value, out subjectId))
                {
                    article.Asubject1 = subjectId;
                }
                else
                {
                    article.Asubject1 = article.Asubject1;
                }
                articleAdditionData.Asubjects = CreateSelectList("Выберите тему", "0", article.Asubject1.ToString());
            }
            else
            {
                articleAdditionData.Asubjects = CreateSelectList("Выберите тему", "0", article.Asubject1.ToString());
            }

            if (proglangsCookie != null && !String.IsNullOrWhiteSpace(proglangsCookie.Value))
            {
                List<string> proglangs_ = proglangsCookie.Value.Split(',').ToList();
                List<int> proglangs = proglangs_.Select(s => int.Parse(s)).ToList();
                article.Proglangs = db.Proglangs.Where(e => proglangs.Contains(e.Id)).ToList();
            }

            articleAdditionData.Proglangs = db.Proglangs.ToList();

            string userId = User.Identity.GetUserId();

            articleAdditionData.Tempimages = db.Tempimages.Where(e => e.Userid == userId)
                .Where(e => e.Articletempid == article.Id).ToList();
        }

        private List<SelectListItem> CreateSelectList(string text, string value, string selected = "0")
        {
            List<SelectListItem> subjectList = new List<SelectListItem>();

            subjectList.Add(new SelectListItem
            {
                Text = text,
                Value = value,
                Selected = value.CompareTo(selected) == 0 ? true : false
            });

            foreach (var item in db.ArticleSubjects.OrderBy(s => s.Id))
            {
                subjectList.Add(new SelectListItem
                {
                    Text = item.Asubject,
                    Value = item.Id.ToString(),
                    Selected = item.Id.ToString().CompareTo(selected) == 0 ? true : false
                });
            }

            return subjectList;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "Id,Title,Articletext,Userid,Adate,Asubject1")] Article article, int[] selectedProglangs)
        {
            if (ModelState.IsValid && selectedProglangs != null && article.Asubject1 != 0)
            {
                int tempId = article.Id;
                article.Id = 0;
                article.Proglangs.Clear();

                foreach (var lang in db.Proglangs.Where(l => selectedProglangs.Contains(l.Id)))
                {
                    article.Proglangs.Add(lang);
                }

                db.Articles.Add(article);

                db.SaveChanges();

                removeArticleCookies(tempId);

                return RedirectToAction("Index");
            }
            else
            {
                if (selectedProglangs == null)
                {
                    ModelState.AddModelError("Proglangs", "Укажите языки программирования");
                }
                else
                {
                    foreach (var lang in db.Proglangs.Where(l => selectedProglangs.Contains(l.Id)))
                    {
                        article.Proglangs.Add(lang);
                    }
                }

                if (article.Asubject1 == 0)
                {
                    ModelState.AddModelError("Asubject1", "Укажите тему статьи");
                }



                ArticleAdditionData articleAdditionData = new ArticleAdditionData();

                GetCurrentArticleData(article, articleAdditionData);

                //articleAdditionData.Proglangs = db.Proglangs.ToList();
                //articleAdditionData.Asubjects = CreateSelectList("Выберите тему", "0");
                //string userId = User.Identity.GetUserId();
                //articleAdditionData.Tempimages = db.Tempimages.Where(e => e.Userid == userId)
                //    .Where(e => e.Articletempid == article.Id).ToList();
                //GetCurrentArticleData(article, articleAdditionData);

                ViewBag.Asubject1 = articleAdditionData.Asubjects;

                ViewBag.articleAdditionData = articleAdditionData;



                //ViewBag.Asubject1 = CreateSelectList("Выберите тему", "0");

                //ViewBag.Proglangs = db.Proglangs.ToList();

                article.Articletext = EncodeString(article.Articletext);
            }

            return View(article);
        }

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

            //ViewBag.Proglangs = db.Proglangs.ToList();

            StringBuilder sb = new StringBuilder(HttpUtility.HtmlDecode(article.Articletext));
            article.Articletext = sb.ToString();

            //var subjectList = new SelectList(db.ArticleSubjects.OrderBy(s => s.Id), "Id", "Asubject", article.Asubject1);

            //ViewBag.Asubject1 = subjectList;



            ArticleAdditionData articleAdditionData = new ArticleAdditionData();

            GetCurrentArticleData(article, articleAdditionData);

            ViewBag.Asubject1 = articleAdditionData.Asubjects;

            ViewBag.articleAdditionData = articleAdditionData;




            return View(article);
        }

        private void removeArticleCookies(int id)
        {
            if (Request.Cookies["Title" + id] != null)
            {
                HttpCookie my_Cookie = new HttpCookie("Title" + id);
                my_Cookie.Expires = DateTime.Now.AddDays(10d);
                //my_Cookie.Value = "VVAALLUUEE";
                Response.Cookies.Add(my_Cookie);
            }

            HttpCookie myCookie = new HttpCookie("Articletext" + id);
            myCookie.Expires = DateTime.Now.AddDays(10d);
            Response.Cookies.Add(myCookie);

            /*
            HttpCookie asubject1Cookie = HttpContext.Request.Cookies.Get("Asubject1" + id);
            HttpCookie proglangsCookie = HttpContext.Request.Cookies.Get("selectedProglangs" + id);

            HttpContext.Response.Cookies.Remove("Title" + id);
            HttpContext.Response.Cookies.Remove("Articletext" + id);
            HttpContext.Response.Cookies.Remove("Asubject1" + id);
            */
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "Id,Title,Articletext,Userid,Adate,Asubject1,Proglangs")] Article article, int[] selectedProglangs)
        {
            if (ModelState.IsValid && selectedProglangs != null)
            {
                Article newAricle = db.Articles.Find(article.Id);
                newAricle.Title = article.Title;
                newAricle.Articletext = article.Articletext;
                newAricle.Asubject1 = article.Asubject1;
                newAricle.Proglangs.Clear();

                foreach (var lang in db.Proglangs.Where(l => selectedProglangs.Contains(l.Id)))
                {
                    newAricle.Proglangs.Add(lang);
                }

                db.Entry(newAricle).State = EntityState.Modified;

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                if (selectedProglangs == null)
                {
                    //ViewBag.Langerror = "Необходимо указать языки программирования";
                    ModelState.AddModelError("Proglangs", "Необходимо указать языки программирования");
                }
                else
                {
                    foreach (var lang in db.Proglangs.Where(l => selectedProglangs.Contains(l.Id)))
                    {
                        article.Proglangs.Add(lang);
                    }
                }

                ViewBag.Asubject1 = new SelectList(db.ArticleSubjects.OrderBy(s => s.Id), "Id", "Asubject", article.Asubject1);

                ViewBag.Proglangs = db.Proglangs.ToList();

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

        [HttpPost]
        public JsonResult Uploadimg()
        {
            Tempimage img = new Tempimage();

            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];

                if (upload.ContentType != "image/jpeg" && upload.ContentType != "image/gif")
                {
                    return Json(new { error = "notvalid", message = "Изображение должно быть в формате jpeg или gif" });
                }

                if (upload.ContentLength > 3 * 1024 * 1024)
                {
                    return Json(new { error = "notvalid", message = "Размер файла не должен привышать 2 Мб" });
                }

                if (upload != null)
                {
                    string path = @"/Files/UploadImages/";
                    //string path = path + System.IO.Path.GetRandomFileName();
                    string originFileName = System.IO.Path.GetFileName(upload.FileName);
                    string newFileName = System.IO.Path.GetRandomFileName() + System.IO.Path.GetExtension(originFileName);
                    //folderName = System.IO.Path.Combine(path, Guid.NewGuid().ToString("N"));
                    //System.IO.Directory.CreateDirectory(folderName);

                    try
                    {
                        upload.SaveAs(Server.MapPath(path + newFileName));
                    }
                    catch (Exception ex)
                    {
                        return Json(new { error = "exception", message = "Произошол збой при загрузке файла. " + ex });
                    }

                    img.Userid = HttpContext.User.Identity.GetUserId();
                    img.Articletempid = int.Parse(Request.Form.GetValues("articleTempId").First());
                    img.Dir = path;
                    img.Imgname = newFileName;
                    string userId = HttpContext.User.Identity.GetUserId();
                    string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
                    System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(conStr);
                    connection.Open();
                    var cmd = new System.Data.SqlClient.SqlCommand("select max(npp) from Tempimages where Userid = '" + userId +
                        "' and Articletempid = " + img.Articletempid, connection);
                    object npp = cmd.ExecuteScalar();
                    img.Npp = string.IsNullOrEmpty(npp.ToString()) ? 1 : (int)npp + 1;

                    db.Tempimages.Add(img);
                    db.SaveChanges();

                    connection.Close();
                }
                else
                {
                    return Json(new { error = "exception", message = "Произошол збой при загрузке файла. Файл не выбран" });
                }
            }
            return Json(new { error = "ok", message = "Файл успешно сохранен", dir = img.Dir, fileName = img.Imgname, npp = img.Npp });
        }
    }
}
