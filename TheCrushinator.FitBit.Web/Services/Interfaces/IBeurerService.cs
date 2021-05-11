using System.Threading.Tasks;

namespace TheCrushinator.FitBit.Web.Services.Interfaces
{
    public interface IBeurerService
    {
        public Task ImportWeightFromJson();
    }
}
