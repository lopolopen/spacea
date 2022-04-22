using SpaceA.Repository.Context;
using SpaceA.WebApi.Hubs;
using SpaceA.WebApi.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SpaceA.WebApi.Security;

namespace SpaceA.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;
            services.AddDbContext<SpaceAContext>(options => options.UseMySql(
                Configuration["ConnectionStrings:DefaultConnection"],
                b => b.MigrationsAssembly("SpaceA.WebApi")));
            services.AddControllers(options =>
            {
                var authenticatedUserPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(authenticatedUserPolicy));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });
            var secretBytes = Encoding.ASCII.GetBytes(Configuration["Token:Secret"]);
            services
                .AddAuthentication("Multiple")
                .AddPolicyScheme("Multiple", "Multiple Auth Scheme", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (authHeader != null && authHeader.StartsWith(ApikeyDefaults.AuthenticationScheme))
                        {
                            return ApikeyDefaults.AuthenticationScheme;
                        }
                        return JwtBearerDefaults.AuthenticationScheme;
                    };
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretBytes),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddApikey();

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SpaceA API",
                    Version = "v1.x",
                });
                config.CustomSchemaIds(t => t.FullName);
                config.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Description = "JWT Authorization header using the Bearer scheme.",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        In = ParameterLocation.Header
                    });
                config.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme,
                                }
                            },
                            new string[]{ }
                        }
                    });
            });

            services.AddCors(options =>
            {
                var cors = Configuration.GetSection("Cors").Get<CorsConfig>();
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .WithOrigins(cors.Origins)
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Token-Expired")
                        .AllowCredentials());
            });

            services.Configure<MinioOptions>(Configuration.GetSection(MinioOptions.PREFIX));

            services.AddDataProtection();

            services.AddSignalR();
            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddCustomServices();
            services.AddJobSchedule();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            // app.UseStaticFiles();
            // app.UseHttpsRedirection();
            // app.Use(async (context,next)=>{
            // })

            //var option = new RewriteOptions();
            //option.AddRedirect("^/?$", "swagger");
            //app.UseRewriter(option);

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<WebhookHub>("/webhookhub");
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/swagger/v1/swagger.json", $"SpaceA.WebApi v1.x");
            });
        }
    }

    class CorsConfig
    {
        public string[] Origins { get; set; }
    }
}
