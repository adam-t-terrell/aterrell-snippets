using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EPIC_Finder.Models;
using System.Collections.Generic;
using EPIC_Finder.JSON;
using System.Linq;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace AnApp.Controllers
{
    public class SomeController : Controller
    {
        private readonly ILogger<SomeController> _logger;

        public SomeController(ILogger<SomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var vm = new SearchModel();
            vm.searchType = "pages";
            return View(vm);
        }

        public async Task<IActionResult> SearchResultsAsync(SearchModel model)
        {
            var jsonService = new JSONService();
            var results = await jsonService.GetResults(model.query, model.searchType, model.limit);
            return View(results);
        }

        [HttpPost]
        public IActionResult OutputCSV(CSVViewModel model)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(model.items);
                }
                return File(stream.ToArray(), "text/plain", "EPIC_results.csv");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
