using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
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

        protected override async Task<Db.Model.Client> MapToEntityAdd(Contract.Model.ClientCreator creator)
        {
            var entity = await base.MapToEntityAdd(creator);
            entity.Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(creator.Password));

            if (!Directory.Exists(creator.BasePath))
            {
                Directory.CreateDirectory(creator.BasePath);
            }
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

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected override async Task<Contract.Model.Client> Enrich(Contract.Model.Client entity, CancellationToken token)
        {
            var userRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository< Db.Model.User>> ();
            var user = await userRepo.GetAsync(entity.UserId, token);
            if (user != null) entity.UserName = user.Name;
            return entity;
        }

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected override async Task<IEnumerable<Contract.Model.Client>> Enrich(IEnumerable<Contract.Model.Client> entities, CancellationToken token)
        {
            List<Contract.Model.Client> result = new List<Contract.Model.Client>();
            var userRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.User>>();
            foreach (var entity in entities)
            {                
                var user = await userRepo.GetAsync(entity.UserId, token);
                if (user != null) entity.UserName = user.Name;
                result.Add(entity);
            }
            return result;
        }

        protected override string DefaultSort => "Name";

    }
}
