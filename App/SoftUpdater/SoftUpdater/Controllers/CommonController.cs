//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUpdater.Deploy;
using System;
using System.Threading.Tasks;

namespace SoftUpdater.Controllers
{
    /// <summary>
    /// Common api calls (without models)
    /// </summary>
    [ApiController]
    [Route("api/v1/common")]
    [Produces("application/json")]
    public class CommonController : CommonControllerBase
    {
        private readonly IDeployService deployService;

        public CommonController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<CommonController>>();
            deployService = serviceProvider.GetRequiredService<IDeployService>();
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpGet("deploy")]
        public async Task<IActionResult> Deploy()
        {
            try
            {
                await deployService.Deploy();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при раскатке базы данных: {ex.Message} {ex.StackTrace}");
                return InternalServerError($"Ошибка при раскатке базы данных: {ex.Message}");
            }
        }
    }

    public abstract class CommonControllerBase : Controller
    {
        protected ILogger _logger;

        public CommonControllerBase(IServiceProvider serviceProvider)
        {

        }

        protected InternalServerErrorObjectResult InternalServerError()
        {
            return new InternalServerErrorObjectResult();
        }

        protected InternalServerErrorObjectResult InternalServerError(object value)
        {
            return new InternalServerErrorObjectResult(value);
        }
    }

    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object value) : base(value)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }

        public InternalServerErrorObjectResult() : this(null)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

}
