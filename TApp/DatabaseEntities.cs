namespace TApp
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DatabaseEntities : DbContext
    {
        public DatabaseEntities()
            : base("name=DatabaseEntities")
        {
        }

        public virtual DbSet<Download> Downloads { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<Sourse> Sourses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Language>()
                .Property(e => e.Language1)
                .IsUnicode(false);

            modelBuilder.Entity<Language>()
                .HasMany(e => e.Sourses)
                .WithMany(e => e.Languages)
                .Map(m => m.ToTable("RepositoryLanguages").MapLeftKey("Language").MapRightKey("RepositoryID"));

            modelBuilder.Entity<Sourse>()
                .Property(e => e.Url)
                .IsUnicode(false);
        }
    }
}
