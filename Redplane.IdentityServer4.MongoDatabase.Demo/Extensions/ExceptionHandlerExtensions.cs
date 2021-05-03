using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models.Exceptions;
using Redplane.IdentityServer4.MongoDatabase.Demo.ViewModels;
using Microsoft.Extensions.Hosting;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Extensions
{
    public static class ExceptionHandlerExtension
    {
        public static void UseExceptionMiddleware(this IApplicationBuilder app, IHostEnvironment env)
        {
            // Use exception handler for errors handling.
            app.UseExceptionHandler(options =>
            {
                options.Run(
                    async context =>
                    {
                        // Get logger.

                        // Mark the response status as Internal server error.
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "application/json";
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                        // No exception handler feature has been found.
                        if (exceptionHandlerFeature == null || exceptionHandlerFeature.Error == null)
                            return;

                        // Initialize response asynchronously.
                        var contractResolver = new DefaultContractResolver();
                        contractResolver.NamingStrategy = new CamelCaseNamingStrategy
                        {
                            ProcessDictionaryKeys = true
                        };

                        var jsonSerializerSettings = new JsonSerializerSettings();
                        jsonSerializerSettings.ContractResolver = contractResolver;
                        jsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

                        // Get the thrown exception.
                        var exception = exceptionHandlerFeature.Error;

                        // Initialize api response.
                        string szApiResponse;
                        var httpFailureResponse = new HttpFailureResponseViewModel("internal_server_error");

                        if (exception is HttpResponseException httpResponseException)
                        {
                            httpFailureResponse = new HttpFailureResponseViewModel(httpResponseException.Message, httpResponseException.MessageCode);
                            httpFailureResponse.AdditionalData = httpResponseException.AdditionalData;
                            szApiResponse = JsonConvert.SerializeObject(httpFailureResponse, jsonSerializerSettings);
                            context.Response.StatusCode = (int)httpResponseException.StatusCode;
                        }

                        szApiResponse = JsonConvert.SerializeObject(httpFailureResponse, jsonSerializerSettings);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;


                        await context.Response.WriteAsync(szApiResponse).ConfigureAwait(false);
                    });
            });
        }
    }
}