namespace IT_Pre.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class DraftArticle
    {
        public int Id { get; set; }

        [StringLength(400)]
        public string Title { get; set; }

        [AllowHtml]
        public string Articletext { get; set; }

        [StringLength(128)]
        public string Userid { get; set; }

        //public DateTime Adate { get; set; }

        public int? Asubject1 { get; set; }

        public string Proglangs { get; set; }

        public string Tags { get; set; }

        public int? Article_Id { get; set; }

        //public virtual ArticleSubject ArticleSubject { get; set; }
    }
}
