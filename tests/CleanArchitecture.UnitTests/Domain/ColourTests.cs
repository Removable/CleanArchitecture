using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Domain.TodoListAggregate;

namespace CleanArchitecture.UnitTests.Domain;

public class ColourTests
{
    [Fact]
    public void ShouldReturnCorrectColourCode()
    {
        const string code = "#FFFFFF";

        var colour = Colour.From(code);

        colour.Code.ShouldBe(code);
    }
    
    [Fact]
    public void ToStringReturnsCode()
    {
        var colour = Colour.White;

        colour.ToString().ShouldBe(colour.Code);
    }

    [Fact]
    public void ShouldPerformImplicitConversionToColourCodeString()
    {
        string code = Colour.White;

        code.ShouldBe("#FFFFFF");
    }

    [Fact]
    public void ShouldPerformExplicitConversionGivenSupportedColourCode()
    {
        var colour = (Colour)"#FFFFFF";

        colour.ShouldBe(Colour.White);
    }

    [Fact]
    public void ShouldThrowUnsupportedColourExceptionGivenNotSupportedColourCode()
    {
        Should.Throw<UnsupportedColourException>(() => Colour.From("##FF33CC"));
    }
}
