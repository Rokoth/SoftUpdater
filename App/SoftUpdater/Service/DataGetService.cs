using AutoMapper;
using SoftUpdater.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public abstract class DataGetService<TEntity, Tdto, TFilter> :
        IGetDataService<Tdto, TFilter>
        where TEntity : Db.Model.IEntity
        where Tdto : Contract.Model.Entity
        where TFilter : Contract.Model.Filter<Tdto>
    {
        protected IServiceProvider _serviceProvider;
        protected IMapper _mapper;
        private readonly IErrorNotifyService _errorNotifyService;

        protected abstract string DefaultSort { get; }

        /// <summary>
        /// function for modify client filter to db filter
        /// </summary>
        protected abstract Expression<Func<TEntity, bool>> GetFilter(TFilter filter);

        protected virtual Func<Db.Model.Filter<TEntity>, CancellationToken, Task<Contract.Model.PagedResult<TEntity>>> GetListFunc(Db.Interface.IRepository<TEntity> repo)
        {
            return repo.GetAsync;
        }

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected virtual async Task<Tdto> Enrich(Tdto entity, CancellationToken token)
        {
            return entity;
        }

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected virtual async Task<IEnumerable<Tdto>> Enrich(IEnumerable<Tdto> entities, CancellationToken token)
        {
            return entities;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public DataGetService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _mapper = _serviceProvider.GetRequiredService<IMapper>();
            _errorNotifyService = _serviceProvider.GetRequiredService<IErrorNotifyService>();
        }

        /// <summary>
        /// Get items method
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Contract.Model.PagedResult<Tdto>> GetAsync(TFilter filter, CancellationToken token)
        {
            return await ExecuteAsync(async (repo) =>
            {                
                string sort = filter.Sort;
                if (string.IsNullOrEmpty(sort))
                {
                    sort = DefaultSort;
                }
                
                var result = await GetListFunc(repo)(new Db.Model.Filter<TEntity>
                {
                    Size = filter.Size,
                    Page = filter.Page,
                    Sort = sort,
                    Selector = GetFilter(filter)
                }, token);
                var prepare = result.Data.Select(s => _mapper.Map<Tdto>(s));
                prepare = await Enrich(prepare, token);
                return new Contract.Model.PagedResult<Tdto>(prepare, result.PageCount);
            });
        }

        /// <summary>
        /// get item method
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Tdto> GetAsync(Guid id, CancellationToken token)
        {
            return await ExecuteAsync(async (repo) =>
            {
                var result = await repo.GetAsync(id, token);
                if (result == null) throw new Exception("Запись в базе данных не найдена");
                var prepare = _mapper.Map<Tdto>(result);
                prepare = await Enrich(prepare, token);
                return prepare;
            });
        }

        /// <summary>
        /// execution wrapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="execute"></param>
        /// <returns></returns>
        protected async Task<T> ExecuteAsync<T>(Func<Db.Interface.IRepository<TEntity>, Task<T>> execute)
        {
            try
            {
                var repo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<TEntity>>();
                return await execute(repo);
            }
            catch (DataServiceException ex)
            {
                await _errorNotifyService.Send($"Ошибка в DataService: {ex.Message} {ex.StackTrace}");
                throw;
            }
            catch (Db.Interface.RepositoryException ex)
            {
                throw new DataServiceException(ex.Message);
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Необработанная ошибка в DataService: {ex.Message} {ex.StackTrace}");
                throw;
            }
        }
    }
}
