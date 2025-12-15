using Xunit;
using FluentAssertions;
using EShop.Domain.Orders;

namespace EShop.Tests.Domain;

public class TrackingNumberTests
{
    [Fact]
    public void TrackingNumber_Should_Have_Correct_Format()
    {
        // act
        var tracking = TrackingNumber.Generate("US");

        // assert
        tracking.Value.Should().StartWith("Unq");
        tracking.Value.Should().EndWith("US");
        tracking.Value.Length.Should().Be(14); // Unq + 9 digits + 2 letters
    }

    [Fact]
    public void TrackingNumber_Should_Validate_Format()
    {
        // act & assert
        var validAction = () => TrackingNumber.Create("Unq123456789US");
        validAction.Should().NotThrow();

        var invalidAction = () => TrackingNumber.Create("invalid");
        invalidAction.Should().Throw<ArgumentException>();
    }
}
