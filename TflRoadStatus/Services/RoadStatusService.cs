using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using TflRoadStatus.Contracts;
using TflRoadStatus.Models;
using System;

namespace TflRoadStatus.Services
{
	public class RoadStatusService : IRoadStatusService
	{
		private readonly ILogger<RoadStatusService> _logger;
		private readonly HttpClient _httpClient;
		private readonly IOptions<RemoteServiceSettings> _appSettings;

		public RoadStatusService(ILoggerFactory loggerFactory,
			HttpClient httpClient,
			IOptions<RemoteServiceSettings> appSettings)
		{
			_logger = loggerFactory.CreateLogger<RoadStatusService>();
			_httpClient = httpClient;
			_appSettings = appSettings;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">the road id</param>
        /// <returns>RoadStatusApiModel</returns>
        public async Task<RoadStatusApiModel> GetRoadStatusDetail(string input)
		{
            try
            {
                RoadStatusApiModel result = new RoadStatusApiModel();

                var url = string.Format("{0}{1}?app_id={2}&app_key={3}",
                    _httpClient.BaseAddress.ToString(),
                    input,
                    _appSettings.Value.ApiId,
                    _appSettings.Value.ApiKey);

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await _httpClient.GetAsync(request.RequestUri);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonConvert.DeserializeObject<IEnumerable<RoadStatusDetailApiModel>>(content);

                    result.RoadStatus = apiResult.ToArray();
                    result.StatusCode = 0;
                }
                else
                {
                    result.RoadStatus = null;
                    result.StatusCode = 1;
                }
                return result;
            }
            catch(Exception ex)
            {
                throw new HttpRequestException("Error: Expected Error occured.", ex);
            }
		}
	}
}