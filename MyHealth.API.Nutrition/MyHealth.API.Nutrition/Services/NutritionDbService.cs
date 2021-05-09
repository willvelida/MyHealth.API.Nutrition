using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using MyHealth.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyHealth.API.Nutrition.Services
{
    public class NutritionDbService : INutritionDbService
    {
        private readonly IConfiguration _configuration;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public NutritionDbService(
            IConfiguration configuration,
            CosmosClient cosmosClient)
        {
            _configuration = configuration;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer(_configuration["DatabaseName"], _configuration["ContainerName"]);
        }

        public async Task<List<NutritionEnvelope>> GetAllNutritionLogs()
        {
            try
            {
                QueryDefinition query = new QueryDefinition("SELECT * FROM Records c WHERE c.DocumentType = 'Nutrition'");
                List<NutritionEnvelope> nutritionEnvelopes = new List<NutritionEnvelope>();

                FeedIterator<NutritionEnvelope> feedIterator = _container.GetItemQueryIterator<NutritionEnvelope>(query);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<NutritionEnvelope> queryResponse = await feedIterator.ReadNextAsync();
                    nutritionEnvelopes.AddRange(queryResponse.Resource);
                }

                return nutritionEnvelopes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<NutritionEnvelope> GetNutritionLogByDate(string nutritionLogDate)
        {
            try
            {
                QueryDefinition query = new QueryDefinition("SELECT * FROM Records c WHERE c.DocumentType = 'Nutrition' AND c.Nutrition.NutritionDate = @nutritionLogDate")
                    .WithParameter("@nutritionLogDate", nutritionLogDate);
                List<NutritionEnvelope> nutritionEnvelopes = new List<NutritionEnvelope>();

                FeedIterator<NutritionEnvelope> feedIterator = _container.GetItemQueryIterator<NutritionEnvelope>(query);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<NutritionEnvelope> queryResponse = await feedIterator.ReadNextAsync();
                    nutritionEnvelopes.AddRange(queryResponse.Resource);
                }

                return nutritionEnvelopes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
