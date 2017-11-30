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
        public virtual DbSet<Sourse> Sourses { get; set; }

        public virtual DbSet<Data> Data { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Download>()
               .HasOptional(e => e.Data)
               .WithRequired(e => e.Download);

            

            modelBuilder.Entity<Sourse>()
                .Property(e => e.Url)
                .IsUnicode(false);
        }
    }
}
