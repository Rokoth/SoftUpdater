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
    public class UserController : Controller
    {
        private IServiceProvider _serviceProvider;

        public UserController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // GET: UserController
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> ListPaged(int page = 0, int size = 10, string sort = null, string name = null)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<User, UserFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new UserFilter(size, page, sort, name), source.Token);
                Response.Headers.Add("x-pages", result.PageCount.ToString());
                return PartialView(result.Data);
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
        public async Task<ActionResult> HistoryListPaged(int page = 0, int size = 10, string sort = null, string name = null, Guid? id = null)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<UserHistory, UserHistoryFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new UserHistoryFilter(size, page, sort, name, id), source.Token);
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
        public async Task<ActionResult> Details(Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<User, UserFilter>>();
                var cancellationTokenSource = new CancellationTokenSource(30000);
                User result = await _dataService.GetAsync(id, cancellationTokenSource.Token);
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
            var user = new UserCreator()
            {

            };
            return View(user);
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Create(UserCreator creator)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IAddDataService<User, UserCreator>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                User result = await _dataService.AddAsync(creator, source.Token);
                return RedirectToAction(nameof(Details), new { id = result.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<User, UserFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                User result = await _dataService.GetAsync(id, source.Token);
                var updater = new UserUpdater()
                {
                    Description = result.Description,
                    Id = result.Id,
                    Login = result.Login,
                    Name = result.Name,
                    PasswordChanged = false
                };
                return View(updater);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit(Guid id, UserUpdater updater)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IUpdateDataService<User, UserUpdater>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                User result = await _dataService.UpdateAsync(updater, source.Token);
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
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<User, UserFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                User result = await _dataService.GetAsync(id, source.Token);
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
        public async Task<ActionResult> Delete(Guid id, User model)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IDeleteDataService<User>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                User result = await _dataService.DeleteAsync(id, source.Token);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
    }
}
