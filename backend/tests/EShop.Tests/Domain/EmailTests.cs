using Xunit;
using FluentAssertions;
using EShop.Domain.Customers;

namespace EShop.Tests.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co.uk")]
    [InlineData("admin@localhost.local")]
    public void Email_Should_Accept_Valid_Formats(string email)
    {
        // act
        var action = () => Email.Create(email);

        // assert
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("")]
    public void Email_Should_Reject_Invalid_Formats(string email)
    {
        // act
        var action = () => Email.Create(email);

        // assert
        action.Should().Throw<ArgumentException>();
    }
}
