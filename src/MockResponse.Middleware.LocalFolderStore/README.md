<a id="readme-top"></a>
# MockResponse.Middleware.LocalFolderStore

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/github/license/BrandonSchreck/mockresponse-middleware)

[![CI/CD Pipeline](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/main.yml/badge.svg)](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/main.yml)
[![CodeQL](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/BrandonSchreck/mockresponse-middleware/actions/workflows/github-code-scanning/codeql)
[![NuGet - Local Folder Store](https://img.shields.io/nuget/v/MockResponse.Middleware.LocalFolderStore.svg)](https://www.nuget.org/packages/MockResponse.Middleware.LocalFolderStore/)

This package provides a file system-based implementation for use with the `MockResponse.Middleware` system. It enables serving mock API responses from a local folder, ideal for testing or offline development scenarios where remote providers like Azure Blob Storage are not feasible.


## Table of Contents
* [Installation](#-installation)
* [Configuration](#️-configuration)
* [LocalFolderStoreOptions Configuration](#-localfolderstoreoptions-configuration)
* [Usage: AddLocalFolderStore](#-usage-addlocalfolderstore)
* [Troubleshooting](#-troubleshooting)


## 📦 Installation

```bash
dotnet add package MockResponse.Middleware.LocalFolderStore
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## ⚙️ Configuration

Specify your local folder path and mock mappings in `appsettings.json`:

```json
{
  ...
  "MockOptions": {
    "ExcludedRequestPaths": ["/api/health", "/openapi", "/metrics", "/redoc", "/swagger"],
    "LocalFolderStoreOptions": {
      "FolderPath": "Mocks"
    },
    "ResponseMappings": {
      "Namespace.TypeName": "ExampleResponse.json",
      "Namespace.TypeName.Variant": "subfolder/ExampleResponse.Variant.json"
    },
    "UseMock": true
  },
  ...
}
```
> 💡 You can also organize your mock files using subfolders inside the specified `FolderPath`.
>
> For example, `"Namespace.TypeName.Variant": "subfolder/ExampleResponse.Variant.json"` will load the file from `Mocks/subfolder/ExampleResponse.Variant.json`.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🔧 LocalFolderStoreOptions Configuration

The following configuration property is used by the local folder provider. It should be set under `MockOptions:LocalFolderStoreOptions`.

| Property     | Type     | Required | Description |
| ---- | ---- | ---- | ---- |
| `FolderPath` | `string` | ✅ | Path to the folder containing mock JSON files. Relative paths are resolved from the application root. |

> 📁 The folder should contain JSON files matching your `ResponseMappings`. You can also organize files into subfolders for variants or grouping.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🚀 Usage: AddLocalFolderStore

Register the middleware in `Startup.cs` or `Program.cs`:

```csharp
services.AddApiMocking(Configuration)
	.AddLocalFolderStore();
```

Use the middleware:

```csharp
app.UseApiMocking();
```

Place your mock JSON files in the folder specified by `FolderPath`. Each file is served based on its `ResponseMappings` key and corresponding HTTP response metadata.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## 🧪 Troubleshooting

### Missing Configurations

#### ⚠️ Missing FolderPath

**Error Message**

The FolderPath field is required.

**Cause**

The `FolderPath` was not specified under `MockOptions:LocalFolderStoreOptions`, or it was empty.

**Solution**

Make sure your `appsettings.json` (or other configuration source) includes a valid path:

```json
{
  "MockOptions": {
    "LocalFolderStoreOptions": {
      "FolderPath": "Mocks"
    }
  }
}
```


### Runtime Errors

#### ⚠️ File not found

**Error Message**

Mock file was not found.

**Cause**

The file mapped in `ResponseMappings` does not exist in the specified folder (or subfolder).

**Solution**

Verify that the filename and subdirectory (if applicable) exist under the `FolderPath`. Be sure filenames match exactly (including casing on case-sensitive systems).

> 💡 Use relative paths like `"subfolder/Example.json"` for files inside subdirectories.


> 📘 For additional error handling scenarios, see the [Core Troubleshooting Guide](../MockResponse.Middleware.Core/README.md#-troubleshooting).

<p align="right">(<a href="#readme-top">back to top</a>)</p>