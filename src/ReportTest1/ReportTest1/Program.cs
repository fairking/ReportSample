using Stimulsoft.Report;
using System;
using System.IO;

namespace ReportTest1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Printing the report..");
            
            var reportMrt = File.ReadAllText("TestReport.mrt");

            using (var masterReport = new StiReport())
            {
                masterReport.LoadFromJson(reportMrt);
                masterReport.Render();
                masterReport.ExportDocument(StiExportFormat.Pdf, $"Report_{DateTime.Now.ToString("yyyyddMM_HHmmss")}.pdf");
            }

            Console.WriteLine("Report Generated.");
            Console.ReadKey();
        }
    }
}
