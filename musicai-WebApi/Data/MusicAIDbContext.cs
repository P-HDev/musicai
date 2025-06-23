using Microsoft.EntityFrameworkCore;

namespace musicai.Data
{
    public class MusicAIDbContext : DbContext
    {
        public MusicAIDbContext(DbContextOptions<MusicAIDbContext> options) : base(options)
        {
        }

        // Aqui serão adicionados os DbSets para as entidades do banco de dados
        // Exemplo: public DbSet<Playlist> Playlists { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Aqui serão configurados os mapeamentos das entidades
            // Exemplo: modelBuilder.Entity<Playlist>().ToTable("playlists");
        }
    }
}
