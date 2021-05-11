using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public async Task<IActionResult> ReadData()
        {
            await _beurerService.ImportWeightFromJson();

            return View();
        }
    }
}
