using Microsoft.EntityFrameworkCore;
using RawgApi.Models;

namespace RawgApi.Data
{
    public class LocalDbContex : DbContext
    {
        public DbSet<Games> Games { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=localdb.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Games>().ToTable("Games");

            modelBuilder.Entity<Games>()
                .HasKey(g => g.Id);

            modelBuilder.Entity<Games>()
                .Property(g => g.Nome)
                .IsRequired();

            modelBuilder.Entity<Games>()
                .Property(g => g.Descricao)
                .IsRequired();

            modelBuilder.Entity<Games>()
                .Property(g => g.ImagemUrl)
                .IsRequired();

            modelBuilder.Entity<Games>()
                .Property(g => g.Avaliacao)
                .IsRequired();

            modelBuilder.Entity<Games>()
                .Property(g => g.Classificacao)
                .IsRequired();
        }
    }
}