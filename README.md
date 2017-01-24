# gilded-rose
## The Gilded Rose Expands
As you may know, the Gilded Rose* is a small inn in a prominent city that buys and sells
only the finest items. The shopkeeper, Allison, is looking to expand by providing
merchants in other cities with access to the shop's inventory via a public HTTP-accessible
API

### Web API action requirement
* Retrieve the current inventory, i.e. list of products with their respective info and stock count 
* Retrieve the inventory item with product info and stock count
* Buy a product. User must be authenticated

This projects implements just that. 

This project features and demonstrates:
* async calls in Web API controller in ASP.NET Core MVC
* dependency injection in ASP.NET Core
* Web API controller unit testing with mocked dependencies in .NET Core
* Web API controller integration testing in ASP.NET Core
* Code documentation

## Frameworks and technologies choices
* **ASP.NET Core with MVC** - Web API service framework
* **Token Based Authentication** - authentication mechanism
* **NSubstitute** - mocking library

### Why ASP.NET Core?
* Integration of modern client-side frameworks and development workflows
* Built-in dependency injection
* New light-weight and modular HTTP request pipeline
* Ability to host on IIS or self-host in your own process
* Ships entirely as NuGet packages
* Build and run cross-platform ASP.NET apps on Windows, Mac and Linux
* Open source and community focused

### Why Token Based Authentication? 
* Token based authentication is the best solution for both web clients and mobile apps
* Simple to use

With the token based authentication, the client acquires a string based encrypted token after a successful log-in. The token also contains some user info and token expiry time. This token must be stored by the client and submitted to the server upon authorized resource request. HTTP Authorization header is used to submit the token. When token expires client needs to re-log-in.

In the Web API application project I used SimpleTokenProvider open source project, a very simple authentication solution. It accepts user credentials and returns the token. I would not use it in the enterprise application. For that IdentiyServer or Azure AD would be a better choice.

For demonstration purposes Web API supports only one hardcoded user: 
```
username=user password=123
```

### Why NSubstitute
* It is simple, succinct and pleasant to use

## Trying it out

### Prerequisites
* **Important!** .NET Core 1.1 runtime. https://go.microsoft.com/fwlink/?LinkID=835009
* Visual Studio 2015 or 2017 with GitHub extensions and Web Tools
* **Important!** If using VS2015: Visual Studio 2015 Update 3 https://go.microsoft.com/fwlink/?LinkId=691129
* **Important!** If using VS2015: .NET Core 1.0.1 tools Preview 2 https://go.microsoft.com/fwlink/?LinkID=827546
* Windows 8 x64, Windows 8.1 x64, Windows 10 x64 

If you get compilation error run following in Visual Studio in Package Manager window: 
```
Install-Package Microsoft.NETCore.Runtime -Version 1.0.1
```

You have two options to rung this Web API service project, IIS Express or Program.cs in a console app. In IDE you can choose it from the run drop down.

Both inventory listing and individual item info requests do not require authentication.
Buy product request requires authantiation. Before you execute buy product request you must obtain authorization token by running get token request with username=user and password=123 entries in x-www-form-urlencoded body. Copy return access_token value and paste it into Autherization Header of the buy product request: 
```
Autorization: Bearer [access_token] // without brackets
```

#### Get list of inventory
![Get list of inventory](/../snapshots/list-inventory.png?raw=true "Postman - List Inventory HTTP request")

#### Get inventory item by product ID
![Alt text](/../snapshots/get-inventory-item.png?raw=true "Postman - Get Inventory Item HTTP request")

#### Get authorization token
![Alt text](/../snapshots/get-token.png?raw=true "Postman - Get Token HTTP request")

#### Buy one product
![Alt text](/../snapshots/buy-one.png?raw=true "Postman - Buy one product HTTP request")

#### Buy two products
![Alt text](/../snapshots/buy-many.png?raw=true "Postman - Buy two products HTTP request")
