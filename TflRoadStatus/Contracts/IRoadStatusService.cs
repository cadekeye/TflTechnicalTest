using System.Threading.Tasks;
using TflRoadStatus.Models;

namespace TflRoadStatus.Contracts
{
	public interface IRoadStatusService
	{
		Task<RoadStatusApiModel> GetRoadStatusDetail(string path);
	}
}