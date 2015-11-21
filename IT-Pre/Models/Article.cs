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

        public virtual ArticleSubject ArticleSubject { get; set; }

        //public Article()
        //{
        //    ArticleSubject = new HashSet<ArticlePicture>();
        //}
    }
}
