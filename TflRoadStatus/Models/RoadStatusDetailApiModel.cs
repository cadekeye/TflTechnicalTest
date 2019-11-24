using System.ComponentModel;

namespace TflRoadStatus.Models
{
	public class RoadStatusDetailApiModel
	{
		public string type { get; set; }

		public string id { get; set; }

		public string displayName { get; set; }

		[DisplayName("Road Status")]
		public string statusSeverity { get; set; }

		[DisplayName("Road Status Description")]
		public string statusSeverityDescription { get; set; }

		public string bounds { get; set; }

		public string envelope { get; set; }

		public string url { get; set; }
	}
}