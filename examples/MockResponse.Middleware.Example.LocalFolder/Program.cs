using MockResponse.Middleware.Core.Extensions;
using MockResponse.Middleware.Example.LocalFolder.Models;
using MockResponse.Middleware.LocalFolderStore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiMocking(builder.Configuration)
    .AddLocalFolderStore();

var app = builder.Build();
app.UseApiMocking();

app.MapGet("/weatherforecast", () =>
        new WeatherForecast(
            City: "Some City",
            State: "Texas",
            Date: DateTime.Now,
            TemperatureC: 27,
            Summary: "Some Summary"
        )
    )
    .Produces<WeatherForecast>() // 200 OK
    .WithName("GetWeatherForecast");

// Will fail because no IProducesResponseTypeMetadata ( .Produces<T>() ) are defined
app.MapGet("/failure", () => new Failure("Some Value")).WithName("Failure");

await app.RunAsync();