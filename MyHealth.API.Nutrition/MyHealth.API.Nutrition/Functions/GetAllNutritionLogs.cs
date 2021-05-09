using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Nutrition.Services;
using MyHealth.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Nutrition.Functions
{
    public class GetAllNutritionLogs
    {
        private readonly INutritionDbService _nutritionDbService;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;

        public GetAllNutritionLogs(
            INutritionDbService nutritionDbService,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration)
        {
            _nutritionDbService = nutritionDbService;
            _serviceBusHelpers = serviceBusHelpers;
            _configuration = configuration;
        }

        [FunctionName(nameof(GetAllNutritionLogs))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "NutritionLogs")] HttpRequest req,
            ILogger log)
        {
            IActionResult result;
            List<mdl.Nutrition> nutritionLogs = new List<mdl.Nutrition>();

            try
            {
                var nutritionEnvelopeLogs = await _nutritionDbService.GetAllNutritionLogs();

                foreach (var item in nutritionEnvelopeLogs)
                {
                    nutritionLogs.Add(item.Nutrition);
                }

                result = new OkObjectResult(nutritionLogs);
            }
            catch (Exception ex)
            {
                log.LogError($"Internal Server Error. Exception thrown: {ex.Message}");
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
