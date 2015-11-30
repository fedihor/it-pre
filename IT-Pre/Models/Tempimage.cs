using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_Pre.Models
{
    public class Tempimage
    {
        [Required]
        [StringLength(128)]
        public string Userid { get; set; }

        [Required]
        [Key]
        [Column(Order = 1)]
        public int Articletempid { get; set; }

        [Required]
        [StringLength(256)]
        public string Dir { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        [StringLength(60)]
        public string Imgname { get; set; }

        [Required]
        public int Npp { get; set; }
    }
}