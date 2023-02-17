using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class RepositoryExtenrions
    {
        public static async Task DeleteInBatchesAsync<TEntity, TPrimaryKey>(this IRepository<TEntity, TPrimaryKey> repository, Expression<Func<TEntity, bool>> predicate, IActiveUnitOfWork currentUnitOfWork, int batchSize = 100) where TEntity : class, IEntity<TPrimaryKey>
        {
            List<TEntity> records;
            do
            {
                records = await repository.GetAll().Where(predicate).Take(batchSize).ToListAsync();
                records.ForEach(record => repository.Delete(record));
                await currentUnitOfWork.SaveChangesAsync();
            }
            while (records.Any());
        }

        public static async Task HardDeleteInBatchesAsync<TEntity, TPrimaryKey>(this IRepository<TEntity, TPrimaryKey> repository, Expression<Func<TEntity, bool>> predicate, IActiveUnitOfWork currentUnitOfWork, int batchSize = 100) where TEntity : class, IEntity<TPrimaryKey>, ISoftDelete
        {
            List<TEntity> records;
            do
            {
                records = await repository.GetAll().Where(predicate).Take(batchSize).ToListAsync();
                records.ForEach(record => repository.HardDelete(record));
                await currentUnitOfWork.SaveChangesAsync();
            }
            while (records.Any());
        }
    }
}
