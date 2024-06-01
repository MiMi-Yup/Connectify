using API.Atrributes;
using API.Core.Contracts;
using API.Core.Implements;
using API.Data;
using API.Entities;
using API.Errors;
using API.Profiles;
using API.SignalR;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;

namespace API.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCORS(this IServiceCollection services, IConfiguration configuration)
        {
            var corsOrigins = configuration.GetSection("OriginAllowed").Get<string[]>() ?? new string[] { };
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(corsOrigins)
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });
        }

        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ConnectifyDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });
        }

        public static void ConfigureRegister(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<LogUserActivityAttribute>();

            services.AddSingleton<PresenceTracker>();

            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
        }

        #region Swagger
        public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
        {
            private readonly IApiVersionDescriptionProvider _provider;
            private readonly IConfiguration _configuration;

            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration)
            {
                _provider = provider;
                _configuration = configuration;
            }

            /// <summary>
            /// Configure each API discovered for Swagger Documentation
            /// </summary>
            /// <param name="options"></param>
            public void Configure(SwaggerGenOptions options)
            {
                // add swagger document for every API version discovered
                foreach (var description in _provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(
                        description.GroupName,
                        CreateVersionInfo(description));
                }

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Place to add JWT with Bearer",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            }

            /// <summary>
            /// Configure Swagger Options. Inherited from the Interface
            /// </summary>
            /// <param name="name"></param>
            /// <param name="options"></param>
            public void Configure(string? name, SwaggerGenOptions options)
            {
                Configure(options);
            }

            /// <summary>
            /// Create information about the version of the API
            /// </summary>
            /// <param name="description"></param>
            /// <returns>Information about the API</returns>
            private OpenApiInfo CreateVersionInfo(ApiVersionDescription desc)
            {
                var info = new OpenApiInfo()
                {
                    Title = "API",
                    Version = desc.ApiVersion.ToString()
                };

                if (desc.IsDeprecated)
                {
                    info.Description += " This API version has been deprecated. Please use one of the new APIs available from the explorer.";
                }

                return info;
            }
        }

        public class AuthorizeCheckOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                // ignore endpoints that have the AllowAnonymousAttribute
                var hasAnonylousAttribute = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() == true
                    || context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

                if (!hasAnonylousAttribute)
                {
                    operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                    operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
                }
            }
        }
        #endregion

        #region API Versioning
        public static void ConfigureAPIVersioning(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(2, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Version"),
                    new MediaTypeApiVersionReader("version"),
                    new UrlSegmentApiVersionReader());
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
            });
        }
        #endregion

        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentityCore<AppUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
            })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleValidator<RoleValidator<AppRole>>()
            .AddEntityFrameworkStores<ConnectifyDbContext>();


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(op =>
            {
                op.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                op.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        public static void ConfigureSignalR(this IServiceCollection services)
        {
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.AddFilter<SignalRHubFilter>();
            });
        }
    }
}
