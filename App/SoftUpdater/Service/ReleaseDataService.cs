using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoftUpdater.Common;
using SoftUpdater.Db.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Service
{
    public class ReleaseDataService : DataService<Db.Model.Release, Contract.Model.Release,
        Contract.Model.ReleaseFilter, Contract.Model.ReleaseCreator, Contract.Model.ReleaseUpdater>
    {
        public ReleaseDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected override async Task<Contract.Model.Release> Enrich(Contract.Model.Release entity, CancellationToken token)
        {
            var clientRepo = _serviceProvider.GetRequiredService<IRepository<Db.Model.Client>>();
            var client = await clientRepo.GetAsync(entity.ClientId, token);
            entity.Client = client.Name;
            return entity;
        }

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected override async Task<IEnumerable<Contract.Model.Release>> Enrich(IEnumerable<Contract.Model.Release> entities, CancellationToken token)
        {
            var clientRepo = _serviceProvider.GetRequiredService<IRepository<Db.Model.Client>>();
            var clientIds = entities.Select(s => s.ClientId).Distinct().ToList();
            var clients = await clientRepo.GetAsync(new Db.Model.Filter<Db.Model.Client>()
            { 
               Selector = s => clientIds.Contains(s.Id)
            }, token);
            List<Contract.Model.Release> result = new List<Contract.Model.Release>();
            foreach (var entity in entities)
            {
                var client = clients.Data.FirstOrDefault(s => s.Id == entity.ClientId);
                entity.Client = client.Name;
                result.Add(entity);
            }

            return result;
        }

        protected override Expression<Func<Db.Model.Release, bool>> GetFilter(Contract.Model.ReleaseFilter filter)
        {
            return s => filter.Clients.Contains(s.ClientId);            
        }

        protected override Db.Model.Release UpdateFillFields(Contract.Model.ReleaseUpdater entity, Db.Model.Release entry)
        {
            entry.ClientId = entity.ClientId;            
            entry.Path = entity.Path;
            entry.Version = entity.Version;
            return entry;
        }
       
        protected override string DefaultSort => "Version";

        protected override async Task<Db.Model.Release> MapToEntityAdd(Contract.Model.ReleaseCreator creator, CancellationToken token)
        {
            var result = await base.MapToEntityAdd(creator, token);
            var _repo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.Release>>();
            var lastRelease = await _repo.GetAsync(new Db.Model.Filter<Db.Model.Release>() { 
               Page = 0, Selector = s=>s.ClientId == creator.ClientId, Size = 1, Sort = "Number desc"
            }, CancellationToken.None);
            result.Number = lastRelease.Data.FirstOrDefault()?.Number + 1 ?? 1;
            return result;
        }
    }

    public class ReleaseArchitectDataService : DataService<Db.Model.ReleaseArchitect, Contract.Model.ReleaseArchitect,
        Contract.Model.ReleaseArchitectFilter, Contract.Model.ReleaseArchitectCreator, Contract.Model.ReleaseArchitectUpdater>
    {
        private readonly IOptions<CommonOptions> options;
        public ReleaseArchitectDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            options = _serviceProvider.GetRequiredService<IOptions<CommonOptions>>();
        }

        protected override Expression<Func<Db.Model.ReleaseArchitect, bool>> GetFilter(Contract.Model.ReleaseArchitectFilter filter)
        {
            return s => s.ReleaseId == filter.ReleaseId && (string.IsNullOrEmpty(filter.Name) || s.Name == filter.Name)
            && (string.IsNullOrEmpty(filter.Path) || s.Path == filter.Path);
        }

        protected override async Task<Db.Model.ReleaseArchitect> MapToEntityAdd(Contract.Model.ReleaseArchitectCreator creator, CancellationToken token)
        {
            var result = await base.MapToEntityAdd(creator, token);
            result.FileName = creator.File.FileName;

            var _releaseRepo = _serviceProvider.GetRequiredService<IRepository<Db.Model.Release>>();
            var release = await _releaseRepo.GetAsync(creator.ReleaseId, token);
            var _clientRepo = _serviceProvider.GetRequiredService<IRepository<Db.Model.Client>>();
            var client = await _clientRepo.GetAsync(release.ClientId, token);
            string path = Path.Combine(options.Value.UploadBasePath, client.BasePath, release.Path, creator.Path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filePath = Path.Combine(path, creator.File.FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await creator.File.CopyToAsync(fileStream);
            }
            return result;
        }

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected override async Task<Contract.Model.ReleaseArchitect> Enrich(Contract.Model.ReleaseArchitect entity, CancellationToken token)
        {
            var releaseRepo = _serviceProvider.GetRequiredService<IRepository<Db.Model.Release>>();
            var release = await releaseRepo.GetAsync(entity.ReleaseId, token);
            entity.Release = release.Version;
            return entity;
        }

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected override async Task<IEnumerable<Contract.Model.ReleaseArchitect>> Enrich(IEnumerable<Contract.Model.ReleaseArchitect> entities, CancellationToken token)
        {
            var releaseRepo = _serviceProvider.GetRequiredService<IRepository<Db.Model.Release>>();
            var releaseIds = entities.Select(s => s.ReleaseId).Distinct().ToList();
            var releases = await releaseRepo.GetAsync(new Db.Model.Filter<Db.Model.Release>()
            {
                Selector = s => releaseIds.Contains(s.Id)
            }, token);
            List<Contract.Model.ReleaseArchitect> result = new List<Contract.Model.ReleaseArchitect>();
            foreach (var entity in entities)
            {
                var release = releases.Data.FirstOrDefault(s => s.Id == entity.ReleaseId);
                entity.Release = release.Version;
                result.Add(entity);
            }

            return result;
        }

        protected override Db.Model.ReleaseArchitect UpdateFillFields(Contract.Model.ReleaseArchitectUpdater entity, Db.Model.ReleaseArchitect entry)
        {
            
            return entry;
        }

        protected override string DefaultSort => "Name";                
    }

    public class LoadHistoryDataService : DataService<Db.Model.LoadHistory, Contract.Model.LoadHistory,
        Contract.Model.LoadHistoryFilter, Contract.Model.LoadHistoryCreator, Contract.Model.LoadHistoryUpdater>
    {
        public LoadHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.LoadHistory, bool>> GetFilter(Contract.Model.LoadHistoryFilter filter)
        {
            return s => true;
        }

        protected override Db.Model.LoadHistory UpdateFillFields(Contract.Model.LoadHistoryUpdater entity, Db.Model.LoadHistory entry)
        {
            throw new NotImplementedException();
        }

        protected override string DefaultSort => "Version";

        protected override async Task<Db.Model.LoadHistory> MapToEntityAdd(Contract.Model.LoadHistoryCreator creator, CancellationToken token)
        {
            var result = await base.MapToEntityAdd(creator, token);            
            return result;
        }
    }
}
