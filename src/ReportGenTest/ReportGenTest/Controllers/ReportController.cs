using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using System;
using System.Linq;
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

        [HttpGet]
        public async Task Get([FromQuery] string name)
        {
            // Export data from external 
            var reportMrt = await System.IO.File.ReadAllTextAsync($"{name}.mrt");

            using (var stiReport = new StiReport())
            {
                stiReport.LoadFromJson(reportMrt);

                //foreach (var db in stiReport.Dictionary.Databases.OfType<StiODataDatabase>())
                //{
                //    db.ConnectionString += ";Authorization=" + Guid.NewGuid().ToString();
                //}

                //foreach (var db in stiReport.Dictionary.Databases.OfType<StiJsonDatabase>())
                //{
                //    db.PathData += ";Authorization=" + Guid.NewGuid().ToString();
                //}

                await stiReport.RenderAsync();
                await stiReport.ExportDocumentAsync(StiExportFormat.Pdf, $"Report_{DateTime.Now.ToString("yyyyddMM_HHmmss")}.pdf");
            }
        }

    }
}
