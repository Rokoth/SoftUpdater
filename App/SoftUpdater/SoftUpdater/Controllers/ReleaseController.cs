using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SoftUpdater.Contract.Model;
using SoftUpdater.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Controllers
{
    public class ReleaseController : Controller
    {
        private IServiceProvider _serviceProvider;

        public ReleaseController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // GET: UserController
        [Authorize]
        public ActionResult Index([FromRoute] Guid clientId)
        {
            ViewData["ClientId"] = clientId.ToString();
            return View();
        }

        [Authorize]
        public async Task<ActionResult> ListPaged([FromRoute]Guid clientId,  
            [FromQuery]int page = 0, [FromQuery]int size = 10,
            [FromQuery]string sort = null, [FromQuery]string name = null)
        {
            try
            {                
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Release, ReleaseFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new ReleaseFilter(clientId, size, page, sort, name), source.Token);
                Response.Headers.Add("x-pages", result.PageCount.ToString());
                return PartialView(result.Data);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: ClientController/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Release, ReleaseFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                Release result = await _dataService.GetAsync(id, source.Token);
                var updater = new ReleaseUpdater()
                {                    
                    Id = result.Id,
                    Number = result.Number,
                    Path = result.Path,
                    Version = result.Version
                };
                return View(updater);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // POST: ClientController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit(Guid id, ReleaseUpdater updater)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IUpdateDataService<Release, ReleaseUpdater>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                Release result = await _dataService.UpdateAsync(updater, source.Token);
                return RedirectToAction("Details", new { id = result.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController
        [Authorize]
        public ActionResult History()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> HistoryListPaged([FromRoute] Guid clientId, [FromQuery] int page = 0, [FromQuery] int size = 10,
            [FromQuery] string sort = null, [FromQuery] string name = null, Guid? id = null)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ReleaseHistory, ReleaseHistoryFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new ReleaseHistoryFilter(clientId, size, page, sort, name, id), source.Token);
                Response.Headers.Add("x-pages", result.PageCount.ToString());
                return PartialView(result.Data);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController/Details/5
        [Authorize]
        public async Task<ActionResult> Details([FromRoute] Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Release, ReleaseFilter>>();
                var cancellationTokenSource = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(id, cancellationTokenSource.Token);
                return View(result);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController/Create
        [Authorize]
        public ActionResult Create()
        {
            //Fill default fields
            var user = new ReleaseCreator();
            return View(user);
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Create(ReleaseCreator creator)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IAddDataService<Release, ReleaseCreator>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);                
                Release result = await _dataService.AddAsync(creator, source.Token);
                return RedirectToAction(nameof(Details), new { id = result.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController/Delete/5
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Release, ReleaseFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                Release result = await _dataService.GetAsync(id, source.Token);
                return View(result);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id, Release model)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IDeleteDataService<Release>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                Release result = await _dataService.DeleteAsync(id, source.Token);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
    }
}
