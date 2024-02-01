//*********************************************************************************************
//* File             :   Startup.cs
//* Author           :   Rout, Sukanta 
//* Date             :   10/1/2024
//* Description      :   Initial version
//* Version          :   1.0
//*-------------------------------------------------------------------------------------------
//* dd-MMM-yyyy	: Version 1.x, Changed By : xxx
//*
//*                 - 1)
//*                 - 2)
//*                 - 3)
//*                 - 4)
//*
//*********************************************************************************************

using Sukanta.DataBus.Redis;
using Sukanta.Edge.RuleEngine.CommandControl;
using Sukanta.Edge.RuleEngine.Data;
using Sukanta.Edge.RuleEngine.Model;
using Sukanta.Edge.RuleEngine.NotificationHub;
using Sukanta.Edge.RuleEngine.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Sukanta.Edge.RuleEngine
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Notification hub path
        /// </summary>
        private const string NOTIFICATION_HUB_PATH = "/hubs/ruleengine/alert";

        /// <summary>
        /// Configurations
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Startup
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        ///This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //Configuration
            services.Configure<RuleEngineSettings>(Configuration.GetSection("RuleEngineSettings"));

            ConfigureDependencyForServices(services);

            //Cors
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition")
                    .SetIsOriginAllowed((hosts) => true));
            });

            //SignalR
            services.AddSignalR(config =>
            {
                config.EnableDetailedErrors = true;
            });

            //Controllers
            services.AddControllers();

            //Swaggerapi doc
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("V1", new OpenApiInfo
                {
                    Version = "V1.0",
                    Title = "Edge RuleEngine API V1.0",
                    Description = "API For RuleEngine"
                });
            });
        }

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<AlertNotificationHub>(NOTIFICATION_HUB_PATH);
            });

            //Swagger api doc
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/V1/swagger.json", "V1");
                options.RoutePrefix = "swagger";
            });
        }

        /// <summary>
        /// ConfigureServices(DI)
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureDependencyForServices(IServiceCollection services)
        {
            //Redis Pub-Sub DataBus subscriber
            services.AddSingleton<IRedisDataBusSubscriber>(serviceProvider =>
            {
                string redisServer = serviceProvider.GetService<IOptions<RuleEngineSettings>>().Value.DataBusSettings.Server;
                return new RedisDataBusSubscriber(redisServer);
            });

            //Redis Pub-Sub DataBus pulisher
            services.AddSingleton<IRedisDataBusPublisher>(serviceProvider =>
            {
                string redisServer = serviceProvider.GetService<IOptions<RuleEngineSettings>>().Value.DataBusSettings.Server;
                return new RedisDataBusPublisher(redisServer);
            });

            //Redis DB 
            services.AddSingleton(serviceProvider =>
            {
                string redisServer = serviceProvider.GetService<IOptions<RuleEngineSettings>>().Value.DataBusSettings.Server;
                return new RedisDataBus(redisServer);//pass connection string
            });

            //RuleDataProvider
            services.AddSingleton<IRuleDataProvider>(serviceProvider =>
            {
                RuleEngineSettings settings = serviceProvider.GetService<IOptions<RuleEngineSettings>>().Value;

                RedisDataBusSubscriber redisSubsciber = new RedisDataBusSubscriber(settings.DataBusSettings.Server, settings.DataBusSettings.SubscribeTopic);
                return new RuleDataProvider(redisSubsciber, settings);//pass connection string, topic
            });

            //AlertNotificationClient Hub
            services.AddSingleton(serviceProvider =>
            {
                RuleEngineSettings ruleEngineSettings = serviceProvider.GetService<IOptions<RuleEngineSettings>>().Value;
                return new AlertNotificationClient(ruleEngineSettings);
            });

            //Command Message
            services.AddSingleton(serviceProvider =>
            {
                RuleEngineSettings ruleEngineSettings = serviceProvider.GetService<IOptions<RuleEngineSettings>>().Value;
                RedisDataBusPublisher databusPublisher = new RedisDataBusPublisher(ruleEngineSettings.DataBusSettings.Server);

                return new ControlCommand(databusPublisher, ruleEngineSettings);
            });

            //Rule executer background service
            services.AddHostedService<RuleExecuterService>();
        }

    }
}
