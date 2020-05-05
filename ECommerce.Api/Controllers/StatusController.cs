using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace ECommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        /// <summary>
        /// Just to check if the API server is up.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Index()
        {
            return "API server is up and running";
        }
    }
}
