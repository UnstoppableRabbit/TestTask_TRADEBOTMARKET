using ArbitrageDomain.Model;
using Microsoft.EntityFrameworkCore;
namespace ArbitrageDomain.DbLayer
{
    public class SpreadDbContext : DbContext
    {
        public DbSet<PairSpread> PairSpreads { get; set; }
       
        private string connectionString;
        public SpreadDbContext(string connectionString)
        {
            this.connectionString = connectionString; 
        }
        public SpreadDbContext(DbContextOptions<SpreadDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = this.connectionString;
                optionsBuilder.UseNpgsql(connectionString);
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PairSpread>().HasKey(p => p.Id);

            modelBuilder.Entity<PairSpread>()
                .HasIndex(p => new { p.Date, p.FirstFutures, p.SecondFutures })
                .IsUnique();
        }
    }
}

