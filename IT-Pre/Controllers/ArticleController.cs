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
using System.Text.RegularExpressions;

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
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Create/" + new Random().Next(10000, 99999));
            }

            Article article = CreateEdit(id, true);

            return View(article);
        }

        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Article article = CreateEdit(id, false);

            if (article == null)
            {
                return HttpNotFound();
            }

            return View(article);
        }

        private Article CreateEdit(int? id, bool isNew)
        {
            Article article;

            if (isNew)
            {
                article = new Article()
                {
                    Id = (int)id
                };
            }
            else
            {
                article = db.Articles.Find(id);

                if (article != null)
                {
                    StringBuilder sb = new StringBuilder(HttpUtility.HtmlDecode(article.Articletext));
                    article.Articletext = sb.ToString();
                }
            }

            ArticleAdditionData articleAdditionData = new ArticleAdditionData();
            GetCurrentArticleData(article, articleAdditionData);
            ViewBag.Asubject1 = articleAdditionData.Asubjects;
            ViewBag.articleAdditionData = articleAdditionData;

            return article;
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
                .Where(e => e.Article_Id == article.Id).ToList();
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
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "Id,Title,Articletext,Userid,Adate,Asubject1,Proglangs")] Article article, int[] selectedProglangs)
        {

            bool saveResult = CreateEdit(article, selectedProglangs, true);

            if (saveResult)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Asubjects = ViewBag.articleAdditionData.Asubjects;
            ViewBag.d = ViewBag.Asubject1[1].Selected;

            /*
            bool error = false;

            if (ModelState.IsValid && selectedProglangs != null && article.Asubject1 != 0)
            {
                int tempId = article.Id;
                article.Id = 0;
                article.Proglangs.Clear();

                foreach (var lang in db.Proglangs.Where(l => selectedProglangs.Contains(l.Id)))
                {
                    article.Proglangs.Add(lang);
                }

                Article newArticle = db.Articles.Add(article);

                db.SaveChanges();

                string imgResult = SaveImages(newArticle, tempId);

                ArticleRate articlerate = new ArticleRate()
                {
                    Article = newArticle,
                    //Article_Id = newArticle.Id,
                    Plus = 10,
                    Minus = 7
                };

                db.ArticleRates.Add(articlerate);
                db.SaveChanges();

                if (imgResult.CompareTo("true") != 0)
                {
                    ModelState.AddModelError("Images", "Произошел сбой при сохранении изображений. Попробуйте еще раз. " + imgResult);
                    error = true;
                }

                removeArticleCookies(tempId);

                if (!error)
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                error = true;

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
            }

            if (error)
            {
                ArticleAdditionData articleAdditionData = new ArticleAdditionData();

                GetCurrentArticleData(article, articleAdditionData);

                ViewBag.Asubject1 = articleAdditionData.Asubjects;

                ViewBag.articleAdditionData = articleAdditionData;

                article.Articletext = EncodeString(article.Articletext);
            }
            */
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "Id,Title,Articletext,Userid,Adate,Asubject1,Proglangs")] Article article, int[] selectedProglangs)
        {
            bool saveResult = CreateEdit(article, selectedProglangs, false);

            if (saveResult)
            {
                return RedirectToAction("Index");
            }
            /*
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


                ArticleAdditionData articleAdditionData = new ArticleAdditionData();

                GetCurrentArticleData(article, articleAdditionData);

                ViewBag.articleAdditionData = articleAdditionData;

                ViewBag.Asubject1 = new SelectList(db.ArticleSubjects.OrderBy(s => s.Id), "Id", "Asubject", article.Asubject1);

                //ViewBag.Proglangs = db.Proglangs.ToList();

                article.Articletext = EncodeString(article.Articletext);
            }*/

            return View(article);
        }

        private bool CreateEdit(Article article, int[] selectedProglangs, bool isNew = true)
        {
            bool error = false;

            List<string> errImgTags = GetErrImgTags(article);

            if (ModelState.IsValid && selectedProglangs != null && article.Asubject1 != 0 && errImgTags == null)
            {
                Article newArticle;
                int tempId = article.Id;

                if (isNew)
                {
                    newArticle = new Article();
                }
                else
                {
                    newArticle = db.Articles.Find(article.Id);
                    newArticle.Proglangs.Clear();
                }

                newArticle.Title = article.Title;
                newArticle.Articletext = article.Articletext;
                newArticle.Asubject1 = article.Asubject1;
                newArticle.Userid = article.Userid;

                foreach (var lang in db.Proglangs.Where(l => selectedProglangs.Contains(l.Id)))
                {
                    newArticle.Proglangs.Add(lang);
                }

                if (isNew)
                {
                    newArticle = db.Articles.Add(newArticle);
                    ArticleRate articlerate = new ArticleRate()
                    {
                        Article = newArticle,
                        Plus = 0,
                        Minus = 0
                    };
                    db.ArticleRates.Add(articlerate);
                }
                else
                {
                    db.Entry(newArticle).State = EntityState.Modified;
                }

                db.SaveChanges();

                string imgResult = SaveImages(newArticle, tempId);

                if (imgResult.CompareTo("true") != 0)
                {
                    ModelState.AddModelError("Images", "Произошел сбой при сохранении изображений. Попробуйте еще раз. " + imgResult);
                    error = true;
                }

                removeArticleCookies(tempId);

                if (!error)
                {
                    return true;
                    //return RedirectToAction("Index");
                }
            }
            else
            {
                error = true;

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

                if (errImgTags != null)
                {
                    ModelState.AddModelError("Articletext", "В статье находятся ошибочно вставленные теги изображения: " + String.Join(", ", errImgTags));
                }
            }

            if (error)
            {
                ArticleAdditionData articleAdditionData = new ArticleAdditionData();

                GetCurrentArticleData(article, articleAdditionData);

                ViewBag.Asubject1 = articleAdditionData.Asubjects;

                ViewBag.articleAdditionData = articleAdditionData;

                //article.Articletext = EncodeString(article.Articletext);
                //article.Asubject1 = int.Parse(articleAdditionData.Asubjects.Where(i => i.Selected == true).First().Value);
                //article_Asubject1
            }

            return false;

        }

        //регулярні вирази для пошуку в тексті тегів зображення та виявлення помилково внесених тегів зображення
        private Regex imgTagPattern = new Regex(@"<и([0-9]+)>(.*?)</и>", RegexOptions.IgnoreCase);
        private Regex imgOpenTagPattern = new Regex(@"<и[0-9]+>", RegexOptions.IgnoreCase);
        private Regex imgCloseTagPattern = new Regex(@"</и>", RegexOptions.IgnoreCase);

        private List<string> GetErrImgTags(Article newArticle)
        {
            if (newArticle.Articletext != null)
            {
                List<string> errTags = new List<string>();

                foreach (var item in imgTagPattern.Matches(newArticle.Articletext))
                {
                    int countImgTagErr = imgOpenTagPattern.Matches(item.ToString()).Count;

                    if (countImgTagErr > 1)
                    {
                        errTags.Add(@"""" + item.ToString() + @"""");
                    }
                }

                StringBuilder artTxtWithoutImgTags = new StringBuilder(imgTagPattern.Replace(newArticle.Articletext, String.Empty));

                foreach (var item in imgOpenTagPattern.Matches(artTxtWithoutImgTags.ToString()))
                {
                    errTags.Add(@"""" + item.ToString() + @"""");
                }
                foreach (var item in imgCloseTagPattern.Matches(artTxtWithoutImgTags.ToString()))
                {
                    errTags.Add(@"""" + item.ToString() + @"""");
                }

                string userId = HttpContext.User.Identity.GetUserId();

                List<int> allImgsNpp = new List<int>();

                foreach (var tempimage in db.Tempimages.Where(i => i.Article_Id == newArticle.Id).Where(i => i.Userid == userId).ToList())
                {
                    allImgsNpp.Add(tempimage.Npp);
                }
                foreach (var articleImage in db.ArticleImages.Where(i => i.Article.Id == newArticle.Id).Where(i => i.Userid == userId).ToList())
                {
                    allImgsNpp.Add(articleImage.Npp);
                }

                foreach (Match imgTag in Regex.Matches(newArticle.Articletext, imgTagPattern.ToString(), RegexOptions.IgnoreCase))
                {
                    int imgTagNpp = Convert.ToInt32(imgTag.Groups[1].Value);

                    if (!allImgsNpp.Contains(imgTagNpp))
                    {
                        errTags.Add(@"""" + imgTag.ToString() + @""" (изображения с таким номером нет)");
                    }
                }

                return errTags.Count == 0 ? null : errTags;
            }

            return null;
        }

        // зберігаємо закачані картинки з тимчасової папки в постійну базу данних
        private string SaveImages(Article newArticle, int tempId)
        {
            string userId = User.Identity.GetUserId();

            List<string> imgTags = new List<string>();

            int nextNpp = GetNextImgNpp(newArticle.Id, "ArticleImages");

            foreach (Match imgTag in Regex.Matches(newArticle.Articletext, imgTagPattern.ToString(), RegexOptions.IgnoreCase))
            {
                int imgTagNpp = Convert.ToInt32(imgTag.Groups[1].Value);

                Tempimage tempimg = db.Tempimages.Where(i => i.Article_Id == tempId).Where(i => i.Npp == imgTagNpp).Where(i => i.Userid == userId).First();

                if (tempimg == null)
                {
                    continue;
                }

                byte[] imageData = null;

                try
                {
                    using (FileStream fs = new FileStream(Server.MapPath("~" + tempimg.Dir + tempimg.Imgname), FileMode.Open))
                    {
                        imageData = new byte[fs.Length];
                        fs.Read(imageData, 0, imageData.Length);
                    }
                }
                catch (Exception ex)
                {
                    return "Error when reading images in FileStream " + ex.Message;
                }

                ArticleImage img = new ArticleImage
                {
                    Article = newArticle,
                    Imgtitle = imgTag.Groups[2].Value,
                    Imgname = tempimg.Imgname,
                    Imgfile = imageData,
                    Npp = nextNpp++,
                    Imgdate = DateTime.Now,
                    Userid = newArticle.Userid
                };

                ArticleImage addImgResul = new ArticleImage();

                try
                {
                    addImgResul = db.ArticleImages.Add(img);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return "Error when adding image to ArticleImages " + ex.Message;
                }

                try
                {
                    if (addImgResul != null)
                    {
                        db.Tempimages.Remove(tempimg);
                        db.SaveChanges();

                        try
                        {
                            string tempImgToDelete = Request.MapPath("~" + tempimg.Dir + tempimg.Imgname);

                            if (System.IO.File.Exists(tempImgToDelete))
                            {
                                System.IO.File.Delete(tempImgToDelete);
                            }
                        }
                        catch (Exception ex)
                        {
                            return "Error when deleting image file from temp dir " + ex.Message;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return "Error when Removing image from Tempimages " + ex.Message;
                }
            }

            return "true";
        }

        private int GetNextImgNpp(int articleId, string tableName, string userId = "")
        {
            int nextNpp = 0;

            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(conStr);

            connection.Open();

            string query = @"select max(npp) from " + tableName + " where Article_Id = " + articleId;

            if (!string.IsNullOrEmpty(userId))
            {
                query += " and  Userid = '" + userId + "'";
            }

            var cmd = new System.Data.SqlClient.SqlCommand(query, connection);
            object maxNpp = cmd.ExecuteScalar();
            nextNpp = string.IsNullOrEmpty(maxNpp.ToString()) ? 1 : (int)maxNpp + 1;

            connection.Close();

            return nextNpp;
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



            //var connection = AdoDbConnection();
            //connection.Open();
            //int delLangResult = DeleteFromSqlTable("Articles_Proglangs", "Article_Id", article.Id.ToString(), connection);
            //int delTagResult = DeleteFromSqlTable("Articles_Tags", "Article_Id", article.Id.ToString(), connection);
            //connection.Close();

            article.DeleteReferencedRows();

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

            string pathToDir = @"C:\Users\Ihor\Documents\Visual Studio 2015\Projects\it-pre\IT-Pre\Files\UploadImages\";
            string newDirName = Request.Form.GetValues("Article_Id").First(); //Path.GetRandomFileName();
            DirectoryInfo newDir = Directory.CreateDirectory(pathToDir + newDirName + @"\");

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
                    string path = @"/Files/UploadImages/" + newDirName + @"/";
                    //string path = path + System.IO.Path.GetRandomFileName();
                    string originFileName = Path.GetFileName(upload.FileName);
                    string newFileName = Path.GetRandomFileName() + Path.GetExtension(originFileName);
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
                    img.Article_Id = int.Parse(Request.Form.GetValues("Article_Id").First());
                    img.Dir = path;
                    img.Imgname = newFileName;
                    string userId = HttpContext.User.Identity.GetUserId();

                    //int nextTempNpp = GetNextImgNpp(img.Article_Id, "Tempimages", userId);

                    //string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
                    //System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(conStr);
                    //connection.Open();
                    //var cmd = new System.Data.SqlClient.SqlCommand("select max(npp) from Tempimages where Userid = '" + userId +
                    //    "' and Article_Id = " + img.Article_Id, connection);
                    //object npp = cmd.ExecuteScalar();
                    //img.Npp = string.IsNullOrEmpty(npp.ToString()) ? 1 : (int)npp + 1;

                    img.Npp = GetNextImgNpp(img.Article_Id, "Tempimages", userId);

                    db.Tempimages.Add(img);
                    db.SaveChanges();

                    //connection.Close();
                }
                else
                {
                    return Json(new { error = "exception", message = "Произошол збой при загрузке файла. Файл не выбран" });
                }
            }
            return Json(new { error = "ok", message = "Файл успешно сохранен", dir = img.Dir, fileName = img.Imgname, npp = img.Npp });
        }

        private void removeArticleCookies(int id)
        {
            /*
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
            */
            /*
            HttpCookie asubject1Cookie = HttpContext.Request.Cookies.Get("Asubject1" + id);
            HttpCookie proglangsCookie = HttpContext.Request.Cookies.Get("selectedProglangs" + id);

            HttpContext.Response.Cookies.Remove("Title" + id);
            HttpContext.Response.Cookies.Remove("Articletext" + id);
            HttpContext.Response.Cookies.Remove("Asubject1" + id);
            */
        }
    }



}
