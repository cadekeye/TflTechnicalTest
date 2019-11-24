
namespace TflRoadStatus.Models
{
	public class RoadStatusApiModel
	{
		public RoadStatusDetailApiModel[] RoadStatus { get; set; }

		public int StatusCode { get; set; }
	}
}