using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUpdater.Db.Context;
using SoftUpdater.Db.Interface;
using SoftUpdater.Db.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace SoftUpdater.Db.Repository
{
    public class Repository<T> : IRepository<T> where T : class, IEntity 
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public Repository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<Repository<T>>>();
        }

        /// <summary>
        /// Метод добавления модели в базу
        /// </summary>
        /// <param name="entity">модель</param>
        /// <param name="withSave">с сохраннеием</param>
        /// <param name="token">токен</param>
        /// <returns>модель</returns>
        public async Task<T> AddAsync(T entity, bool withSave, CancellationToken token)
        {
            return await ExecuteAsync(async (context) => {
                var item = context.Set<T>().Add(entity).Entity;
                if (withSave) await context.SaveChangesAsync();
                return item;
            }, "AddAsync");
        }
               
        public async Task<T> DeleteAsync(T entity, bool withSave, CancellationToken token)
        {
            return await ExecuteAsync(async (context) => {
                entity.IsDeleted = true;
                var item = context.Set<T>().Update(entity).Entity;
                if (withSave) await context.SaveChangesAsync();
                return item;
            }, "DeleteAsync");
        }

        /// <summary>
        /// Метод получения отфильтрованного списка моделей с постраничной отдачей
        /// </summary>
        /// <param name="filter">фильтр</param>
        /// <param name="token">токен</param>
        /// <returns>список моделей</returns>
        public async Task<Contract.Model.PagedResult<T>> GetAsync(Filter<T> filter, CancellationToken token)
        {
            return await ExecuteAsync(async (context) => {
                var all = context.Set<T>().Where(s=>!s.IsDeleted).Where(filter.Selector);
                if (!string.IsNullOrEmpty(filter.Sort))
                {
                    all = all.OrderBy(filter.Sort);
                }
                var result = await all
                    .Skip(filter.Size * filter.Page)
                    .Take(filter.Size)
                    .ToListAsync();
                var count = await all.CountAsync();
                var pageCount = Math.Max(((count % filter.Size) == 0) ? (count / filter.Size) : ((count / filter.Size) + 1), 1);
                return new Contract.Model.PagedResult<T>(result, pageCount);
            }, "GetAsync");
        }

        /// <summary>
        /// Метод получения модели по id
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="token">token</param>
        /// <returns></returns>
        public async Task<T> GetAsync(Guid id, CancellationToken token)
        {
            return await ExecuteAsync(async (context) => {
                return await context.Set<T>()
                    .Where(s => !s.IsDeleted && s.Id == id).FirstOrDefaultAsync();
            }, "GetAsync");
        }

        public async Task<Contract.Model.PagedResult<T>> GetAsyncDeleted(Filter<T> filter, CancellationToken token)
        {
            return await ExecuteAsync(async (context) => {
                var all = context.Set<T>().Where(filter.Selector);
                if (!string.IsNullOrEmpty(filter.Sort))
                {
                    all = all.OrderBy(filter.Sort);
                }
                var result = await all
                    .Skip(filter.Size * filter.Page)
                    .Take(filter.Size)
                    .ToListAsync();
                var count = await all.CountAsync();
                var pageCount = Math.Max(((count % filter.Size) == 0) ? (count / filter.Size) : ((count / filter.Size) + 1),1);
                return new Contract.Model.PagedResult<T>(result, pageCount);
            }, "GetAsyncDeleted");
        }

        public async Task<T> GetAsyncDeleted(Guid id, CancellationToken token)
        {
            return await ExecuteAsync(async (context) => {
                return await context.Set<T>()
                    .Where(s => s.Id == id).FirstOrDefaultAsync();
            }, "GetAsync");
        }

        public async Task<T> UpdateAsync(T entity, bool withSave, CancellationToken token)
        {
            return await ExecuteAsync(async (context) => {                
                var item = context.Set<T>().Update(entity).Entity;
                if (withSave) await context.SaveChangesAsync();
                return item;
            }, "UpdateAsync");
        }

        private async Task<TEx> ExecuteAsync<TEx>(Func<DbPgContext, Task<TEx>> action, string method)
        {
            try
            {
                var context = _serviceProvider.GetRequiredService<DbPgContext>();
                return await action(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в методе {method} Repository: {ex.Message} {ex.StackTrace}");
                throw new RepositoryException($"Ошибка в методе {method} Repository: {ex.Message}");
            }
        }
    }

    [Serializable]
    public class RepositoryException : Exception
    {
        /// <summary>
        /// default ctor
        /// </summary>
        public RepositoryException()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        public RepositoryException(string message) : base(message)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
