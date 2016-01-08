using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IT_Pre.Models
{
    public class ArticleRate
    {

        public int Id { get; set; }
        public int Plus { get; set; }
        public int Minus { get; set; }

        public virtual Article Article { get; set; }
    }
}