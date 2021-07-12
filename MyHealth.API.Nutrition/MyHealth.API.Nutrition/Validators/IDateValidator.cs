namespace MyHealth.API.Nutrition.Validators
{
    public interface IDateValidator
    {
        bool IsNutritionDateValid(string nutritionLogDate);
    }
}
