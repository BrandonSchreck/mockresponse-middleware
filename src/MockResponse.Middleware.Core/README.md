<a id="readme-top"></a>
# MockResponse.Middleware.Core

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/github/license/BrandonSchreck/mockresponse-middleware)

[![CI/CD Pipeline](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/main.yml/badge.svg)](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/main.yml)
[![CodeQL](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/github-code-scanning/codeql)
[![NuGet - Core](https://img.shields.io/nuget/v/MockResponse.Middleware.Core.svg)](https://www.nuget.org/packages/MockResponse.Middleware.Core/)

This package provides the core abstractions, middleware, and configuration models used by `MockResponse.Middleware`.

It defines shared interfaces, options models, resolution strategies, and the base middleware pipeline that provider-specific 
implementations (e.g. Azure Blob Storage, Local Folder Store, or your own) can plug into.


## Table of Contents
* [Installation](#-installation)
* [Intended Usage](#-intended-usage)
* [Features](#-features)
* [Mock Resolution Strategy](#-mock-resolution-strategy)
* [MockOptions Configuration](#️-mockoptions-configuration)
* [Troubleshooting](#-troubleshooting)


## 📦 Installation

```bash
dotnet add package MockResponse.Middleware.Core
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🚦 Intended Usage

This package is **not intended to be used directly**. Instead, install one of the following provider packages based on your needs:

* [MockResponse.Middleware.Azure.BlobStorage](../MockResponse.Middleware.Azure.BlobStorage/README.md)
* [MockResponse.Middleware.LocalFolderStore](../MockResponse.Middleware.LocalFolderStore/README.md)

### 🔧 Create Your Own Provider

If neither built-in provider suits your scenario, you can create a custom one by following these steps:

1. **Create a new class library**, e.g. `MockResponse.Middleware.CustomType`

2. **Reference dependencies**

```xml
<PackageReference Include="MockResponse.Middleware.Core" Version="x.y.z" />
<PackageReference Include="Microsoft.Extensions.Options" Version="x.y.z" />
```

3. **Define your options**

```csharp
public record CustomProviderOptions : IProviderOptions
{
	public static string SectionName => nameof(CustomProviderOptions);

	[Required]
	public string SomeSetting { get; set; } = default!;
}
```

4. **Implement the provider**

```csharp
internal sealed class CustomResponseProvider : IMockResponseProvider, IMockResponseProviderDefinition
{
	public static string ProviderName => "CustomProvider";
	public string Name => ProviderName;
	private readonly CustomProviderOptions _options;
	
	public CustomResponseProvider(IOptionsMonitor<CustomProviderOptions> opts)
	{
		_options = opts.CurrentValue;
	}

	public Task<(string Response, string ProviderName)> GetMockResponseAsync(string identifier)
	{
		// this is where are the logic happens...
		return Task.FromResult(("{\"ok\":true}", Name));
	}
}
```

5. **Register your provider**

```csharp
services.AddApiMocking(Configuration)
	.AddStore<CustomProviderOptions, CustomResponseProvider>(CustomResponseProvider.Name);
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## ✨ Features

* Core middleware pipeline for injecting mock responses
* Pluggable provider model via `IMockResponseProvider`
* Support for:
	* path exclusion
	* Metadata resolution from endpoint definitions
	* Flexible configuration via `IOptionsMonitor<T>`

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🔗 Mock Resolution Strategy

The mock resolution process is driven by **the matched endpoint, response metadata, and request headers**. Here's how it works:

### How Mapping Works

Mock responses are resolved using a **combination of the request's route (via endpoint metadata), response status code, and an optional variant header.**

Each route endpoint that uses `.Produces<T>()` defines a C# type (`T`) that becomes part of the lookup key in `MockOptions:ResponseMappings`.

> Note: By default, if no status is passed to `.Produces<T>()`, it defaults to 200 OK. 
>
> **200 OK**
>    ```
>    .Produces<WeatherForecast>();
>    ```
> **201 Created**
>    ```
>    .Produces<WeatherForecast>(StatusCodes.Status201Created);
>    ```

#### Mapping Format

```
  "<Namespace>.<ClassName>[.<X-Mock-Variant>]":"<Identifier>"
```

- `<Namespace>.<ClassName>`: The fully qualified name of the response model (from `.Produces<T>()`)
- `<X-Mock-Variant>`: *(Optional)* Custom variant passed in the `X-Mock-Variant` header
- `<Identifier>`: The value passed to the mock provider (this might be a file name, blob name, database key, etc)

> Note: If no `X-Mock-Variant` header is provided, the middleware will look for a mapping using just the fully qualified type name.

#### Examples

##### Without Variant
For this endpoint:
```csharp
app.MapGet("/weatherforecast", () => ...).Produces<WeatherForecast>();
```

And this request:
```http
GET /weatherforecast
  X-Mock-Status: 200
```

The middleware matches:
* The `/weatherforecast` route -> associated endpoint
* The declared response type for status 200 -> `Your.Namespace.WeatherForecast`
* The following mapping:

```json
"ResponseMappings": {
  "Your.Namespace.WeatherForecast": "WeatherForecast.json"
}
```

✅ If found, it returns the content from `WeatherForecast.json` (or whatever identifier is needed by the provider)

❌ If not found, a `404 Not Found` is returned

> Note: If no `.Produces<T>(<StatusCode>)` exists for the given status, the system cannot infer the appropriate mock response type and will return an error.

##### With Variant
Using the same endpoint above, but with:
```http
GET /weatherforecast
  X-Mock-Status: 200
  X-Mock-Variant: Freezing
```

The middleware appends the response type variant (`Freezing`) to the fully qualified class name:
```
Your.Namespace.WeatherForecast.Freezing
```

And looks for a match in:
```json
"ResponseMappings": {
  "Your.Namespace.WeatherForecast.Freezing": "WeatherForecast.Freezing.json"
}
```
✅ If found, it returns the mock for the variant

❌ If not, a 404 is returned with an error explaining the missing mapping

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## ⚙️ MockOptions Configuration

The `MockOptions` class defines middleware behavior and is bound from the `MockOptions` section of your configuration (e.g. `appsettings.json` or User Secrets).

### Structure

```json
{
  "MockOptions": {
    "ExcludedRequestPaths": [ "/api/health", "/openapi", "/metrics", "/redoc", "/swagger" ],
    "ResponseMappings": {
      "Some.Namespace.WeatherForecast": "WeatherForecast.json",
      "Some.Namespace.WeatherForecast.Freezing": "subfolder/WeatherForecast.Freezing.json"
    },
    "UseMock": true
  }
}
```
> 🔧 These options are bound via `services.Configure<MockOptions>()` as part of `AddApiMocking(...)` setup and can be accessed through `IOptionsMonitor<MockOptions>` if needed for advanced scenarios.

### Description of Properties

|Property|Type|Description|Default|
|----|----|----|----|
| `UseMock` | `bool` | Enables or disables mocking. When `true`, matching requests will return mock responses instead of reaching real endpoints. | `false` |
| `ExcludedRequestPaths` | `string[]` | Optional list of path relatives to bypass mock resolution. Useful for excluding routes like health checks, OpenAPI docs, or metrics endpoints. | `[]` (empty) |
| `ResponseMappings` | `Dictionary<string,string>` | Maps fully qualified response type names (optionally including variants) to identifiers (like filenames or blob keys) used by the selected provider. Case-insensitive. | {} |
> ℹ️ Response type keys in `ResponseMappings` must match the fully qualified type name declared in `.Produces<T>()`. Casing is ignored.

For full property definitions, see [MockOptions](./Options/MockOptions.cs).

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🧪 Troubleshooting

### 400 Bad Request

#### ⚠️ Missing required 'X-Mock-Status' header

Make sure the request includes the `X-Mock-Status` header with a valid integer value.

#### ⚠️ '"200"' is not a valid StatusCode

Ensure you are passing a valid integer (200) in the `X-Mock-Status` header...not a string "200".

### 404 Not Found

#### ⚠️ Metadata not found

**Error Message**

No [400] status code metadata was found for endpoint [HTTP: GET /weatherforecast]

**Cause**

The endpoint does not define `.Produces<T>()` for the given status code.

**Solution**

Add `.Produces<T>(StatusCodes.Status400BadRequest)` to the route definition (or whatever status code you were trying to use).

#### ⚠️ Mock not defined

**Error Message**

No [200] status code mapping was found for [HTTP: GET /weatherforecast] and variant [Inferno]

**Cause**

The middleware found metadata for the status code, but no mapping exists for the resolved response type and variant.

**Solution**

Ensure `MockOptions:ResponseMappings` contains a mapping for the full key. For example:
```json
"ResponseMappings": {
  "MockResponse.Middleware.Example.LocalFolder.Models.WeatherForecast.Inferno": "WeatherForecast.Inferno.json"
}
```

Double-check that the key name matches exactly, including casing and variant.

<p align="right">(<a href="#readme-top">back to top</a>)</p>