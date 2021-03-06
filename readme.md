# IdentityServer.MongoDbAdapter

[![Build status](https://ci.appveyor.com/api/projects/status/lmq51jpnkjklv09g?svg=true)](https://ci.appveyor.com/project/redplane/identityserver4-mongodbadapter)

**IdentityServer4.Extensions.MongoDbAdapter** is a small library which helps developers integrate [IdentityServer4](http://docs.identityserver.io/en/latest/) with [Mongo database](https://www.mongodb.com/).

## Installation:
- #### MyGet installation.
  - Following [this tutorial](https://docs.myget.org/docs/walkthrough/getting-started-with-nuget) and set Nuget package source to: [https://www.myget.org/F/identity-server-integration/api/v3/index.json](https://www.myget.org/F/identity-server-integration/api/v3/index.json).
  - Find package **IdentityServer4.MongoDbAdapter** and install.

- #### Nuget installation:
    - Not yet deployed to nuget.

### API(s):

-  `AddMongoDatabaseAdapter(string contextName, string clientCollectionName, string identityResourcesCollectionName, string apiResourcesCollectionName, string persistedGrantsCollectionName, DatabaseInitializer dbClientInitializer)`: Integrate mongo database with identity server in ASP.Net Core application.
    - `contextName`: Name of integration context. In case of multiple context implementation, developer can find **IAuthenticationMongoContext** by this name from DI frameworks.
    
    - `clientCollectionName`: Name of clients table (called collection in mongo database).
    
    - `identityResourcesCollectionName`: Name of identity resources collection in mongo database.
    
    - `apiResourcesCollectionName`: Name of api resources collection in mongo database.
    
    - `persistedGrantCollectionName`: Name of collection in mongo database to store user persisted grants. This will be applied when system uses **Reference token**.
    
    - `dbClientInitializer(IServiceProvider provider)`: A function that returns an instance of `IMongoClient` to initialize mongo database client. For example:
    ```
    var dbClient = new MongoClient(new MongoUrl(<connection string>));
    return dbClient.GetDatabase(<database name>);
    ```

- `AddIdentityServerMongoDbService<T>()`: Register an instance of `IAuthenticationMongoDatabaseService` which helps developers to initiate initial **clients**, **identity resources**, **api resources**.

- `UseInitialMongoDbAuthenticationItems()`: Generate initial items which have been defined in `IAuthenticationMongoDatabaseService` implemented by `AddIdentityServerMongoDbService<T>` above.

- `AddExpiredAccessTokenCleaner(string cronJob)`: Register a background task that runs along with the system to clean up expired **Persisted grants** that generated by user authentication.
    - `cronJob`: Cron job definition that can follows cron job syntax, please refer [this document]() for further information. By default, this value will be: `*/30 * * * *` (Every 30 minutes).

## Interfaces:
- `IAuthenticationMongoDatabaseService`: Define functions which return identity server initial items in mongo database.
    - `Task<List<Client>> LoadClientsAsync(CancellationToken cancellationToken = default)`: Returns a list of **clients** that will be stored into mongo database.
    
    - `Task<List<ApiResource>> LoadApiResourcesAsync(CancellationToken cancellationToken = default)`: Returns a list of **API Resources** that will be stored into mongo database.
    
    - `Task<List<IdentityResource>> LoadIdentityResourcesAsync(CancellationToken cancellationToken = default)`: Returns a list of **Identity Resources** that will be stored into mongo database.

## Usage:

- In **Startup.cs** file, implement:

```
public void ConfigureServices(IServiceCollection services)
{
    //...
    services
        .AddIdentityServer()
        .AddMongoDatabaseAdapter(DatabaseContextNameConstants.AuthenticationDbContext,
            identityServerSettings.ClientsCollectionName,
            identityServerSettings.IdentityResourcesCollectionName,
            identityServerSettings.ApiResourcesCollectionName,
            identityServerSettings.PersistedGrantsCollectionName,
            provider =>
            {
                var dbClient = new MongoClient(new MongoUrl(authenticationDbConnectionString));
                return dbClient.GetDatabase(identityServerSettings.DatabaseName);
            })
        .AddExpiredAccessTokenCleaner()
        .AddIdentityServerMongoDbService<AuthenticationDbService>()
        .AddProfileService<ProfileService>()
        .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
        .AddDeveloperSigningCredential();
        
    // Add jwt validation.
    services
        .AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddIdentityServerAuthentication(options =>
        {
            options.Authority = identityServerSettings.Authority;
            options.ApiSecret = identityServerSettings.ApiSecret;
            options.ApiName = "profile";
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.SupportedTokens = SupportedTokens.Reference;
        });
    //...
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    //...
    app.UseIdentityServer()
            .UseInitialMongoDbAuthenticationItems();

    app.UseAuthentication();
    app.UseAuthorization();
    //...
}
}
```

## Releases:
- **1.0.0-Preview-01**: Technical preview.

## Bugs and reports:
- If you find any issues while using this library, please report them at [https://github.com/redplane/IdentityServer4.MongoDbAdapter](https://github.com/redplane/IdentityServer4.MongoDbAdapter).
- Demo application is also provided with this repository, please find it [here](https://github.com/redplane/IdentityServer4.MongoDbAdapter/tree/master/IdentityServer4.MongoDbAdapter.Demo).
