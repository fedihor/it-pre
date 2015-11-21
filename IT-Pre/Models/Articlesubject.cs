namespace IT_Pre.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ArticleSubject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string Asubject { get; set; }

        public virtual ICollection<Article> Articles { get; set; }
    }
}
