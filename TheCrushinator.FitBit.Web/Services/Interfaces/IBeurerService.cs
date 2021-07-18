using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheCrushinator.FitBit.Web.Models;

namespace TheCrushinator.FitBit.Web.Services.Interfaces
{
    public interface IBeurerService
    {
        Task<ScaleEntry> GetNextScaleEntry(bool isUnsynchronised = true);
        Task<IEnumerable<ScaleEntry>> ReadScaleDataFromFileInToDatabase();
        Task<IEnumerable<ScaleEntry>> FetchScaleDataFromBeurerInToDatabase(CancellationToken cancellationToken);
    }
}
