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

        public int Viewcounter { get; set; }

        [Required]
        public virtual ICollection<Proglang> Proglangs { get; set; }

        public virtual ICollection<ArticleImage> ArticleImages { get; set; }


        public Article()
        {
            Proglangs = new List<Proglang>();
        }



        //??
        public virtual ArticleSubject ArticleSubject { get; set; }
    }

    public class ArticleAdditionData
    {
        public List<SelectListItem> Asubjects { get; set; }
        public List<Proglang> Proglangs { get; set; }
        public List<Tempimage> Tempimages { get; set; }
    }
}
