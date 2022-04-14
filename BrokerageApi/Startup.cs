using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using BrokerageApi.V1.Controllers;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase;
using BrokerageApi.V1.UseCase.Interfaces;
using BrokerageApi.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BrokerageApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            AWSSDKHandler.RegisterXRayForAllServices();
        }

        public IConfiguration Configuration { get; }
        private static List<ApiVersionDescription> _apiVersions { get; set; }
        private const string ApiName = "ASC Brokerage";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCors()
                .AddMvc()
                .AddNewtonsoftJson(o => o.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true; // assume that the caller wants the default version if they don't specify
                o.ApiVersionReader = new UrlSegmentApiVersionReader(); // read the version number from the url segment header)
            });

            services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();

            services.AddSwaggerGen(c =>
            {
                var scheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Your Hackney Token",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                var requirement = new OpenApiSecurityRequirement
                {
                    { scheme, Array.Empty<string>() }
                };

                c.AddSecurityDefinition(scheme.Reference.Id, scheme);
                c.AddSecurityRequirement(requirement);

                //Looks at the APIVersionAttribute [ApiVersion("x")] on controllers and decides whether or not
                //to include it in that version of the swagger document
                //Controllers must have this [ApiVersion("x")] to be included in swagger documentation!!
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    apiDesc.TryGetMethodInfo(out var methodInfo);

                    var versions = methodInfo?
                        .DeclaringType?.GetCustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions).ToList();

                    return versions?.Any(v => $"{v.GetFormattedApiVersion()}" == docName) ?? false;
                });

                //Get every ApiVersion attribute specified and create swagger docs for them
                foreach (var apiVersion in _apiVersions)
                {
                    var version = $"v{apiVersion.ApiVersion.ToString()}";
                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = $"{ApiName} API {version}",
                        Version = version,
                        Description = $"{ApiName} version {version}. Please check older versions for depreciated endpoints."
                    });
                }

                c.CustomSchemaIds(x => x.Name);

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });

            // Add JWT bearer token authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
                {
                    x.IncludeErrorDetails = true;
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;

                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.Email,
                        ValidIssuers = new List<string>() { "Hackney" },
                        ValidateAudience = false,
                        RequireAudience = false,
                        RequireExpirationTime = false,
                        SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                        {
                            return new JwtSecurityToken(token);
                        }
                    };
                });

            services.AddSwaggerGenNewtonsoftSupport();

            ConfigureLogging(services, Configuration);

            ConfigureDbContext(services);

            RegisterGateways(services);
            RegisterUseCases(services);

            services.AddTransient<IClaimsTransformation, BrokerageClaimsTransformer>();
            services.AddTransient<IDbSaver, DbSaver>();
        }

        private void ConfigureDbContext(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                                ?? Configuration.GetValue<string>("DatabaseConnectionString");

            services.AddDbContext<BrokerageContext>(
                opt => opt
                    .UseNpgsql(connectionString)
                    .UseSnakeCaseNamingConvention()
            );
        }

        private static void ConfigureLogging(IServiceCollection services, IConfiguration configuration)
        {
            // We rebuild the logging stack so as to ensure the console logger is not used in production.
            // See here: https://weblog.west-wind.com/posts/2018/Dec/31/Dont-let-ASPNET-Core-Default-Console-Logging-Slow-your-App-down
            services.AddLogging(config =>
            {
                // clear out default configuration
                config.ClearProviders();

                config.AddConfiguration(configuration.GetSection("Logging"));
                config.AddDebug();
                config.AddEventSourceLogger();

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
                {
                    config.AddConsole();
                }
            });
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.AddScoped<IReferralGateway, ReferralGateway>();
            services.AddScoped<IServiceGateway, ServiceGateway>();
            services.AddScoped<IUserGateway, UserGateway>();
        }

        private static void RegisterUseCases(IServiceCollection services)
        {
            services.AddTransient<ICreateReferralUseCase, CreateReferralUseCase>();
            services.AddTransient<IGetAssignedReferralsUseCase, GetAssignedReferralsUseCase>();
            services.AddTransient<IGetCurrentReferralsUseCase, GetCurrentReferralsUseCase>();
            services.AddTransient<IGetReferralByIdUseCase, GetReferralByIdUseCase>();
            services.AddTransient<IGetAllServicesUseCase, GetAllServicesUseCase>();
            services.AddTransient<IGetAllUsersUseCase, GetAllUsersUseCase>();
            services.AddTransient<IAssignBrokerToReferralUseCase, AssignBrokerToReferralUseCase>();
            services.AddTransient<IReassignBrokerToReferralUseCase, ReassignBrokerToReferralUseCase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCorrelation();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseXRay("brokerage-api");

            // Get All ApiVersions,
            var api = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
            _apiVersions = api.ApiVersionDescriptions.ToList();

            // Swagger ui to view the swagger.json file
            app.UseSwaggerUI(c =>
            {
                foreach (var apiVersionDescription in _apiVersions)
                {
                    //Create a swagger endpoint for each swagger version
                    c.SwaggerEndpoint($"{apiVersionDescription.GetFormattedApiVersion()}/swagger.json",
                        $"{ApiName} API {apiVersionDescription.GetFormattedApiVersion()}");
                }
            });
            app.UseSwagger();
            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // SwaggerGen won't find controllers that are routed via this technique.
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
