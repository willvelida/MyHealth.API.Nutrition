using System.Collections.Generic;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.API.Nutrition.Services
{
    public interface INutritionDbService
    {
        /// <summary>
        /// Retrieves all nutrition logs from the Records container.
        /// </summary>
        /// <returns></returns>
        Task<List<mdl.NutritionEnvelope>> GetAllNutritionLogs();

        /// <summary>
        /// Gets a nutrition record for a provided date.
        /// </summary>
        /// <param name="nutritionLogDate"></param>
        /// <returns></returns>
        Task<mdl.NutritionEnvelope> GetNutritionLogByDate(string nutritionLogDate);
    }
}
