using System.Reflection;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.MongoDbAdapter.Demo.AuthorizationHandlers;
using IdentityServer4.MongoDbAdapter.Demo.AuthorizationRequirements;
using IdentityServer4.MongoDbAdapter.Demo.Behaviors;
using IdentityServer4.MongoDbAdapter.Demo.Constants;
using IdentityServer4.MongoDbAdapter.Demo.Extensions;
using IdentityServer4.MongoDbAdapter.Demo.Models;
using IdentityServer4.MongoDbAdapter.Demo.Services.Implementations;
using IdentityServer4.MongoDbAdapter.Demo.Services.Interfaces;
using IdentityServer4.MongoDbAdapter.Extensions;
using IdentityServer4.MongoDbAdapter.Setups;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IdentityServer4.MongoDbAdapter.Demo
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
                var emailIndex = new CreateIndexModel<User>(userIndexesBuilder.Ascending(user => user.Username), uniqueIndexOptions);
                users
                    .Indexes
                    .CreateOne(emailIndex);

                return users;

            });

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
                .AddIdentityServerMongoDb(DatabaseContextNameConstants.AuthenticationDbContext,
                    identityServerSettings.ClientsCollectionName, identityServerSettings.IdentityResourcesCollectionName,
                    identityServerSettings.ApiResourcesCollectionName, identityServerSettings.PersistedGrantsCollectionName,
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

            // Add jwt validation.
            services
                .AddMvc(options =>
                {
                    // only allow authenticated users
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes(IdentityServerAuthenticationDefaults.AuthenticationScheme)
#if !ALLOW_ANONYMOUS
                        .AddRequirements(new SolidUserRequirement())
#endif
                        .Build();

                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddJsonOptions(options =>
                {
                    var camelCasePropertyNamesContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ContractResolver = camelCasePropertyNamesContractResolver;
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseHttpsRedirection();
            app.UseMvc();
        }

        #endregion
    }
}