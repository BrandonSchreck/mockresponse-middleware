# MockResponse.Middleware Example - Local Folder Store

This example demonstrates how to integrate and use the [MockResponse.Middleware.LocalFolderStore](../../src/MockResponse.Middleware.LocalFolderStore/) provider to serve dynamic mock API responses from a local folder.

Mock responses are conditionally served based on HTTP headers, simplifying API testing and development workflows without modifying backend services.


## ⚙️ Project Configuration

### Application Settings Configuration

This project uses the following configuration (`appsettings.json`):

```json
{
  "MockOptions": {
    "ExcludedRequestPaths": [ "/api/health", "/openapi", "/metrics", "/redoc", "/swagger" ],
    "LocalFolderStoreOptions": {
      "FolderPath": "Mocks"
    },
    "ResponseMappings": {
      "MockResponse.Middleware.Example.LocalFolder.Models.WeatherForecast": "WeatherForecast.json",
      "MockResponse.Middleware.Example.LocalFolder.Models.WeatherForecast.Freezing": "WeatherForecast.Freezing.json"
    },
    "UseMock": true
  }
}
```


### 📁 Mock File Location

The mock response files are already placed in the `Mocks` folder within the project structure. You may also organize files into subfolders, as shown below:

* `Mocks/WeatherForecast.json`
* `subfolder/WeatherForecast.Freezing.json`

Example file contents:

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


## 🚀 Usage Examples

Run the ASP.NET Core project and send requests using Postman (or curl):

### Default Mock Response

#### Request:

```bash
GET https://localhost:<port>/weatherforecast
Headers:
  - X-Mock-Status: 200
```

#### Response:

```http
HTTP/1.1 200 OK
X-Mock-Identifier: WeatherForecast.json
X-Mock-Provider: LocalFolderStore
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

#### Response:

```http
HTTP/1.1 200 OK
X-Mock-Identifier: subfolder/WeatherForecast.Freezing.json
X-Mock-Provider: LocalFolderStore
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


### Unmapped Request

#### Request:

```bash
GET https://localhost:<port>/failure
Headers:
  - X-Mock-Status: 200
```

#### Response:

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