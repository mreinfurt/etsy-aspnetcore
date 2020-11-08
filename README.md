# etsy-aspnetcore
This project shows how to use the Etsy API in ASP.NET Core (C#).
It has been written as part of my medium article at https://manuelreinfurt.medium.com/use-etsy-api-with-oauth-1-0-in-asp-net-core-c-819fb6edd376.

# How to run
Simply clone the repository and call:
- dotnet restore
- dotnet run

# How to use
- Replace the placeholders in EtsyController.cs (ConsumerKey, ConsumerSecret, RequestUrl).
- Start the application
- Open https://localhost:5001/api/etsy/login
- Log into your etsy account
- Open https://localhost:5001/api/etsy/openorders

The last request will then show JSON containing all of your open orders in Etsy.
