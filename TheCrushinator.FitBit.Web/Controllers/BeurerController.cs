using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TheCrushinator.FitBit.Web.Services.Interfaces;

namespace TheCrushinator.FitBit.Web.Controllers
{
    public class BeurerController : Controller
    {
        private readonly ILogger<BeurerController> _logger;
        private readonly IBeurerService _beurerService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="beurerService"></param>
        public BeurerController(ILogger<BeurerController> logger, IBeurerService beurerService)
        {
            _logger = logger;
            _beurerService = beurerService;
        }

        public async Task<IActionResult> FetchData(CancellationToken cancellationToken)
        {
            var newEntries = await _beurerService.FetchScaleDataFromBeurerInToDatabase(cancellationToken);

            ViewBag.Entries = newEntries;

            return View();
        }

        public async Task<IActionResult> ReadData()
        {
            var newEntries = await _beurerService.ReadScaleDataFromFileInToDatabase();

            ViewBag.Entries = newEntries;

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> FetchNewDataFromBeurer()
        {
            return View("ReadData");
        }
    }
}
