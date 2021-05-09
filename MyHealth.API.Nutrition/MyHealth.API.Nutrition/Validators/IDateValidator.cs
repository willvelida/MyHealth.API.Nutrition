using System;
using System.Collections.Generic;
using System.Text;

namespace MyHealth.API.Nutrition.Validators
{
    public interface IDateValidator
    {
        bool IsNutritionDateValid(string nutritionLogDate);
    }
}
