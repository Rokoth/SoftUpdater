using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class LoadHistoryController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public LoadHistoryController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // GET: ClientController
        /// <summary>
        /// List method
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// paged partial view
        /// </summary>
        /// <param name="page">page number from 0</param>
        /// <param name="size">page size</param>
        /// <param name="sort">sorting</param>
        /// <param name="name">name filter</param>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> ListPaged(int page = 0, int size = 10, string sort = null)
        {
            try
            {
                var userId = User.Identity.Name;
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<LoadHistory, LoadHistoryFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new LoadHistoryFilter(size, page, sort), source.Token);
                Response.Headers.Add("x-pages", result.PageCount.ToString());
                return PartialView(result.Data);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { ex.Message });
            }
        }
    }
}
