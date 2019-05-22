using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DNTScheduler.TestApi.Controllers
{
    [Route("api/test")]
    [Produces("application/json")]
    [ApiController]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Get a list of values
        /// </summary>
        /// <returns>An ActionResult of type IEnumerable of string</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetValues()
        {
            return Ok(new[] { "Val 1", "Val 2" });
        }
    }
}