# MockResponse.Middleware Example - Azure Blob Storage

This example illustrates how to integrate and use the [MockResponse.Middleware.Azure.BlobStorage](../../src/MockResponse.Middleware.Azure.BlobStorage/) provider to dynamically serve mock API responses from an Azure Blob Storage container. 

Mock responses are conditionally served based on HTTP headers, simplifying API testing and development workflows without modifying backend services.


## ⚙️ Project Setup

### Step 1: Configure Azure Blob Storage Credentials

> **Security Tip:** Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0) to manage sensitive credentials securely instead of storing them directly in configuration files.

Initialize and configure User Secrets:
```bash
dotnet user-secrets init
dotnet user-secrets set "MockOptions:BlobStorageOptions:ConnectionString" "<your-connection-string>"
dotnet user-secrets set "MockOptions:BlobStorageOptions:ContainerName" "<your-container-name>"
```


### Step 2: Upload Mock Files to Azure Blob Storage

Ensure the following mock JSON files are uploaded to your Azure Blob Storage container: 

`WeatherForecast.json`
```json
{
  "city": "Fort Worth",
  "state": "Texas",
  "date": "2025-06-10T00:00:00Z",
  "temperatureC": 27,
  "summary": "Mostly Cloudy",
  "temperatureF": 80
}
```

`subfolder/WeatherForecast.Freezing.json`
```json
{
  "city": "Fort Worth",
  "state": "Texas",
  "date": "2025-02-20T12:53:00Z",
  "temperatureC": -11,
  "summary": "Mostly Clear",
  "temperatureF": 12
}
```

You can organize your mock files within virtual folders, leveraging Azure Blob Storage's directory-like naming conventions.


## 🚀 Usage Examples

Run the ASP.NET Core project and send requests as shown below.

### Default Mock Response

#### Request:

```bash
GET https://localhost:<port>/weatherforecast
Headers:
  - X-Mock-Status: 200
```

#### Expected Response:

```http
HTTP/1.1 200 OK
X-Mock-Identifier: WeatherForecast.json
X-Mock-Provider: AzureBlobStorage
```
```json
{
  "city": "Fort Worth",
  "state": "Texas",
  "date": "2025-06-10T00:00:00Z",
  "temperatureC": 27,
  "summary": "Mostly Cloudy",
  "temperatureF": 80
}
```


### Variant Mock Response (e.g. "Freezing")

#### Request:

```bash
GET https://localhost:<port>/weatherforecast
Headers:
  - X-Mock-Status: 200
  - X-Mock-Variant: Freezing
```

#### Expected Response:

```http
HTTP/1.1 200 OK
X-Mock-Identifier: subfolder/WeatherForecast.Freezing.json
X-Mock-Provider: AzureBlobStorage
```
```json
{
  "city": "Fort Worth",
  "state": "Texas",
  "date": "2025-02-20T12:53:00Z",
  "temperatureC": -11,
  "summary": "Mostly Clear",
  "temperatureF": 12
}
```


### Unmapped Requests

#### Request:

```bash
GET https://localhost:<port>/failure
Headers:
  - X-Mock-Status: 200
```

#### Expected Response:

```http
HTTP/1.1 404 Not Found
```
```json
No [200] status code mapping was found for endpoint [HTTP: GET /failure]
```


## 🔍 Understanding Mock Resolution

The middleware selects mock responses based on:

* **HTTP Status Code** (`X-Mock-Status`)
* **Optional Variant** (`X-Mock-Variant`)
* **Endpoint Metadata** (defined by `.Produces<T>()`)

For a detailed explanation of the mock resolution logic, **see the [Mock Resolution Strategy](../../src/MockResponse.Middleware.Core/README.md#-mock-resolution-strategy) section** of the Core middleware documentation.