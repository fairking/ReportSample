using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ReportGenTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;

        public ReportController(ILogger<ReportController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Export data from external sources (odata or json) with appended authentication Bearer token
        /// Please use the following links to test the functionality
        /// https://localhost:5001/report?name=TestReport2 (Json)
        /// https://localhost:5001/report?name=TestReport3 (Odata)
        /// You can try to change "Bearer ABC" to something else. It must throw an authorization exception.
        /// </summary>
        [HttpGet]
        public async Task Get([FromQuery] string name)
        {
            // Load json report from file
            var reportMrt = await System.IO.File.ReadAllTextAsync($"{name}.mrt");

            // Create a new report
            using (var stiReport = new StiReport())
            {
                // Load report
                stiReport.LoadFromJson(reportMrt);

                // Set Authorization token to the Odata databases by using a workaround proposed by stimulsoft team
                foreach (var db in stiReport.Dictionary.Databases.OfType<StiODataDatabase>())
                {
                    db.ConnectionString += ";Token=" + "ABC";
                }

                // Set Authorization token to the Json databases by using a custom implementation of json database
                for (int i = 0; i < stiReport.Dictionary.Databases.Count; i++)
                {
                    var jsonDb = stiReport.Dictionary.Databases[i] as StiJsonDatabase;
                    if (jsonDb != null)
                    {
                        var customJsonDb = new CustomStiJsonDatabase(jsonDb.Name, jsonDb.PathData, jsonDb.Key)
                        {
                            ThrowConnectionException = true,
                        };
                        customJsonDb.CustomHeaders.Add(HttpRequestHeader.Authorization.ToString(), "Bearer ABC");
                        stiReport.Dictionary.Databases[i] = customJsonDb;
                    }
                }

                // Render the report (it will hit the WeatherForecast controller
                await stiReport.RenderAsync();

                // Save the rendered data to pdf file
                await stiReport.ExportDocumentAsync(StiExportFormat.Pdf, $"Report_{DateTime.Now.ToString("yyyyddMM_HHmmss")}.pdf");
            }
        }

    }
}
