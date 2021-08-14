using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{

    public abstract class DataService<TEntity, Tdto, TFilter, TCreator, TUpdater> :
        DataGetService<TEntity, Tdto, TFilter>, IAddDataService<Tdto, TCreator>, IUpdateDataService<Tdto, TUpdater>, IDeleteDataService<Tdto>
          where TEntity : Db.Model.IEntity
          where TUpdater: Contract.Model.IEntity
          where Tdto : Contract.Model.Entity
          where TFilter : Contract.Model.Filter<Tdto>
    {       

        public DataService(IServiceProvider serviceProvider): base(serviceProvider)
        {
            
        }

        protected virtual async Task<TEntity> MapToEntityAdd(TCreator creator)
        {
            var result = _mapper.Map<TEntity>(creator);
            result.Id = Guid.NewGuid();
            result.IsDeleted = false;
            result.VersionDate = DateTimeOffset.Now;
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// add item method
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Tdto> AddAsync(TCreator creator, CancellationToken token)
        {
            return await ExecuteAsync(async (repo) =>
            {
                var entity = await MapToEntityAdd(creator);
                var result = await repo.AddAsync(entity, true, token);
                var prepare = _mapper.Map<Tdto>(result);
                prepare = await Enrich(prepare, token);
                return prepare;
            });
        }

        protected abstract TEntity UpdateFillFields(TUpdater entity, TEntity entry);

        public async Task<Tdto> UpdateAsync(TUpdater entity, CancellationToken token)
        {
            return await ExecuteAsync(async (repo) =>
            {
                var entry = await repo.GetAsync(entity.Id, token);
                entry = UpdateFillFields(entity, entry);
                TEntity result = await repo.UpdateAsync(entry, true, token);
                var prepare = _mapper.Map<Tdto>(result);
                prepare = await Enrich(prepare, token);
                return prepare;
            });
        }

        public async Task<Tdto> DeleteAsync(Guid id, CancellationToken token)
        {
            return await ExecuteAsync(async (repo) =>
            {
                var entity = await repo.GetAsync(id, token);
                if (entity == null) throw new DataServiceException($"Entity with id = {id} not found in DB");
                entity = await repo.DeleteAsync(entity, true, token);
                var prepare = _mapper.Map<Tdto>(entity);
                prepare = await Enrich(prepare, token);
                return prepare;
            });
        }
    }
}
