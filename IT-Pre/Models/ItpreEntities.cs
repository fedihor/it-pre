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
        public virtual DbSet<ArticleSubject> ArticleSubjects { get; set; }
        public virtual DbSet<Proglang> Proglangs { get; set; }
        //public virtual DbSet<Article_Proglang> Articles_Proglangs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Article>()
                .HasMany(c => c.Proglangs)
                .WithMany(s => s.Articles)
                .Map(t => t.MapLeftKey("Article_Id")
                .MapRightKey("Proglang_Id")
                .ToTable("Articles_Proglangs"));


            modelBuilder.Entity<ArticleSubject>()
                .HasMany(e => e.Articles)
                .WithRequired(e => e.ArticleSubject)
                .HasForeignKey(e => e.Asubject1)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Article>()
            //    .HasMany(e => e.Articles_Proglangs)
            //    .WithRequired(e => e.Article)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Proglang>()
            //    .HasMany(e => e.Articles_Proglangs)
            //    .WithRequired(e => e.Proglang)
            //    .WillCascadeOnDelete(false);
        }
    }
}
