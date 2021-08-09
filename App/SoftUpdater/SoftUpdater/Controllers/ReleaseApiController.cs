using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUpdater.Contract.Model;
using SoftUpdater.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Controllers
{
    [Route("api/v1/release")]
    [ApiController]
    public class ReleaseApiController : CommonControllerBase
    {
        private IServiceProvider _serviceProvider;        

        public ReleaseApiController(IServiceProvider serviceProvider): base(serviceProvider)
        {
            _serviceProvider = serviceProvider;            
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetReleases([FromQuery]string version)
        {
            try
            {
                var clientId = Guid.Parse(User.Identity.Name);
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Release, ReleaseFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var allSelected = false;
                List<Release> result = new List<Release>();
                int page = 0;
                while (!allSelected)
                {
                    var selected = await _dataService.GetAsync(new ReleaseFilter(new List<Guid> { clientId }, 100, page++, null, null), source.Token);
                    result.AddRange(selected.Data);
                    allSelected = selected.PageCount == page;
                }
                if (!string.IsNullOrEmpty(version) && result.Any(s => s.Version == version))
                {
                    var last = result.FirstOrDefault(s => s.Version == version);
                    result = result.Where(s => s.Number > last.Number).ToList();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении списка релизов: {ex.Message} {ex.StackTrace}");
                return InternalServerError($"Ошибка при получении списка релизов: {ex.Message}");                
            }
        }
    }

}
