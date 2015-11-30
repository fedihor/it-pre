using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IT_Pre.Models
{
    public class ArticleImage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(400)]
        public string Title { get; set; }

        [Required]
        [StringLength(400)]
        public string Name { get; set; }

        //public int Article_Id { get; set; }

        [Required]
        public byte[] Imagefile { get; set; }



        public virtual Article Article { get; set; }
    }
}