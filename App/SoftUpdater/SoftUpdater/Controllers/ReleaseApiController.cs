//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUpdater.Common;
using SoftUpdater.Contract.Model;
using SoftUpdater.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Controllers
{
    /// <summary>
    /// API Controller for get releases
    /// </summary>
    [Route("api/v1/release")]
    [ApiController]
    public class ReleaseApiController : CommonControllerBase
    {
        private IServiceProvider _serviceProvider;
        private readonly IErrorNotifyService _errorNotifyService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ReleaseApiController(IServiceProvider serviceProvider): base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _errorNotifyService = serviceProvider.GetRequiredService<IErrorNotifyService>();
        }

        /// <summary>
        /// Get Releases method
        /// </summary>
        /// <param name="version">last downloaded version</param>
        /// <returns>List<Release></returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetReleases([FromQuery]string version)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Release, ReleaseFilter>>();

                var clientId = Guid.Parse(User.Identity.Name);                
                CancellationTokenSource source = new CancellationTokenSource(30000);

                var result = (await _dataService.GetAsync(new ReleaseFilter(new List<Guid> { clientId }), source.Token)).Data;
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
                await _errorNotifyService.Send($"Ошибка в методе ReleaseApiController::GetReleases: {ex.Message} {ex.StackTrace}");
                return InternalServerError($"Ошибка при получении списка релизов: {ex.Message}");                
            }
        }
    }

}
