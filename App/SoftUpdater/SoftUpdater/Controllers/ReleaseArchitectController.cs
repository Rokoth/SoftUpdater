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
    public class ReleaseArchitectController : Controller
    {
        private IServiceProvider _serviceProvider;

        public ReleaseArchitectController(IServiceProvider serviceProvider)
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
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
    }
}
