using Xunit;
using System.ComponentModel.DataAnnotations;
using PaymentGateway.Models;
using PaymentGateway.Attributes;

namespace PaymentGateway.Tests
{
    public class PaymentRequestTests
    {
        // CARD NUMBER TESTS
        [Fact]
        public void CardNumber_ShouldHaveRequiredAttribute()
        {
            AssertPropertyHasAttribute<RequiredAttribute>("CardNumber");
        }

        [Fact]
        public void CardNumber_ShouldHaveStringLengthAttributeWithCorrectParameters()
        {
            var attribute = GetPropertyAttribute<StringLengthAttribute>("CardNumber");
            Assert.NotNull(attribute);
            Assert.Equal(14, attribute.MinimumLength);
            Assert.Equal(19, attribute.MaximumLength);
        }

        [Fact]
        public void CardNumber_ShouldHaveRegularExpressionAttributeWithCorrectPattern()
        {
            var attribute = GetPropertyAttribute<RegularExpressionAttribute>("CardNumber");
            Assert.NotNull(attribute);
            Assert.Equal("^[0-9]*$", attribute.Pattern);
        }

        // EXPIRY DATE TESTS
        [Fact]
        public void ExpiryMonth_ShouldHaveRequiredAttribute()
        {
            AssertPropertyHasAttribute<RequiredAttribute>("ExpiryMonth");
        }

        [Fact]
        public void ExpiryMonth_ShouldHaveRangeAttributeWithCorrectParameters()
        {
            var attribute = GetPropertyAttribute<RangeAttribute>("ExpiryMonth");
            Assert.NotNull(attribute);
            Assert.Equal(1, attribute.Minimum);
            Assert.Equal(12, attribute.Maximum);
        }

        [Fact]
        public void ExpiryYear_ShouldHaveRequiredAttribute()
        {
            AssertPropertyHasAttribute<RequiredAttribute>("ExpiryYear");
        }

        [Fact]
        public void ExpiryYear_ShouldHaveRangeAttributeWithCorrectParameters()
        {
            var attribute = GetPropertyAttribute<RangeAttribute>("ExpiryYear");
            Assert.NotNull(attribute);
            Assert.Equal(1, attribute.Minimum);
            Assert.Equal(9999, attribute.Maximum);
        }

        [Fact]
        public void ExpiryYear_ShouldHaveFutureExpiryDateAttribute()
        {
            AssertPropertyHasAttribute<FutureExpiryDateAttribute>("ExpiryYear");
        }

        // CURRENCY TESTS
        [Fact]
        public void Currency_ShouldHaveRequiredAttribute()
        {
            AssertPropertyHasAttribute<RequiredAttribute>("Currency");
        }

        [Fact]
        public void Currency_ShouldHaveStringLengthAttributeWithCorrectParameters()
        {
            var attribute = GetPropertyAttribute<StringLengthAttribute>("Currency");
            Assert.NotNull(attribute);
            Assert.Equal(3, attribute.MinimumLength);
            Assert.Equal(3, attribute.MaximumLength);
        }

        // AMOUNT TESTS
        [Fact]
        public void Amount_ShouldHaveRequiredAttribute()
        {
            AssertPropertyHasAttribute<RequiredAttribute>("Amount");
        }

        [Fact]
        public void Amount_ShouldHaveRangeAttributeWithCorrectParameters()
        {
            var attribute = GetPropertyAttribute<RangeAttribute>("Amount");
            Assert.NotNull(attribute);
            Assert.Equal(1, attribute.Minimum);
            Assert.Equal(int.MaxValue, attribute.Maximum);
        }

        // CVV TESTS
        [Fact]
        public void CVV_ShouldHaveRequiredAttribute()
        {
            AssertPropertyHasAttribute<RequiredAttribute>("CVV");
        }

        [Fact]
        public void CVV_ShouldHaveStringLengthAttributeWithCorrectParameters()
        {
            var attribute = GetPropertyAttribute<StringLengthAttribute>("CVV");
            Assert.NotNull(attribute);
            Assert.Equal(3, attribute.MinimumLength);
            Assert.Equal(4, attribute.MaximumLength);
        }

        [Fact]
        public void CVV_ShouldHaveRegularExpressionAttributeWithCorrectPattern()
        {
            var attribute = GetPropertyAttribute<RegularExpressionAttribute>("CVV");
            Assert.NotNull(attribute);
            Assert.Equal("^[0-9]*$", attribute.Pattern);
        }

        // Helper Functions
        private void AssertPropertyHasAttribute<TAttribute>(string propertyName)
            where TAttribute : Attribute
        {
            var property = typeof(PaymentRequest).GetProperty(propertyName);
            Assert.NotNull(property);
            Assert.NotNull(property.GetCustomAttributes(typeof(TAttribute), false).SingleOrDefault());
        }

        private TAttribute GetPropertyAttribute<TAttribute>(string propertyName)
            where TAttribute : Attribute
        {
            var property = typeof(PaymentRequest).GetProperty(propertyName);
            return property.GetCustomAttributes(typeof(TAttribute), false).SingleOrDefault() as TAttribute;
        }
    }
}