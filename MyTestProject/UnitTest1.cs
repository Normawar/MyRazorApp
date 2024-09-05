using Xunit;

namespace MyTestProject
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            var expectedValue = 5;
            var actualValue = 2 + 3;

            // Act & Assert
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
