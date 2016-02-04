namespace IT_Pre.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class Article
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(400)]
        public string Title { get; set; }

        [Required]
        [AllowHtml]
        public string Articletext { get; set; }

        [Required]
        [StringLength(128)]
        public string Userid { get; set; }

        //public DateTime Adate { get; set; }

        public int? Asubject1 { get; set; }

        public int Viewcounter { get; set; }

        [Required]
        public virtual ICollection<Proglang> Proglangs { get; set; }

        public virtual ICollection<ArticleImage> ArticleImages { get; set; }

        public virtual ICollection<ArticleRate> ArticleRates { get; set; }

        public Article()
        {
            Proglangs = new List<Proglang>();
            ArticleImages = new List<ArticleImage>();
            ArticleRates = new List<ArticleRate>();
        }

        //??
        public virtual ArticleSubject ArticleSubject { get; set; }


        public void DeleteReferencedRows()
        {
            var connection = DeleteTableRows.AdoDbConnection();
            connection.Open();
            int delLangResult = DeleteTableRows.DeleteFromSqlTable("Articles_Proglangs", "Article_Id", this.Id.ToString(), connection);
            int delTagResult = DeleteTableRows.DeleteFromSqlTable("Articles_Tags", "Article_Id", this.Id.ToString(), connection);
            connection.Close();
        }
    }

    public class ArticleAdditionData
    {
        public List<SelectListItem> Asubjects { get; set; }
        public List<Proglang> Proglangs { get; set; }
        public List<DraftArticleImage> DraftArticleImags { get; set; }
    }
}
