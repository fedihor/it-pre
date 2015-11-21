namespace IT_Pre.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public partial class ItpreEntities : DbContext
    {
        public ItpreEntities()
            : base("name=DefaultConnection")
        {
        }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    throw new UnintentionalCodeFirstException();
        //}

        public virtual DbSet<Article> Articles { get; set; }
    }
}
