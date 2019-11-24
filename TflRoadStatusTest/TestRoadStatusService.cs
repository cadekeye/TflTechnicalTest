
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TflRoadStatus.Contracts;
using TflRoadStatus.Models;
using TflRoadStatus.Services;

namespace TflRoadStatusTest
{
    public class TestRoadStatusService
    {
        public IRoadStatusService roadStatusService { get; set; }
        private HttpClient httpClient;
        private Mock<ILoggerFactory> loggerMock;
        private Mock<IOptions<RemoteServiceSettings>> optionMock;

        [SetUp]
        public void Setup()
        {
            httpClient = new HttpClient();

            loggerMock = new Mock<ILoggerFactory>();

            optionMock = new Mock<IOptions<RemoteServiceSettings>>();

            optionMock.Setup(x => x.Value).Returns(new RemoteServiceSettings
            {
                ApiId = "743e932d",
                ApiKey = "a450397c0cb433cd21547912373193b1",
                BaseUrl = "https://api.tfl.gov.uk/Road/"
            });
        }
        [Test]
        [TestCase("https://api.tfl.gov.uk/Road/")]
        public async Task TestWithValidUrlReturnCorrectResult (string baseUrl)
        {
            // Arrange
            httpClient.BaseAddress  = new Uri(baseUrl);

            roadStatusService = new RoadStatusService(loggerMock.Object, httpClient, optionMock.Object);

            // Act
            var result = await roadStatusService.GetRoadStatusDetail("a2");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.StatusCode);
            Assert.AreEqual(1, result.RoadStatus.Count());
        }

        [Test]
        [TestCase("https://text/")]
        public async Task TestWithInValidUrlRaiseException(string baseUrl)
        {
            // Arrange
            httpClient.BaseAddress = new Uri(baseUrl);

            roadStatusService = new RoadStatusService(loggerMock.Object, httpClient, optionMock.Object);

            // Act
            HttpRequestException ex = await Task.Run(() => Assert.ThrowsAsync<HttpRequestException>(async () => await roadStatusService.GetRoadStatusDetail("a2")));

            // Assert
            Assert.AreEqual("Error: Expected Error occured.", ex.Message);
            Assert.IsTrue(ex.InnerException.Message.Contains("No such host is known"));
        }

        [Test]
        [TestCase("https://api.tfl.gov.uk/Road/","a2")]
        public async Task TestWithValidRoadIdReturnZeroExitCode(string baseUrl, string roadId)
        {
            // Arrange
            httpClient.BaseAddress = new Uri(baseUrl);

            roadStatusService = new RoadStatusService(loggerMock.Object, httpClient, optionMock.Object);

            // Act
            var result = await roadStatusService.GetRoadStatusDetail(roadId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.StatusCode);
            Assert.AreEqual(1, result.RoadStatus.Count());
        }

        [Test]
        [TestCase("https://api.tfl.gov.uk/Road/", "a233")]
        public async Task TestWithInValidRoadIdReturnNonZeroExitCode(string baseUrl, string roadId)
        {
            // Arrange
            httpClient.BaseAddress = new Uri(baseUrl);

            roadStatusService = new RoadStatusService(loggerMock.Object, httpClient, optionMock.Object);

            // Act
            var result = await roadStatusService.GetRoadStatusDetail(roadId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.RoadStatus);
            Assert.AreEqual(1, result.StatusCode);
        }

        [Test]
        [TestCase("https://api.tfl.gov.uk/Road/", "a2","invalid-test-key","invalid-test-id")]
        public async Task TestWithInValidAppKeyAndIdRaiseException(string baseUrl, 
            string roadId, 
            string appkey, 
            string appId)
        {
            // Arrange
            httpClient.BaseAddress = new Uri(baseUrl);

            roadStatusService = new RoadStatusService(loggerMock.Object, httpClient, optionMock.Object);

            optionMock.Setup(x => x.Value).Returns(new RemoteServiceSettings
            {
                ApiId = appId,
                ApiKey = appkey,
                BaseUrl = baseUrl
            });

            // Act
            HttpRequestException ex = await Task.Run(() => Assert.ThrowsAsync<HttpRequestException>(async () => await roadStatusService.GetRoadStatusDetail("a2")));

            // Assert
            Assert.AreEqual("Error: Expected Error occured.", ex.Message);
            Assert.IsTrue(ex.InnerException.Message.Contains("Unexpected character encountered"));
        }

        [Test]
        [TestCase("https://api.tfl.gov.uk/Road/", "")]
        public async Task TestWithEmptyRoadIdReturnZeroExitCode(string baseUrl, string roadId)
        {
            // Arrange
            httpClient.BaseAddress = new Uri(baseUrl);

            roadStatusService = new RoadStatusService(loggerMock.Object, httpClient, optionMock.Object);

            // Act
            var result = await roadStatusService.GetRoadStatusDetail(roadId);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.RoadStatus.Any());
            Assert.AreEqual(0, result.StatusCode);
        }

    }
}
