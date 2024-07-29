using System.Collections.Immutable;
using System.Text;
using ConsoleApp;
using Xunit.Abstractions;

namespace Tests;

public class CityTemperatureStatCalcTest
{
    [Theory]
    [InlineData("cit")]
    [InlineData("city")]
    [InlineData("city12")]
    [InlineData("city1234")]
    [InlineData("city123456")]
    public void EnsureCitiesAreUnique(string cityName)
    {
        var cityTemp1 = new CityTemp(Encoding.UTF8.GetBytes(cityName), "12.3"u8);
        var cityTemp2 = new CityTemp(Encoding.UTF8.GetBytes(cityName), "19.3"u8);

        var stats = new CityTemperatureStatCalc(2);
        stats.AddCityTemp(cityTemp1);
        stats.AddCityTemp(cityTemp2);

        Assert.Equal(1, stats.NumCities);
    }
}