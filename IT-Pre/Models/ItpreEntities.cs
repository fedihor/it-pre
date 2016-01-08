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
        public virtual DbSet<ArticleImage> ArticleImages { get; set; }
        public virtual DbSet<Tempimage> Tempimages { get; set; }
        public virtual DbSet<ArticleRate> ArticleRates { get; set; }

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

            //modelBuilder.Entity<ArticleRate>()
            //    .HasRequired(e => e.Article)
            //    .WithRequiredDependent(e => e.ArticleRates)
            //    .WillCascadeOnDelete(true);


            //modelBuilder.Entity<Article>()
            //   .HasMany(e => e.ArticleImages)
            //   .WithRequired(e => e.Article)
            //   .WillCascadeOnDelete(false);



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

    public static class DeleteTableRows
    {
        public static System.Data.SqlClient.SqlConnection AdoDbConnection(string connectionStringName = "DefaultConnection")
        {
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName].ToString();
            return new System.Data.SqlClient.SqlConnection(conStr);
        }

        public static int DeleteFromSqlTable(string tableName, string columnName, string key, System.Data.SqlClient.SqlConnection connection)
        {
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            cmd.CommandText = "delete from " + tableName + " where " + columnName + " = " + key;
            cmd.Connection = connection;
            return cmd.ExecuteNonQuery();
        }
    }
}
