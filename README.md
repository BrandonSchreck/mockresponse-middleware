<a id="readme-top"></a>
# MockResponse.Middleware


**MockResponse.Middleware** is a flexible and lightweight ASP.NET Core middleware solution designed to simplify serving mock responses during development, testing, or offline scenarios. It conditionally intercepts HTTP requests and returns configured mock responses based on defined mappings, request headers, and endpoint metadata, without altering backend logic.


> ✅ Tested with .NET 8 (ASP.NET Core Minimal API). Compatibility with earlier versions has not been verified.


## Table of Contents
* [Key Features](#-key-features)
* [Available Providers](#-available-providers)
* [Installation](#-installation)
* [Examples](#-examples)
* [How the Mock Mapping System Works](#-how-the-mock-mapping-system-works)
* [Getting Started](#-getting-started)
* [Contributing](#-contributing)


## 🚀 Key Features
* **Plug-and-play Provider Model:** Easily integrate mock responses using Azure Blob Storage, local file systems, or implement your own custom providers
* **Variant Handling:** Effortlessly switch response variations with the `X-Mock-Variant` header
* **Status Code Simulation:** Test and handle different HTTP status codes by specifying them with the `X-Mock-Status` header
* **OpenAPI Integration:** Utilize the mapping system powered by OpenAPI's `Produces<T>()` response types to automagically match response payloads

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 📚 Available Providers

* **[MockResponse.Middleware.Azure.BlobStorage](./src/MockResponse.Middleware.Azure.BlobStorage/)** - Provider using Azure Blob Storage for remote mocks
* **[MockResponse.Middleware.LocalFolderStore](./src/MockResponse.Middleware.LocalFolderStore/)** - Provider serving mocks from a local file system, perfect for offline scenarios

> **Note:** The `Core` package is only required when creating custom providers; it is automatically included with the Azure and Local Folder providers.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 📦 Installation

Install your desired provider via .NET CLI:

```bash
dotnet add package MockResponse.Middleware.Azure.BlobStorage
```
*OR*
```bash
dotnet add package MockResponse.Middleware.LocalFolderStore
```
> **Important:** Only one provider may be registered. Attempting to register more than one will throw a runtime exception.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 💡 Examples

Detailed examples demonstrating provider integrations can be found here:

* **[Azure Blob Storage Example](./examples/MockResponse.Middleware.Example.AzureBlobStorage/)**
* **[Local Folder Store Example](./examples/MockResponse.Middleware.Example.LocalFolder/)**

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🔗 How the Mock Mapping System Works

Mock responses are selected based on:
* **HTTP Status Code** (`X-Mock-Status` header)
* **Optional Variant** (`X-Mock-Variant` header)
* **Response Type Metadata** (`.Produces<T>()`)

```http
GET /weatherforecast
  X-Mock-Status: 200 // HTTP Status to lookup
```

```csharp
app.MapGet("/weatherforecast", () => ...)
    .Produces<WeatherForecast>() // 200 - Object Type to be used
    .WithName("GetWeatherForecast");
```

The middleware creates a fully qualified lookup key from these inputs, referencing entries in your `appsettings.json`:

```json
"MockOptions": {
  "ResponseMappings": {
    "Namespace.WeatherForecast": "WeatherForecast.json", // JSON to return
    "Namespace.WeatherForecast.VariantName": "WeatherForecast.Variant.json"
  }
}
```

For detailed mapping examples and explanations, refer to the [Mock Resolution Strategy](./src/MockResponse.Middleware.Core/README.md#mock-resolution-strategy) documentation.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🚦 Getting Started

Quick setup steps:
1. Install middleware provider packages.
2. Configure your desired mock provider in `appsettings.json` and/or Secrets Manager.
3. Register middleware in application startup:

```csharp
services.AddApiMocking(Configuration)
    .AddAzureBlobStorage(); // or .AddLocalFolderStore()
```

4. Enable middleware in your pipeline:

```csharp
app.UseApiMocking();
```

For comprehensive details, troubleshooting, and provider-specific configurations, see:

* **[Core Middleware Documentation](./src/MockResponse.Middleware.Core/README.md)**
* **[Azure Blob Storage Provider Documentation](./src/MockResponse.Middleware.Azure.BlobStorage/README.md)**
* **[Local Folder Store Provider Documentation](./src/MockResponse.Middleware.LocalFolderStore/README.md)**

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 📌 Contributing

Contributions and feedback are welcomed and greatly appreciated! Open an issue or submit a pull request to enhance the project. 