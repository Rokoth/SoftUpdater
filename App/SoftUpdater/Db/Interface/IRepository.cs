using SoftUpdater.Db.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Db.Interface
{
    public interface IRepository<T> where T : IEntity
    {
        /// <summary>
        /// Get model list with paging
        /// </summary>
        /// <param name="filter">filter</param>
        /// <param name="token">token</param>
        /// <returns>PagedResult<T></returns>
        Task<Contract.Model.PagedResult<T>> GetAsync(Filter<T> filter, CancellationToken token);
        Task<Contract.Model.PagedResult<T>> GetAsyncDeleted(Filter<T> filter, CancellationToken token);
        /// <summary>
        /// Get item of model by id
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="token">token</param>
        /// <returns></returns>
        Task<T> GetAsync(Guid id, CancellationToken token);
        Task<T> GetAsyncDeleted(Guid id, CancellationToken token);
        /// <summary>
        /// add model to db
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="withSave">save after add</param>
        /// <param name="token">token</param>
        /// <returns></returns>
        Task<T> AddAsync(T entity, bool withSave, CancellationToken token);
        Task<T> DeleteAsync(T entity, bool v, CancellationToken token);
        Task<T> UpdateAsync(T entry, bool v, CancellationToken token);
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
