using MyHealth.API.Nutrition.Validators;
using Xunit;

namespace MyHealth.API.Nutrition.UnitTests.ValidatorTests
{
    public class DateValidatorShould
    {
        private DateValidator _sut;

        public DateValidatorShould()
        {
            _sut = new DateValidator();
        }

        [Fact]
        public void ReturnFalseIfNutritionDateIsNotInValidFormat()
        {
            // Arrange
            string testNutritionDate = "100/12/2021";

            // Act
            var response = _sut.IsNutritionDateValid(testNutritionDate);

            // Assert
            Assert.False(response);
        }

        [Fact]
        public void ReturnTrueIfNutritionDateIsInValidFormat()
        {
            // Arrange
            string testNutritionDate = "2020-12-31";

            // Act
            var response = _sut.IsNutritionDateValid(testNutritionDate);

            // Assert
            Assert.True(response);
        }
    }
}
