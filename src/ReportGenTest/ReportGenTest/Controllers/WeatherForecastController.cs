using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportGenTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [EnableCors("all-cors")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            // Send Headers to the console
            foreach (var h in this.Request.Headers)
                Console.WriteLine($"{h.Key}:{h.Value}");

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        //[HttpGet]
        //public async Task Report([FromQuery]string name)
        //{
        //    // Export data from external 
        //    var reportMrt = System.IO.File.ReadAllText($"{name}.mrt");

        //    using (var stiReport = new StiReport())
        //    {
        //        stiReport.LoadFromJson(reportMrt);
        //        stiReport.Render();
        //        stiReport.ExportDocument(StiExportFormat.Pdf, $"Report_{DateTime.Now.ToString("yyyyddMM_HHmmss")}.pdf");
        //    }
        //}

    }
}
