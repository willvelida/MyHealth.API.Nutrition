using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.API.Nutrition.Services;
using MyHealth.API.Nutrition.Validators;
using MyHealth.Common;
using System;
using System.Threading.Tasks;

namespace MyHealth.API.Nutrition.Functions
{
    public class GetNutritionLogByDate
    {
        private readonly INutritionDbService _nutritionDbService;
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration;
        private readonly IDateValidator _dateValidator;

        public GetNutritionLogByDate(
            INutritionDbService nutritionDbService,
            IServiceBusHelpers serviceBusHelpers,
            IConfiguration configuration,
            IDateValidator dateValidator)
        {
            _nutritionDbService = nutritionDbService;
            _serviceBusHelpers = serviceBusHelpers;
            _configuration = configuration;
            _dateValidator = dateValidator;
        }

        [FunctionName(nameof(GetNutritionLogByDate))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "NutritionLog")] HttpRequest req,
            ILogger log)
        {
            IActionResult result;

            try
            {
                string nutritionDate = req.Query["date"];

                bool isDateValid = _dateValidator.IsNutritionDateValid(nutritionDate);
                if (isDateValid == false)
                {
                    result = new BadRequestResult();
                    return result;
                }

                var nutritionResponse = await _nutritionDbService.GetNutritionLogByDate(nutritionDate);
                if (nutritionResponse == null)
                {
                    result = new NotFoundResult();
                    return result;
                }

                var nutrition = nutritionResponse.Nutrition;
                result = new OkObjectResult(nutrition);
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
