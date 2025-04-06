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
                //optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BinanceSpreads;Username=postgres;Password=414876776"); //для CodeFirst создания базы через Ef-tools (нужно будет еще убрать конструкторы для корректного создания бд через команду Update-database)
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

