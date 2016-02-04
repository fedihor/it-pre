using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_Pre.Models
{
    public class Proglang
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [StringLength(3)]
        public string Shortname { get; set; }

        [Required]
        [StringLength(15)]
        public string Longname { get; set; }
        
        public virtual ICollection<Article> Articles { get; set; }

        public Proglang()
        {
            Articles = new List<Article>();
        }
        
        //???
        //public virtual ICollection<Article_Proglang> Articles_Proglangs { get; set; }





    }
    /*
    [Table("Articles_Proglangs")]
    public partial class Article_Proglang
    {
        public int Id { get; set; }

        //public int Article_Id { get; set; }

        //public int Proglang_Id { get; set; }

        public virtual Article Article { get; set; }

        public virtual Proglang Proglang { get; set; }
    }
    */
}