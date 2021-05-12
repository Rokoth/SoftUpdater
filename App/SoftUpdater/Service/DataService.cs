﻿using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public class UserDataService : DataService<Db.Model.User, Contract.Model.User,
        Contract.Model.UserFilter, Contract.Model.UserCreator, Contract.Model.UserUpdater>
    {
        public UserDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.User, bool>> GetFilter(Contract.Model.UserFilter filter)
        {
            return s => filter.Name == null || s.Name.Contains(filter.Name);
        }

        protected override Db.Model.User MapToEntityAdd(Contract.Model.UserCreator creator)
        {
            var entity = base.MapToEntityAdd(creator);
            entity.Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(creator.Password));
            return entity;
        }

        protected override Db.Model.User UpdateFillFields(Contract.Model.UserUpdater entity, Db.Model.User entry)
        {
            entry.Description = entity.Description;
            entry.Login = entity.Login;
            entry.Name = entity.Name;
            if (entity.PasswordChanged)
            {
                entry.Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(entity.Password));
            }
            return entry;
        }

        protected override Func<Contract.Model.User, Contract.Model.User> EnrichFunc => null;

        protected override string DefaultSort => "Name";
        
    }

    public class ClientDataService : DataService<Db.Model.Client, Contract.Model.Client,
        Contract.Model.ClientFilter, Contract.Model.ClientCreator, Contract.Model.ClientUpdater>
    {
        public ClientDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.Client, bool>> GetFilter(Contract.Model.ClientFilter filter)
        {
            return s => (filter.Name == null || s.Name.Contains(filter.Name)) && (filter.UserId == null || s.UserId == filter.UserId);
        }

        protected override Db.Model.Client MapToEntityAdd(Contract.Model.ClientCreator creator)
        {
            var entity = base.MapToEntityAdd(creator);
            entity.Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(creator.Password));
            return entity;
        }

        protected override Db.Model.Client UpdateFillFields(Contract.Model.ClientUpdater entity, Db.Model.Client entry)
        {
            entry.Description = entity.Description;
            entry.Login = entity.Login;
            entry.Name = entity.Name;
            entry.UserId = entity.UserId;
            entry.BasePath = entity.BasePath;
            if (entity.PasswordChanged)
            {
                entry.Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(entity.Password));
            }
            return entry;
        }

        protected override Func<Contract.Model.Client, Contract.Model.Client> EnrichFunc => null;

        protected override string DefaultSort => "Name";

    }

    public class UserHistoryDataService : DataGetService<Db.Model.UserHistory, Contract.Model.UserHistory,
        Contract.Model.UserHistoryFilter>
    {
        public UserHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Func<Contract.Model.UserHistory, Contract.Model.UserHistory> EnrichFunc => null;

        protected override string DefaultSort => "Name";

        protected override Expression<Func<Db.Model.UserHistory, bool>> GetFilter(Contract.Model.UserHistoryFilter filter)
        {
            return s => (filter.Name == null || s.Name.Contains(filter.Name)) 
                && (filter.Id == null || s.Id == filter.Id);
        }
    }

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

        protected virtual TEntity MapToEntityAdd(TCreator creator)
        {
            var result = _mapper.Map<TEntity>(creator);
            result.Id = Guid.NewGuid();
            result.IsDeleted = false;
            result.VersionDate = DateTimeOffset.Now;
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
                var entity = MapToEntityAdd(creator);
                var result = await repo.AddAsync(entity, true, token);
                var prepare = _mapper.Map<Tdto>(result);
                if (EnrichFunc != null)
                {
                    prepare = EnrichFunc(prepare);
                }
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
                if (EnrichFunc != null)
                {
                    prepare = EnrichFunc(prepare);
                }
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
                if (EnrichFunc != null)
                {
                    prepare = EnrichFunc(prepare);
                }
                return prepare;
            });
        }
    }

    public interface IGetDataService<Tdto, TFilter>
        where Tdto : Contract.Model.Entity
        where TFilter : Contract.Model.Filter<Tdto>
    {
        Task<Tdto> GetAsync(Guid id, CancellationToken token);
        Task<Contract.Model.PagedResult<Tdto>> GetAsync(TFilter filter, CancellationToken token);
    }

    public interface IAddDataService<Tdto, TCreator> where Tdto : Contract.Model.Entity
    {
        Task<Tdto> AddAsync(TCreator entity, CancellationToken token);
    }

    public interface IUpdateDataService<Tdto, TUpdater> where Tdto : Contract.Model.Entity
    {
        Task<Tdto> UpdateAsync(TUpdater entity, CancellationToken token);
    }

    public interface IDeleteDataService<Tdto> where Tdto : Contract.Model.Entity
    {
        Task<Tdto> DeleteAsync(Guid id, CancellationToken token);
    }

    public static class DataServiceExtension
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddDataService<UserDataService, Db.Model.User, Contract.Model.User,
                Contract.Model.UserFilter, Contract.Model.UserCreator, Contract.Model.UserUpdater>();
            services.AddDataService<ClientDataService, Db.Model.Client, Contract.Model.Client,
                Contract.Model.ClientFilter, Contract.Model.ClientCreator, Contract.Model.ClientUpdater>();

            services.AddScoped<IGetDataService<Contract.Model.UserHistory, Contract.Model.UserHistoryFilter>, UserHistoryDataService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }

        private static IServiceCollection AddDataService<TService, TEntity, Tdto, TFilter, TCreator, TUpdater>(this IServiceCollection services)
            where TEntity : Db.Model.Entity
            where TUpdater : Contract.Model.IEntity
            where TService : DataService<TEntity, Tdto, TFilter, TCreator, TUpdater>
            where Tdto : Contract.Model.Entity
            where TFilter : Contract.Model.Filter<Tdto>
        {
            services.AddScoped<IGetDataService<Tdto, TFilter>, TService>();
            services.AddScoped<IAddDataService<Tdto, TCreator>, TService>();
            services.AddScoped<IUpdateDataService<Tdto, TUpdater>, TService>();
            services.AddScoped<IDeleteDataService<Tdto>, TService>();
            return services;
        }
    }

    [Serializable]
    internal class DataServiceException : Exception
    {
        public DataServiceException()
        {
        }

        public DataServiceException(string message) : base(message)
        {
        }

        public DataServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public abstract class DataGetService<TEntity, Tdto, TFilter> :
        IGetDataService<Tdto, TFilter>
        where TEntity : Db.Model.IEntity
        where Tdto : Contract.Model.Entity
        where TFilter : Contract.Model.Filter<Tdto>
    {
        protected IServiceProvider _serviceProvider;
        protected IMapper _mapper;

        protected abstract string DefaultSort { get; }

        /// <summary>
        /// function for modify client filter to db filter
        /// </summary>
        protected abstract Expression<Func<TEntity, bool>> GetFilter(TFilter filter);


        /// <summary>
        /// function for enrichment data
        /// </summary>
        protected abstract Func<Tdto, Tdto> EnrichFunc { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public DataGetService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _mapper = _serviceProvider.GetRequiredService<IMapper>();
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
                //Expression<Func<TEntity, bool>> expr = s => GetFilter(s, filter);
                //var func = expr.Compile(true);

                string sort = filter.Sort;
                if (string.IsNullOrEmpty(sort))
                {
                    sort = DefaultSort;
                }
                var result = await repo.GetAsync(new Db.Model.Filter<TEntity>
                {
                    Size = filter.Size,
                    Page = filter.Page,
                    Sort = sort,
                    Selector = GetFilter(filter)
                }, token);
                var prepare = result.Data.Select(s => _mapper.Map<Tdto>(s));
                if (EnrichFunc != null)
                {
                    prepare = prepare.Select(s => EnrichFunc(s));
                }
                return new Contract.Model.PagedResult<Tdto>(prepare, result.AllCount);
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
                var prepare = _mapper.Map<Tdto>(result);
                if (EnrichFunc != null)
                {
                    prepare = EnrichFunc(prepare);
                }
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
            catch (DataServiceException)
            {
                throw;
            }
            catch (Db.Interface.RepositoryException ex)
            {
                throw new DataServiceException(ex.Message);
            }
        }
    }

    public interface IAuthService
    {
        Task<ClaimsIdentity> Auth(Contract.Model.ClientIdentity login, CancellationToken token);
        Task<ClaimsIdentity> Auth(Contract.Model.UserIdentity login, CancellationToken token);
    }

    public class AuthService : IAuthService
    {
        private const string CLIENT_ROLE_TYPE = "Client";
        private const string TOKEN_AUTH_TYPE = "Token";
        private const string USER_ROLE_TYPE = "User";
        private const string COOKIES_AUTH_TYPE = "Cookies";

        private readonly IServiceProvider _serviceProvider;
        public AuthService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Client Auth
        /// </summary>
        /// <param name="login"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ClaimsIdentity> Auth(Contract.Model.ClientIdentity login, CancellationToken token)
        {
            return await AuthInternal<Db.Model.Client, Contract.Model.ClientIdentity>(login, CLIENT_ROLE_TYPE, TOKEN_AUTH_TYPE, token);
        }

        /// <summary>
        /// User Auth
        /// </summary>
        /// <param name="login"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ClaimsIdentity> Auth(Contract.Model.UserIdentity login, CancellationToken token)
        {
            return await AuthInternal<Db.Model.User, Contract.Model.UserIdentity>(login, USER_ROLE_TYPE, COOKIES_AUTH_TYPE, token);
        }

        private async Task<ClaimsIdentity> AuthInternal<T, I>(I login, string roleType, string authType, CancellationToken token)
            where T : Db.Model.Entity, Db.Model.IIdentity
            where I : Contract.Model.IIdentity
        {
            var repo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<T>>();
            var password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(login.Password));
            var client = (await repo.GetAsync(new Db.Model.Filter<T>()
            {
                Page = 0,
                Size = 10,
                Selector = s => s.Login == login.Login && s.Password == password
            }, token)).Data.FirstOrDefault();
            if (client != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, client.Id.ToString()),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, roleType)
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, authType,
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }
            // если пользователя/клиента не найдено
            return null;
        }
    }
}
