using Xunit;
using FluentAssertions;
using EShop.Domain.Products;

namespace EShop.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Product_Should_Be_Created_With_Valid_Data()
    {
        // arrange
        var id = ProductId.New();
        var name = "test product";
        var price = Money.Create(99.99m);
        var sku = Sku.Generate(name, "china");

        // act
        var product = new Product(id, name, price, sku, "china", "warehouse a");

        // assert
        product.Id.Should().Be(id);
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
    }

    [Fact]
    public void Money_Should_Not_Allow_Negative_Amount()
    {
        // act & assert
        var action = () => Money.Create(-10);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Sku_Should_Be_Stable_For_Same_Input()
    {
        // arrange
        var name = "widget";
        var origin = "usa";

        // act
        var sku1 = Sku.Generate(name, origin);
        var sku2 = Sku.Generate(name, origin);

        // assert
        sku1.Should().Be(sku2);
    }
}
