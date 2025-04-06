using ArbitrageDomain.DbLayer;
using ArbitrageDomain.Model;
using ArbitrageDomain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ArbitrageRepository
{
    public class SpreadRepository : ISpreadRepository
    {
        private readonly SpreadDbContext _context;

        public SpreadRepository(SpreadDbContext context)
        {
            _context = context;
        }

        public async Task SaveSpreadAsync(PairSpread spread)
        {
            try {
                await _context.PairSpreads.AddAsync(spread);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx &&
                                   pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new Exception("Данный spread уже существует");
            }
            catch //здесь я предполагаю что наш сервис не работает с UI / конечным пользователем, поэтому могу пробросить ошибку наверх
            { 
                throw; 
            }
        }

        public void SaveSpread(PairSpread spread)
        {
            try
            {
                _context.PairSpreads.Add(spread);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx &&
                                   pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new Exception("Данный spread уже существует");
            }
            catch //здесь я предполагаю что наш сервис не работает с UI / конечным пользователем, поэтому могу пробросить ошибку наверх 
            {
                throw;
            }
        }
    }
}
