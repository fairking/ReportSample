using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ReportGenTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Use https://localhost:5001/weatherforecast or https://localhost:5001/odata/weatherforecast
        /// </summary>
        [HttpGet]
        [EnableCors("all-cors")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            // Send Headers to the console
            foreach (var h in this.Request.Headers)
                Console.WriteLine($"{h.Key}:{h.Value}");

            // Validate Authorization
            // It must throw an exception during the report rendering.
            if (!this.Request.Headers.TryGetValue(HttpRequestHeader.Authorization.ToString(), out var auth) || auth.FirstOrDefault() != "Bearer ABC")
            {
                //HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                //return new WeatherForecast[0];
                Console.WriteLine("Authorization Failed!!!");
            }
            else
            {
                Console.WriteLine("Authorization Passed!!!");
            }

            return WeatherForecastDataGenerator.GetData();
        }
    }
}
