using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.API.Nutrition.Services;
using MyHealth.API.Nutrition.UnitTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Nutrition.UnitTests.ServiceTests
{
    public class NutritionDbServiceShould
    {
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Container> _mockContainer;
        private Mock<IConfiguration> _mockConfiguration;

        private NutritionDbService _sut;

        public NutritionDbServiceShould()
        {
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _mockCosmosClient.Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_mockContainer.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["DatabaseName"]).Returns("db");
            _mockConfiguration.Setup(x => x["ContainerName"]).Returns("col");

            _sut = new NutritionDbService(
                _mockConfiguration.Object,
                _mockCosmosClient.Object);
        }

        [Fact]
        public async Task GetAllNutritionLogs()
        {
            // Arrange
            List<mdl.NutritionEnvelope> nutritionEnvelopes = new List<mdl.NutritionEnvelope>();
            mdl.NutritionEnvelope nutritionEnvelope = new mdl.NutritionEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                DocumentType = "Test",
                Nutrition = new mdl.Nutrition
                {
                    NutritionDate = "2021-05-07"
                }
            };
            nutritionEnvelopes.Add(nutritionEnvelope);

            _mockContainer.SetupItemQueryIteratorMock(nutritionEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int> { nutritionEnvelopes.Count });

            // Act
            var response = await _sut.GetAllNutritionLogs();

            // Assert
            Assert.Equal(nutritionEnvelopes.Count, response.Count);
        }

        [Fact]
        public async Task GetAllActivies_NoResultsReturned()
        {
            // Arrange
            List<mdl.NutritionEnvelope> nutritionEnvelopes = new List<mdl.NutritionEnvelope>();

            var getLogs = _mockContainer.SetupItemQueryIteratorMock(nutritionEnvelopes);
            getLogs.feedIterator.Setup(x => x.HasMoreResults).Returns(false);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { 0 });

            // Act
            var response = await _sut.GetAllNutritionLogs();

            // Act
            Assert.Empty(response);
        }

        [Fact]
        public async Task CatchExceptionWhenCosmosThrowsExceptionWhenGetActivitiesIsCalled()
        {
            // Arrange
            _mockContainer.Setup(x => x.GetItemQueryIterator<mdl.NutritionEnvelope>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
                .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.GetAllNutritionLogs();

            // Act
            await responseAction.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetActivityByDate()
        {
            // Arrange
            List<mdl.NutritionEnvelope> nutritionEnvelopes = new List<mdl.NutritionEnvelope>();
            mdl.NutritionEnvelope nutritionEnvelope = new mdl.NutritionEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                DocumentType = "Test",
                Nutrition = new mdl.Nutrition
                {
                    NutritionDate = "2021-05-07"
                }
            };
            nutritionEnvelopes.Add(nutritionEnvelope);

            _mockContainer.SetupItemQueryIteratorMock(nutritionEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int> { nutritionEnvelopes.Count });

            var nutritionDate = nutritionEnvelope.Nutrition.NutritionDate;

            // Act
            var response = await _sut.GetNutritionLogByDate(nutritionDate);

            // Assert
            Assert.Equal(nutritionDate, response.Nutrition.NutritionDate);
        }

        [Fact]
        public async Task GetActivityByDate_NoResultsReturned()
        {
            // Arrange
            var emptyActivitiesList = new List<mdl.NutritionEnvelope>();

            var getActivities = _mockContainer.SetupItemQueryIteratorMock(emptyActivitiesList);
            getActivities.feedIterator.Setup(x => x.HasMoreResults).Returns(false);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { 0 });

            // Act
            var response = await _sut.GetNutritionLogByDate("2021-05-01");

            // Act
            Assert.Null(response);
        }

        [Fact]
        public async Task CatchExceptionWhenCosmosThrowsExceptionWhenGetActivityByDateIsCalled()
        {
            // Arrange
            _mockContainer.Setup(x => x.GetItemQueryIterator<mdl.NutritionEnvelope>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
                .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.GetNutritionLogByDate("2021-05-01");

            // Act
            await responseAction.Should().ThrowAsync<Exception>();
        }
    }
}
