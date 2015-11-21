namespace IT_Pre.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Articlesubject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string Asubject { get; set; }

        public virtual List<Article> Articles { get; set; }
}
}
