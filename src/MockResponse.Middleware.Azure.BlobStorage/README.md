<a id="readme-top"></a>
# MockResponse.Middleware.Azure.BlobStorage

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/github/license/BrandonSchreck/mockresponse-middleware)

[![CI/CD Pipeline](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/main.yml/badge.svg)](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/main.yml)
[![CodeQL](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/github-code-scanning/codeql)
[![NuGet - Azure Blob Storage](https://img.shields.io/nuget/v/MockResponse.Middleware.Azure.BlobStorage.svg)](https://www.nuget.org/packages/MockResponse.Middleware.Azure.BlobStorage/)

This package provides an Azure Blob Storage-backed provider for use with the `MockResponse.Middleware` system. It enables serving mock API responses from Azure Blob Storage, ideal for testing and shared development environments where cloud-based access is still needed.


## Table of Contents
* [Installation](#-installation)
* [Configuration](#️-configuration)
* [BlobStorageOptions Configuration](#-blobstorageoptions-configuration)
* [Usage: AddAzureBlobStorage](#-usage-addazureblobstorage)
* [Troubleshooting](#-troubleshooting)

## 📦 Installation

```bash
dotnet add package MockResponse.Middleware.Azure.BlobStorage
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## ⚙️ Configuration

Configure the Azure Blob Storage provider using [Microsoft Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0) to avoid storing sensitive information in plain text files.

1. Initialize Secrets Manager for your project (if not already initialized):

```bash
dotnet user-secrets init
```

2. Add your Azure Blob Storage settings:

```bash
dotnet user-secrets set "MockOptions:BlobStorageOptions:ConnectionString" "<your-connection-string>"
dotnet user-secrets set "MockOptions:BlobStorageOptions:ContainerName" "<your-container-name>"
```

These values are securely stored in your local secrets store and accessed via `IConfiguration` at runtime.

3. Add additional mock configuration properties to your `appsettings.json`:

```json
{
  "MockOptions": {
    "ExcludedRequestPaths": ["/api/health", "/openapi", "/metrics", "/redoc", "/swagger"],
    "ResponseMappings": {
      "Namespace.TypeName": "ExampleResponse.json",
      "Namespace.TypeName.Variant": "subfolder/ExampleResponse.Variant.json"
    },
    "UseMock": true
  }
}
```
> 💡 Azure Blob Storage supports virtual folders by including `/` in blob names.
>
> For example, `"subfolder/ExampleResponse.Variant.json"` will correctly resolve and retrieve the mock from the `subfolder` virtual directory.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🔧 BlobStorageOptions Configuration

The following configuration properties are used by the Azure Blob Storage mock provider. These settings are nested under `MockOptions:BlobStorageOptions`.

| Property | Type | Required | Description |
| ---- | ---- | ---- | ---- |
| `ConnectionString` | `string` | ✅ | Azure Blob Storage connection string with permission to read blobs from the configured container. |
| `ContainerName` | `string` | ✅ | The name of the container where mock JSON files are stored. |

> 🔐 These values should be stored in [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0) or injected securely via environment variables or Key Vault.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🚀 Usage: AddAzureBlobStorage

Register the middleware in `Startup.cs` or `Program.cs`:

```csharp
services.AddApiMocking(Configuration)
	.AddAzureBlobStorage();
```

Use the middleware:

```csharp
app.UseApiMocking();
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🧪 Troubleshooting

The Azure Blob Storage mock provider may encounter configuration or runtime errors that prevent it from serving mock responses.
> ℹ️ **Note:** Detailed error messages (such as missing or invalid configuration) are logged by the application. The HTTP response will typically return a generic error message like:
> ```html
> 500 - Application configuration is missing or invalid. See logs for more details.
> ```
> Check application logs for full diagnostic information, including exception details and stack traces.

### Missing Configurations

#### ⚠️ Missing `ConnectionString`

**Error Message**

The ConnectionString field is required.

**Cause**

The `ConnectionString` was not provided in `MockOptions:BlobStorageOptions`, or it wasn't loaded via configuration (e.g. missing from secrets or environment).

**Solution**

Set the connection string securely via [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0), environment variables, or other config providers:

```bash
dotnet user-secrets set "MockOptions:BlobStorageOptions:ConnectionString" "<your-connection-string>"
```

#### ⚠️ Missing `ContainerName`

**Error Message**

The ContainerName field is required.

**Cause**

The `ContainerName` was not provided in `MockOptions:BlobStorageOptions`, or it wasn't loaded via configuration (e.g. missing from secrets or environment).

**Solution**

Set the container name securely via [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0), environment variables, or other config providers:

```bash
dotnet user-secrets set "MockOptions:BlobStorageOptions:ContainerName" "<your-container-name>"
```


### Runtime Errors

#### ⚠️ Unable to Access Azure Storage Container

**Error Message**

```html
500 - An application configuration error occurred. See logs for more details.
```
> The application returns a generic `500 Internal Server Error` to avoid exposing sensitive details. Check the logs for specific root cause.

**Example Log Output**
```text
An application configuration error occurred. See logs for more details. Unable to access the Azure Storage Container: The account being accessed does not support http.
RequestId:********-****-****-****-************
Status: 400 (The account being accessed does not support http.)
ErrorCode: AccountRequiresHttps
```

**Cause**

The Azure Blob Storage provider failed to connect to the configured container. This is often due to:
* Incorrect or incomplete `ConnectionString`
* Nonexistent or misnamed container
* Missing permissions or expired credentials
* Storage account requiring HTTPS
* Network restrictions, firewall rules, or IP filtering

**Solution**

Check your `MockOptions:BlobStorageOptions` configuration and ensure:
* The connection string uses `https://` (not `http://`)
* The `ContainerName` is correct and the container exists
* The storage account key or credentials are valid and have read access

> 🔐 Ensure the storage account connection string has **read permissions** to the container. This is typically required if using a shared access key or managed identity.

> 🔗 See also: [Azure Storage error codes](https://learn.microsoft.com/en-us/rest/api/storageservices/common-rest-api-error-codes)

> 📘 For additional error handling scenarios, see the [Core Troubleshooting Guide](../MockResponse.Middleware.Core/README.md#-troubleshooting).

<p align="right">(<a href="#readme-top">back to top</a>)</p>