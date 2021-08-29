//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUpdater.Common;
using SoftUpdater.Contract.Model;
using SoftUpdater.Service;
using System;
using System.Collections.Generic;
using System.IO;
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
        [Authorize("Token")]
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

                var ret = result.Select(s => new ReleaseClient()
                { 
                   Version = s.Version,
                   Id = s.Id,
                   Architects = new List<ReleaseArchitectClient>()
                }).ToList();
                var archDataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                foreach (var item in ret)
                {
                    var architects = await archDataService.GetAsync(new ReleaseArchitectFilter(item.Id, null, null, null, null, null), source.Token);
                    foreach (var arch in architects.Data)
                    {
                        item.Architects.Add(new ReleaseArchitectClient() { 
                           Id = arch.Id,
                           Name = arch.Name
                        });
                    }
                }
                return Ok(ret);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении списка релизов: {ex.Message} {ex.StackTrace}");
                await _errorNotifyService.Send($"Ошибка в методе ReleaseApiController::GetReleases: {ex.Message} {ex.StackTrace}");
                return InternalServerError($"Ошибка при получении списка релизов: {ex.Message}");                
            }
        }

        /// <summary>
        /// Get Releases method
        /// </summary>
        /// <param name="version">last downloaded version</param>
        /// <returns>List<Release></returns>
        [Authorize("Token")]
        [HttpGet("download")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> DownloadRelease([FromQuery] Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Release, ReleaseFilter>>();
                var archDataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                var options = _serviceProvider.GetRequiredService<IOptions<CommonOptions>>();

                var clientId = Guid.Parse(User.Identity.Name);
                CancellationTokenSource source = new CancellationTokenSource(30000);

                var arch = await archDataService.GetAsync(id, source.Token);
                var release = await _dataService.GetAsync(arch.ReleaseId, source.Token);
                if (release.ClientId != clientId)
                    return NotFound("Релиз не найден");

                var _clientRepo = _serviceProvider.GetRequiredService<IGetDataService<Client, ClientFilter>>();
                var client = await _clientRepo.GetAsync(release.ClientId, source.Token);
                string path = Path.Combine(options.Value.UploadBasePath, client.BasePath, release.Path, arch.Path, arch.FileName);
                if (System.IO.File.Exists(path))
                {
                    var content = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(path));                   
                    return File(content, "application/octet-stream", arch.FileName);                                     
                }
                return NotFound("Файл на сервере не найден");
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
