using System.Collections.Generic;
using System.Threading.Tasks;
using TheCrushinator.FitBit.Web.Models;

namespace TheCrushinator.FitBit.Web.Services.Interfaces
{
    public interface IBeurerService
    {
        public Task<IEnumerable<ScaleEntry>> ReadScaleDataInToDatabase();
    }
}
