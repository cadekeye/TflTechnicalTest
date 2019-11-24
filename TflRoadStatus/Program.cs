using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using TflRoadStatus.Contracts;
using TflRoadStatus.Models;
using TflRoadStatus.Services;

namespace TflRoadStatus
{
	internal class Program
	{
		public static int ApiStatusCode { get; set; }

		private static int Main(string[] args)
		{
			try
			{
                if (args.Length != 1)
                {
                    Console.WriteLine("Please supply only the road id");
                }
                else
				{
					string input = args[0];

					var builder = new ConfigurationBuilder()
					 .SetBasePath(Directory.GetCurrentDirectory())
					 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

					IConfigurationRoot configuration = builder.Build();

					var serviceProvider = new ServiceCollection()
						.AddLogging()
						.Configure<RemoteServiceSettings>(configuration.GetSection("RemoteService"))
						.AddHttpClient<IRoadStatusService, RoadStatusService>((sp, client) =>
						{
							var httpClientOptions = sp
							  .GetRequiredService<IOptions<RemoteServiceSettings>>()
							  .Value;

							client.BaseAddress = new Uri(httpClientOptions.BaseUrl);//new Uri(configuration["RemoteServiceBaseUrl"]);
							client.DefaultRequestHeaders.Accept.Clear();
							client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
							client.DefaultRequestHeaders.Add("app_key", httpClientOptions.ApiKey);
							client.DefaultRequestHeaders.Add("app_id", httpClientOptions.ApiId);
						})
						.SetHandlerLifetime(TimeSpan.FromMinutes(5))
						.Services
						.BuildServiceProvider();

					serviceProvider
						.GetService<ILoggerFactory>();

					var logger = serviceProvider.GetService<ILoggerFactory>()
						.CreateLogger<Program>();

					logger.LogDebug("Starting application");

					var roadStatusService = serviceProvider.GetService<IRoadStatusService>();
					var roadStatusDetails = roadStatusService.GetRoadStatusDetail(input).GetAwaiter().GetResult();

                    ApiStatusCode = roadStatusDetails.StatusCode;

					ShowRoadStatus(roadStatusDetails, input);

				}
				Console.ReadKey();
                return ApiStatusCode;
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("Error: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                Console.ReadKey();
                return 1;
			}
		}

		private static void ShowRoadStatus(RoadStatusApiModel roadStatusApi, string input)
		{
			if (roadStatusApi.StatusCode == 0)
			{
				Console.WriteLine($"The Status of the {roadStatusApi.RoadStatus[0].displayName} is as follows\r\n Road Status is {roadStatusApi.RoadStatus[0].statusSeverity}\r\n" +
					$"Road Status Description {roadStatusApi.RoadStatus[0].statusSeverityDescription}");
			}
			else
			{
                Console.WriteLine($"{input.First().ToString().ToUpper()}{input.Substring(1)} is not a valid road");
            }
        }
	}
}