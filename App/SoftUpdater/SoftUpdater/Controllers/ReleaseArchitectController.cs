using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
    public class ReleaseArchitectController : Controller
    {
        private IServiceProvider _serviceProvider;
        private readonly IErrorNotifyService _errorNotifyService;
        

        public ReleaseArchitectController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _errorNotifyService = _serviceProvider.GetRequiredService<IErrorNotifyService>();
            
        }

        // GET: UserController
        [Authorize]
        public ActionResult Index(Guid releaseId)
        {
            ViewData["ReleaseId"] = releaseId.ToString();
            return View();
        }

        [Authorize]
        public async Task<ActionResult> ListPaged(Guid releaseId, int page = 0, int size = 10, string sort = null, string name = null)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new ReleaseArchitectFilter(releaseId, size, page, sort, name), source.Token);
                Response.Headers.Add("x-pages", result.PageCount.ToString());
                return PartialView(result.Data);
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе ReleaseArchitectController::ListPaged: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
                
        [Authorize]
        public async Task<ActionResult> Details([FromRoute] Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                var cancellationTokenSource = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(id, cancellationTokenSource.Token);
                return View(result);
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе ReleaseArchitectController::Details: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
               
        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(id, source.Token);
                var updater = new ReleaseArchitectUpdater()
                {
                    Id = result.Id,
                    Name = result.Name,
                    Path = result.Path,
                    ReleaseId = result.ReleaseId,
                    Release = result.Release
                };
                return View(updater);
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе ReleaseArchitectController::Edit: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
              
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit(Guid id, ReleaseArchitectUpdater updater)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IUpdateDataService<ReleaseArchitect, ReleaseArchitectUpdater>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.UpdateAsync(updater, source.Token);
                return RedirectToAction("Details", new { id = result.Id });
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе ReleaseArchitectController::Edit: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> Create([FromQuery] Guid releaseId)
        {
            var userId = User.Identity.Name;
            var _releaseDataService = _serviceProvider.GetRequiredService<IGetDataService<Release, ReleaseFilter>>();
            var cancellationTokenSource = new CancellationTokenSource(30000);
            var release = await _releaseDataService.GetAsync(releaseId, cancellationTokenSource.Token);
            if (release == null)
            {
                return RedirectToAction("Index", "Error", new { Message = "Неверный клиент" });
            }            

            var creator = new ReleaseArchitectCreator()
            {
               Release = release.Version,
               ReleaseId = releaseId
            };
            return View(creator);
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ReleaseArchitectCreator creator)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IAddDataService<ReleaseArchitect, ReleaseArchitectCreator>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);               
                ReleaseArchitect result = await _dataService.AddAsync(creator, source.Token);
                return RedirectToAction(nameof(Details), new { id = result.Id });
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе ReleaseArchitectController::Create: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController/Delete/5
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                ReleaseArchitect result = await _dataService.GetAsync(id, source.Token);
                return View(result);
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе ReleaseArchitectController::Delete: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id, ReleaseArchitect model)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IDeleteDataService<ReleaseArchitect>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                ReleaseArchitect result = await _dataService.DeleteAsync(id, source.Token);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе ReleaseArchitectController::Delete: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
    }
}
