#if NETCOREAPP3_0 || NETCOREAPP3_1
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
#else
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
#endif
using System.Reflection;
using FluentValidation.AspNetCore;
using IdentityServer4.AccessTokenValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Redplane.IdentityServer4.MongoDatabase.Demo.AuthorizationHandlers;
using Redplane.IdentityServer4.MongoDatabase.Demo.AuthorizationRequirements;
using Redplane.IdentityServer4.MongoDatabase.Demo.Behaviors;
using Redplane.IdentityServer4.MongoDatabase.Demo.Constants;
using Redplane.IdentityServer4.MongoDatabase.Demo.Extensions;
using Redplane.IdentityServer4.MongoDatabase.Demo.HostedServices;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models;
using Redplane.IdentityServer4.MongoDatabase.Demo.Services.Implementations;
using Redplane.IdentityServer4.MongoDatabase.Demo.Services.Interfaces;
using Redplane.IdentityServer4.MongoDatabase.Extensions;

namespace Redplane.IdentityServer4.MongoDatabase.Demo
{
    public class Startup
    {
        #region Constructor

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion

        #region Methods

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var authenticationDbConnectionString =
                Configuration.GetConnectionString(ConnectionStringNameConstants.DefaultAuthenticationDatabase);

            services.AddScoped(options =>
            {
                var dbClient = new MongoClient(authenticationDbConnectionString);
                return dbClient.GetDatabase(DatabaseContextNameConstants.AuthenticationDbContext);
            });

            services.AddScoped(options =>
            {
                var dbClient = options.GetService<IMongoDatabase>();
                var users = dbClient.GetCollection<User>(DbCollectionNameConstants.Users);

                var userIndexesBuilder = Builders<User>.IndexKeys;
                var uniqueIndexOptions = new CreateIndexOptions();
                uniqueIndexOptions.Unique = true;
                var emailIndex = new CreateIndexModel<User>(userIndexesBuilder.Ascending(user => user.Username),
                    uniqueIndexOptions);
                users
                    .Indexes
                    .CreateOne(emailIndex);

                return users;
            });

            services.AddHostedService<DummyHostedService>();
            services.AddScoped<IUserService, UserService>();

            // Add authorization handler.
            services.AddScoped(typeof(IAuthorizationHandler), typeof(SolidUserRequirementHandler));
            //services.AddScoped(typeof(IAuthorizationHandler), typeof(InRoleRequirementHandler));

            // Add mediator.
            services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);

            // Request validation.
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            // Get identity server 4 configuration.
            var identityServerSettings = new IdentityServerSettings();
            Configuration.GetSection(AppSettingKeyConstants.IdentityServer).Bind(identityServerSettings);

            services.AddAuthorization();

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
                .AddIdentityServerMongoDbService<AuthenticationDbService>().AddProfileService<ProfileService>()
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

#if NETCOREAPP3_0 || NETCOREAPP3_1
            services
                .AddControllers(options =>
                {
                    ////only allow authenticated users
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                        .AddRequirements(new SolidUserRequirement())
                        .Build();

                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddFluentValidation(options =>
                    options.RegisterValidatorsFromAssembly(typeof(Startup).Assembly))
                .AddNewtonsoftJson(options =>
                {
                    var camelCasePropertyNamesContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ContractResolver = camelCasePropertyNamesContractResolver;
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
#else
            // Add jwt validation.
            services
                .AddMvc(options =>
                {
                    // only allow authenticated users
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                        .AddRequirements(new SolidUserRequirement())
                        .Build();

                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddFluentValidation(options =>
                    options.RegisterValidatorsFromAssembly(typeof(Startup).Assembly))
                .AddJsonOptions(options =>
                {
                    var camelCasePropertyNamesContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ContractResolver = camelCasePropertyNamesContractResolver;
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
#endif
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
#if NETCOREAPP3_0 || NETCOREAPP3_1
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#else
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#endif
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            app.UseExceptionMiddleware(env);

            app.UseIdentityServer()
                .UseInitialMongoDbAuthenticationItems();

            app.UseAuthentication();

#if NETCOREAPP3_0 || NETCOREAPP3_1
            // Use routing middleware.
            app.UseRouting();

            // Enable mvc pipeline.
            app
                .UseEndpoints(endpointBuilder => endpointBuilder.MapControllers());
#else
            app.UseMvc();
#endif
            
        }

#endregion
    }
}