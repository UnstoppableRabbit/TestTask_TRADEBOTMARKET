﻿using ArbitrageDomain.DbLayer;
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
        public async Task UpdateSpreadAsync(int id, decimal firstFuturesPrice, decimal secondFuturesPrice)
        {
            try
            {
                var currentSpread = await _context.PairSpreads.FirstOrDefaultAsync(x => x.Id == id);
                if (currentSpread == null)
                    throw new Exception($"Spread with id: {id} does not exists");
                currentSpread.FirstFuturesPrice = firstFuturesPrice;
                currentSpread.SecondFuturesPrice = secondFuturesPrice;
                currentSpread.SpreadValue = firstFuturesPrice - secondFuturesPrice;
                await _context.SaveChangesAsync();
            }
            catch //здесь я предполагаю что наш сервис не работает с UI / конечным пользователем, поэтому могу пробросить ошибку наверх
            {
                throw;
            }
        }

        public void UpdateSpread(int id, decimal firstFuturesPrice, decimal secondFuturesPrice)
        {
            try
            {
                var currentSpread =  _context.PairSpreads.FirstOrDefault(x => x.Id == id);
                if (currentSpread == null)
                    throw new Exception($"Spread with id: {id} does not exists");
                currentSpread.FirstFuturesPrice = firstFuturesPrice;
                currentSpread.SecondFuturesPrice = secondFuturesPrice;
                currentSpread.SpreadValue = firstFuturesPrice - secondFuturesPrice;
                _context.SaveChanges();
            }
            catch //здесь я предполагаю что наш сервис не работает с UI / конечным пользователем, поэтому могу пробросить ошибку наверх
            {
                throw;
            }
        }

        /// <summary>
        /// Trying to find spread with same futures pair and date, where prices are not equal
        /// </summary>
        /// <returns>exists, spreadId</returns>
        public async Task<(bool, int)> AnySpreadToUpdate(PairSpread spread)
        {
            if (await _context.PairSpreads.AnyAsync(x => x.FirstFutures == spread.FirstFutures && x.SecondFutures == spread.SecondFutures && x.Date == spread.Date && 
                                                            (x.FirstFuturesPrice != spread.FirstFuturesPrice || x.SecondFuturesPrice != spread.SecondFuturesPrice)))
                return (true, (await _context.PairSpreads.FirstAsync(x => x.FirstFutures == spread.FirstFutures && x.SecondFutures == spread.SecondFutures && x.Date == spread.Date)).Id);
            else
                return (false, 0);
        }

        /// <summary>
        /// Trying to find spread with same futures pair and date
        /// </summary>
        /// <returns>exists, spreadId</returns>
        public async Task<(bool, int)> AnySpreadLikeThis(PairSpread spread)
        {
            if (await _context.PairSpreads.AnyAsync(x => x.FirstFutures == spread.FirstFutures && x.SecondFutures == spread.SecondFutures && x.Date == spread.Date))
                return (true, (await _context.PairSpreads.FirstAsync(x => x.FirstFutures == spread.FirstFutures && x.SecondFutures == spread.SecondFutures && x.Date == spread.Date)).Id);
            else
                return (false, 0);
        }
    }
}
