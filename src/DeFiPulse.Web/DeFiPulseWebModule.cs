using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using DeFiPulse.MongoDB;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace DeFiPulse.Web
{
    [DependsOn(
        typeof(AbpAspNetCoreMvcModule),
        typeof(DeFiPulseApplicationModule),
        typeof(DeFiPulseMongoDbModule),
        typeof(AbpAutofacModule)
        )]
    public class  DeFiPulseWebModule : AbpModule
    {
        private const string DefaultCorsPolicyName = "Default";

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            
            ConfigureAutoMapper();
            ConfigureAutoApiControllers();
            ConfigureCors(context, configuration);
            ConfigureSwaggerServices(context.Services);
            
            context.Services.AddLogging(builder =>
            {
                builder.AddConfiguration(context.Services.GetConfiguration().GetSection("Logging"));

                builder.AddLog4Net();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            
            Configure<AbpAntiForgeryOptions>(options =>
            {
                options.AutoValidate = false;
            });
        }
        
        private void ConfigureAutoMapper()
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<DeFiPulseWebModule>();
            });
        }

        private void ConfigureAutoApiControllers()
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(DeFiPulseApplicationModule).Assembly);
            });
        }

        private void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SendCoin API", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                    options.DocumentFilter<ApiOptionFilter>();
                }
            );
        }
        
        private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, builder =>
                {
                    builder
                        .WithOrigins(configuration["App:CorsOrigins"]
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray()
                        )
                        .WithAbpExposedHeaders()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    if (configuration["App:CorsOrigins"] != "*")
                    {
                        builder.AllowCredentials();
                    }
                });
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            
            app.UseRouting();
            app.UseCors(DefaultCorsPolicyName);
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SendCoin API");
            });
            //app.UseAuditing();
            app.UseConfiguredEndpoints();
        }
    }
}
