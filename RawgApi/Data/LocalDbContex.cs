using RawgApi.Models;
using Microsoft.EntityFrameworkCore;

namespace RawgApi.Data
{
    public class LocalDbContex : DbContext
    {
        public DbSet<Games> Games { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        {
            // Isso criará um arquivo chamado "meus_jogos.db" localmente
            optionsBuilder.UseSqlite("Data Source=localdb.db");
        }   
    }
}
