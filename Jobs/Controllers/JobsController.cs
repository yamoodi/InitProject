using Jobs.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jobs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Consts.Roles.SuperVisor)]
    public class JobsController : ControllerBase
    {
        /// <summary>
        /// Gets the price for a ticker symbol
        /// </summary>
        /// <param name="tickerSymbol"></param>
        /// <returns>A SharePriceResponse which contains the price of the share</returns>
        /// <response code="200">Returns 200 and the share price</response>
        /// <response code="400">Returns 400 if the query is invalid</response>
        [HttpGet]
        public IActionResult GetDate()
        {
            return Ok("hellow");
        }
    }
}
