using System;
using System.Globalization;

namespace MyHealth.API.Nutrition.Validators
{
    public class DateValidator : IDateValidator
    {
        public bool IsNutritionDateValid(string nutritionLogDate)
        {
            bool isDateValid = false;
            string pattern = "yyyy-MM-dd";
            DateTime parsedNutritionDate;

            if (DateTime.TryParseExact(nutritionLogDate, pattern, null, DateTimeStyles.None, out parsedNutritionDate))
            {
                isDateValid = true;
            }

            return isDateValid;
        }
    }
}
