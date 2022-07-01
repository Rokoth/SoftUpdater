//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SoftUpdater.Common;
using SoftUpdater.Contract.Model;
using SoftUpdater.Service;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Controllers
{

    public class BaseController : Controller
    {
        protected IServiceProvider _serviceProvider;
        protected readonly IErrorNotifyService _errorNotifyService;
        protected string _controllerName;

        public BaseController(IServiceProvider serviceProvider, string controllerName)
        {
            _serviceProvider = serviceProvider;
            _errorNotifyService = _serviceProvider.GetRequiredService<IErrorNotifyService>();
            _controllerName = controllerName;
        }

        protected async Task<ActionResult> BaseExec(Func<Task<ActionResult>> action, string methodName)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе {_controllerName}::{methodName}: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
    }
    public class ReleaseArchitectController : BaseController
    {               
        public ReleaseArchitectController(IServiceProvider serviceProvider): base(serviceProvider, "ReleaseArchitectController") { }

        // GET: UserController
        [Authorize]
        public ActionResult Index(Guid releaseId)
        {
            ViewData["ReleaseId"] = releaseId.ToString();
            return View();
        }

        [Authorize]
        public async Task<ActionResult> ListPaged(Guid releaseId, int page = 0, int size = 10, string sort = null, string name = null, string path = null)
        {
            return await BaseExec(async () =>
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new ReleaseArchitectFilter(releaseId, size, page, sort, name, path), source.Token);
                Response.Headers.Add("x-pages", result.PageCount.ToString());
                return PartialView(result.Data);
            }, "ListPaged");            
        }
                
        [Authorize]
        public async Task<ActionResult> Details([FromRoute] Guid id)
        {
            return await BaseExec(async () =>
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                var cancellationTokenSource = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(id, cancellationTokenSource.Token);
                return View(result);
            }, "Details");
        }
               
        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            return await BaseExec(async () =>
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
            }, "Edit");
        }
              
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit(Guid id, ReleaseArchitectUpdater updater)
        {
            return await BaseExec(async () =>
            {
                var _dataService = _serviceProvider.GetRequiredService<IUpdateDataService<ReleaseArchitect, ReleaseArchitectUpdater>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.UpdateAsync(updater, source.Token);
                return RedirectToAction("Details", new { id = result.Id });
            }, "Edit");
        }

        [Authorize]
        public async Task<IActionResult> Create([FromQuery] Guid releaseId)
        {
            return await BaseExec(async () =>
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
            }, "Create");
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ReleaseArchitectCreator creator)
        {
            return await BaseExec(async () =>
            {
                var _dataService = _serviceProvider.GetRequiredService<IAddDataService<ReleaseArchitect, ReleaseArchitectCreator>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);               
                ReleaseArchitect result = await _dataService.AddAsync(creator, source.Token);
                return RedirectToAction(nameof(Details), new { id = result.Id });
            }, "Create");
        }

        [HttpGet]
        public async Task<JsonResult> CheckName(string name, Guid releaseId)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(name))
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                var cancellationTokenSource = new CancellationTokenSource(30000);
                var check = await _dataService.GetAsync(new ReleaseArchitectFilter(releaseId, null, null, null, name, null), cancellationTokenSource.Token);
                result = !check.Data.Any();
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<JsonResult> CheckPath(string path, Guid releaseId)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(path))
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                var cancellationTokenSource = new CancellationTokenSource(30000);
                var check = await _dataService.GetAsync(new ReleaseArchitectFilter(releaseId, null, null, null, null, path), cancellationTokenSource.Token);
                result = !check.Data.Any();
            }
            return Json(result);
        }

        // GET: UserController/Delete/5
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            return await BaseExec(async () =>
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseArchitect, ReleaseArchitectFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                ReleaseArchitect result = await _dataService.GetAsync(id, source.Token);
                return View(result);
            }, "Delete");
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id, ReleaseArchitect model)
        {
            return await BaseExec(async () =>
            {
                var _dataService = _serviceProvider.GetRequiredService<IDeleteDataService<ReleaseArchitect>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                ReleaseArchitect result = await _dataService.DeleteAsync(id, source.Token);
                return RedirectToAction(nameof(Index));
            }, "Delete");
        }
    }
}
