//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Mvc;

namespace SoftUpdater.Controllers
{
    /// <summary>
    /// Common api calls (without models)
    /// </summary>
    [ApiController]
    [Route("api/v1/common")]
    [Produces("application/json")]
    public class CommonController : Controller
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}
