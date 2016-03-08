# Microsoft Graph .NET Client Library

Integrate the [Microsoft Graph API](https://graph.microsoft.io) into your .NET
project!

The Microsoft Graph .NET Client Library is built as a Portable Class Library targeting profile 111.
This targets the following frameworks:

* .NET 4.5
* .NET for Windows Store apps
* Windows Phone 8.1 and higher

## Installation via NuGet

To install the client library via NuGet:

* Search for `Microsoft.Graph` in the NuGet Library, or
* Type `Install-Package Microsoft.Graph` into the Package Manager Console.

## Getting started

### 1. Register your application

Register your application to use Microsoft Graph API using one of the following
supported authentication portals:

* [Microsoft Application Registration Portal](https://apps.dev.microsoft.com):
  Register a new application that works with Microsoft Account and/or
  organizational accounts using the unified V2 Authentication Endpoint.
* [Microsoft Azure Active Directory](https://manage.windowsazure.com): Register
  a new application in your tenant's Active Directory to support work or school
  users for your tenant or multiple tenants.
  
### 2. Authenticate for the Microsoft Graph service

The Microsoft Graph .NET Client Library does not include any default authentication implementations.
Instead, the user will want to authenticate with the library of their choice, or against the OAuth
endpoint directly, and built-in **DelegateAuthenticationProvider** class to authenticate each request.
For more information on `DelegateAuthenticationProvider`, see the [library overview](docs/overview)

The recommended library for authenticating against AAD is [ADAL](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet).

For an example of authenticating a UWP app using the V2 Authentication Endpoint, see the [Microsoft Graph UWP Connect Library](https://github.com/ginach/Microsoft-Graph-UWP-Connect-Library).

### 3. Create a Microsoft Graph client object with an authentication provider

An instance of the **GraphServiceClient** class handles building requests,
sending them to Microsoft Graph API, and processing the responses. To create a
new instance of this class, you need to provide an instance of
`IAuthenticationProvider` which can authenticate requests to Microsoft Graph.

For more information on initializing a client instance, see the [library overview](docs/overview)

### 4. Make requests to the graph

Once you have completed authentication and have a GraphServiceClient, you can
begin to make calls to the service. The requests in the SDK follow the format
of the Microsoft Graph API's RESTful syntax.

For example, to retrieve a user's default drive:

```csharp
var drive = await graphClient.Me.Drive.Request().GetAsync();
```

`GetAsync` will return a `Drive` object on success and throw a
`ServiceException` on error.

To get the current user's root folder of their default drive:

```csharp
var rootItem = await graphClient.Me.Drive.Root.Request().GetAsync();
```

`GetAsync` will return a `DriveItem` object on success and throw a
`ServiceException` on error.

For a general overview of how the SDK is designed, see [overview](docs/overview.md).

The following sample applications are also available:
* [Microsoft Graph UWP Connect Library](https://github.com/ginach/Microsoft-Graph-UWP-Connect-Library) - Windows Universal app

## Documentation and resources

* [Overview](docs/overview.md)
* [Auth](docs/auth.md)
* [Items](docs/items.md)
* [Collections](docs/collections.md)
* [Errors](docs/errors.md)
* [OneDrive API](http://dev.onedrive.com)

## Issues

To view or log issues, see [issues](https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues).

## Other resources

* NuGet Package: [https://www.nuget.org/packages/Microsoft.Graph](https://www.nuget.org/packages/Microsoft.Graph)


## License

Copyright (c) Microsoft Corporation. All Rights Reserved. Licensed under the MIT [license](LICENSE.txt)