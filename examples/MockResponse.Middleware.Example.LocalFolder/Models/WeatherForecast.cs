namespace MockResponse.Middleware.Example.LocalFolder.Models;

internal record WeatherForecast(string City, string State, DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}