using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FastReport;
using FastReport.Data.JsonConnection;
using FastReport.Export.Image;
using FastReport.Export.PdfSimple;
using FastReport.Utils;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;

namespace FastReportDotNET5.Controllers
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

        private readonly IWebHostEnvironment _webHostEnvironment;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("report")]
        public IActionResult GetReport()
        {
            var reportName = "report.frx";
            var contentType = "application/pdf";
            var fileDownloadName = "report.pdf";

            var webRootPath = _webHostEnvironment.WebRootPath;
            var reportPath = (webRootPath + "/reports/" + reportName);

            Config.WebMode = true;

            var jsonString = JsonConvert.SerializeObject(Summaries);

            var generator = new JSchemaGenerator();
            var myJsonSchema = generator.Generate(Summaries.GetType());

            var jsonBuilder = new JsonDataSourceConnectionStringBuilder 
            {
                JsonSchema = myJsonSchema.ToString(),
                Json = jsonString
            };

            var report = new Report();

            report.Load(reportPath);

            report.Dictionary.Connections[0].ConnectionString = jsonBuilder.ConnectionString;

            report.Prepare();

            var pdfExport = new PDFSimpleExport
            {
                ShowProgress = false
            };

            var stream = new MemoryStream();
            report.Export(pdfExport, stream);
            stream.Position = 0;

            return File(stream, contentType, fileDownloadName);
        }
    }
}
