using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.API.Nutrition.Functions;
using MyHealth.API.Nutrition.Services;
using MyHealth.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Nutrition.UnitTests.FunctionTests
{
    public class GetAllNutritionLogsShould
    {
        private Mock<INutritionDbService> _mockNutritionDbService;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger> _mockLogger;

        private GetAllNutritionLogs _func;

        public GetAllNutritionLogsShould()
        {
            _mockNutritionDbService = new Mock<INutritionDbService>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger>();

            _func = new GetAllNutritionLogs(
                _mockNutritionDbService.Object,
                _mockServiceBusHelpers.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenNutritionLogsAreFound()
        {
            // Arrange
            var nutritions = new List<mdl.NutritionEnvelope>();
            var nutritionEnvelope = new mdl.NutritionEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Nutrition = new mdl.Nutrition
                {
                    NutritionDate = "2021-05-06"
                },
                DocumentType = "Test"
            };
            nutritions.Add(nutritionEnvelope);
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(nutritions));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockNutritionDbService.Setup(x => x.GetAllNutritionLogs()).ReturnsAsync(nutritions);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenNoActivitiesFound()
        {
            // Arrange
            var nutritions = new List<mdl.NutritionEnvelope>();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(nutritions));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockNutritionDbService.Setup(x => x.GetAllNutritionLogs()).ReturnsAsync(nutritions);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task Throw500InternalServerErrorStatusCodeWhenActivityDbServiceThrowsException()
        {
            // Arrange
            var nutritions = new List<mdl.NutritionEnvelope>();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(nutritions));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockNutritionDbService.Setup(x => x.GetAllNutritionLogs()).ThrowsAsync(new Exception());

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(StatusCodeResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(500, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
