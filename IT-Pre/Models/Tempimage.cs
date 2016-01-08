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
        public int Article_Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Dir { get; set; }

        [Required]
        [StringLength(60)]
        public string Imgname { get; set; }

        public string Imgtext { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        public int Npp { get; set; }

        //public DateTime Imgdate { get; set; }
    }
}