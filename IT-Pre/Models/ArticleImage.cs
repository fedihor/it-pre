using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IT_Pre.Models
{
    public class ArticleImage
    {
        public int Id { get; set; }

        //[Required]
        [StringLength(400)]
        public string Imgtitle { get; set; }

        [StringLength(256)]
        public string Imgdir { get; set; }
        
        [StringLength(256)]
        public string Imgname { get; set; }

        //[Required]
        public byte[] Imgfile { get; set; }

        [Required]
        //[Key]
        //[Column(Order = 2)]
        public int Npp { get; set; }

        public DateTime? Imgdate { get; set; }

        [Required]
        [StringLength(128)]
        public string Userid { get; set; }

        //public int Article_Id { get; set; }

        //[Key]
        //[Column(Order = 1)]
        public virtual Article Article { get; set; }
    }
}