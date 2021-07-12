using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.API.Nutrition.Functions;
using MyHealth.API.Nutrition.Services;
using MyHealth.API.Nutrition.Validators;
using MyHealth.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Nutrition.UnitTests.FunctionTests
{
    public class GetNutritionLogByDateShould
    {
        private Mock<INutritionDbService> _mockNutritionDbService;
        private Mock<IDateValidator> _mockDateValidator;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<ILogger> _mockLogger;

        private GetNutritionLogByDate _func;

        public GetNutritionLogByDateShould()
        {
            _mockNutritionDbService = new Mock<INutritionDbService>();
            _mockDateValidator = new Mock<IDateValidator>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockLogger = new Mock<ILogger>();

            _func = new GetNutritionLogByDate(
                _mockNutritionDbService.Object,
                _mockServiceBusHelpers.Object,
                _mockConfiguration.Object,
                _mockDateValidator.Object);
        }

        [Theory]
        [InlineData("2020-12-100")]
        [InlineData("2020-111-12")]
        [InlineData("20201-12-11")]
        public async Task ThrowBadRequestResultWhenNutritionDateRequestIsInvalid(string invalidDateInput)
        {
            // Arrange
            var nutritionEnvelope = new mdl.NutritionEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(nutritionEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsNutritionDateValid(invalidDateInput)).Returns(false);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object, invalidDateInput);

            // Assert
            Assert.Equal(typeof(BadRequestResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(400, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ThrowNotFoundResultWhenNutritionResponseIsNull()
        {
            // Arrange
            var nutritionEnvelope = new mdl.NutritionEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(nutritionEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsNutritionDateValid(It.IsAny<string>())).Returns(true);
            _mockNutritionDbService.Setup(x => x.GetNutritionLogByDate(It.IsAny<string>())).Returns(Task.FromResult<mdl.NutritionEnvelope>(null));

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object, "2019-12-31");

            // Assert
            Assert.Equal(typeof(NotFoundResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(404, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public async Task ReturnOkObjectResultWhenNutritionIsFound()
        {
            // Arrange
            var nutritionEnvelope = new mdl.NutritionEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Nutrition = new mdl.Nutrition
                {
                    NutritionDate = "2019-12-31"
                },
                DocumentType = "Test"
            };
            var nutritionDate = nutritionEnvelope.Nutrition.NutritionDate;
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(nutritionEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsNutritionDateValid(nutritionDate)).Returns(true);
            _mockNutritionDbService.Setup(x => x.GetNutritionLogByDate(nutritionDate)).ReturnsAsync(nutritionEnvelope);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object, nutritionDate);

            // Assert
            Assert.Equal(typeof(OkObjectResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }


        [Fact]
        public async Task Throw500InternalServerErrorStatusCodeWhenNutritionDbServiceThrowsException()
        {
            // Arrange
            var activityEnvelope = new mdl.ActivityEnvelope();
            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activityEnvelope));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockDateValidator.Setup(x => x.IsNutritionDateValid(It.IsAny<string>())).Returns(true);
            _mockNutritionDbService.Setup(x => x.GetNutritionLogByDate(It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object, "2019-12-31");

            // Assert
            Assert.Equal(typeof(StatusCodeResult), response.GetType());
            var responseAsStatusCodeResult = (StatusCodeResult)response;
            Assert.Equal(500, responseAsStatusCodeResult.StatusCode);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
