using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using FluentValidation.AspNetCore;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Redplane.IdentityServer4.MongoDatabase.Demo.AuthorizationHandlers;
using Redplane.IdentityServer4.MongoDatabase.Demo.AuthorizationRequirements;
using Redplane.IdentityServer4.MongoDatabase.Demo.Behaviors;
using Redplane.IdentityServer4.MongoDatabase.Demo.Constants;
using Redplane.IdentityServer4.MongoDatabase.Demo.Extensions;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities;
using Redplane.IdentityServer4.MongoDatabase.Demo.Services.Implementations;
using Redplane.IdentityServer4.MongoDatabase.Demo.Services.Implementations.Builders;
using Redplane.IdentityServer4.MongoDatabase.Demo.Services.Interfaces;
using Redplane.IdentityServer4.MongoDatabase.Extensions;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Builders;

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
				Configuration.GetConnectionString(ConnectionStringKeys.AuthenticationDatabase);

			services.AddScoped(options =>
			{
				var dbClient = new MongoClient(authenticationDbConnectionString);
				return dbClient.GetDatabase(DatabaseContextNames.Authentication);
			});

			if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
			{
				BsonClassMap.RegisterClassMap<User>(options =>
				{
					options.AutoMap();
					options.SetIgnoreExtraElements(true);
					options.MapCreator(x => new User(x.Id, x.Username));
					options.SetIgnoreExtraElementsIsInherited(true);
				});
			}

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

			// Add authorization middleware.
			services.AddAuthorization();

			// Add authentication data builder.
			services.AddScoped<IAuthenticationDataBuilder, ApiResourceDataBuilder>();
			services.AddScoped<IAuthenticationDataBuilder, ApiScopeDataBuilder>();
			services.AddScoped<IAuthenticationDataBuilder, ClientDataBuilder>();
			services.AddScoped<IAuthenticationDataBuilder, IdentityResourceDataBuilder>();

			services.AddScoped<IAuthenticationDataBuilder, UserDataBuilder>();

			// Authentication entity resolver registration.
			services
				.AddIdentityServer()
				.AddMongoDatabaseAdapter(provider =>
					{
						// Get connection settings.
						var authenticationDatabaseUrl = Configuration.GetConnectionString(ConnectionStringKeys.AuthenticationDatabase);
						var mongoClient = new MongoClient(authenticationDatabaseUrl);
						var authenticationDatabaseContext = new AuthenticationDatabaseContext("default",
							mongoClient.GetDatabase(identityServerSettings.DatabaseName));

						return authenticationDatabaseContext;
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
					options.RequireHttpsMetadata = false;
					options.SaveToken = true;
					options.SupportedTokens = SupportedTokens.Both;
				});

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
		}

		/// <summary>
		///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		public void Configure(IApplicationBuilder app, IHostEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();

			IdentityModelEventSource.ShowPII = true;

			app.UseExceptionMiddleware(env);

			// Start identity server.
			app.UseIdentityServer()
				.BuildAuthenticationDatabaseRecords();

			app.UseAuthentication();

			// Use routing middleware.
			app.UseRouting();

			app.UseAuthorization();

			// Enable mvc pipeline.
			app
				.UseEndpoints(endpointBuilder => endpointBuilder.MapControllers());

		}

		#endregion
	}
}