using Core.Infrastructure.CrossCutting.Text;

namespace Core.Web.Tests.Text;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("(35) 99999-9999", "+5535999999999")]
    [InlineData("35 3531-1234", "+553535311234")]
    [InlineData("+55 35 99999-9999", "+5535999999999")]
    public void Converts_brazilian_numbers_to_e164(string input, string expected)
    {
        Assert.Equal(expected, PhoneNumber.ToE164Brazil(input, out var valid));
        Assert.True(valid);
    }

    [Fact]
    public void Empty_is_valid_and_null()
    {
        Assert.Null(PhoneNumber.ToE164Brazil("  ", out var valid));
        Assert.True(valid);
    }

    [Fact]
    public void Too_short_is_invalid()
    {
        PhoneNumber.ToE164Brazil("9999-999", out var valid);
        Assert.False(valid);
    }
}
