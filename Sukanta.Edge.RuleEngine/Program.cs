//*********************************************************************************************
//* File             :   Program.cs
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

using Sukanta.Edge.RuleEngine.Common;
using Sukanta.Edge.RuleEngine.Model;
using Sukanta.LoggerLib;
using Sukanta.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Net;

namespace Sukanta.Edge.RuleEngine
{
    /// <summary>
    /// Start of program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            //Base directory setting     
            DirectoryInfo baseDirectory = Reflections.GetEntryAssemblyLocation();
            Directory.SetCurrentDirectory(baseDirectory.FullName);

            //Configure logger
            Log.Logger = LoggerHelper.ConfigureLogger();

            //Dislay framework info
            DisplayFrameworkInformation();

            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            //Configuration load
            IConfigurationRoot config = new ConfigurationBuilder()
                   .SetBasePath(Reflections.GetCurrentAssemblyLocation().FullName)
                   .AddJsonFile("RuleEngineSettings.json", optional: false, reloadOnChange: true)
                   .Build();

            IConfigurationSection appSettingsSection = config.GetSection("RuleEngineSettings");

            RuleEngineSettings appSettings = appSettingsSection.Get<RuleEngineSettings>();

            //Bind to localhost only, for security reasons
            string BindAddress = config["RuleEngineSettings:Binding"];

            //If nothing mentioned bind to localhost
            if (string.IsNullOrEmpty(BindAddress))
            {
                BindAddress = "localhost";
            }

            //Url
            string url = $"http://{BindAddress}:{config["RuleEngineSettings:HttpPort"]}";

            //Webhost
            return WebHost.CreateDefaultBuilder(args)
            .UseConfiguration(config)
            .UseKestrel(options =>
            {
                options.AddServerHeader = false;

                options.Limits.MaxRequestBodySize = null;

                if (appSettings.UseHttps)
                {
                    options.Listen(IPAddress.Any, appSettings.Port, listenOptions =>
                    {
                        listenOptions.UseHttps(appSettings.CertificateFile, DataEncrypter.Decrypt(appSettings.CertificatePassword));
                    });
                }

                if (appSettings.UseMutualTls)
                {
                    options.Listen(IPAddress.Any, appSettings.MutualTlsPort, listenOptions =>
                    {
                        listenOptions.UseHttps(appSettings.CertificateFile, DataEncrypter.Decrypt(appSettings.CertificatePassword), kerstelOptions =>
                        {
                            kerstelOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                            kerstelOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                        });
                    });
                }

                if (appSettings.UseHttp)
                {
                    options.Listen(IPAddress.Any, appSettings.HttpPort);
                }
            })
            .UseUrls(url)
            .UseStartup<Startup>();
        }

        /// <summary>
        /// Print RuleEngine Framework Information
        /// </summary>
        private static void DisplayFrameworkInformation()
        {
            LoggerHelper.Information("******************************************************************");
            LoggerHelper.Information($@"Sukanta Edge RuleEngine v{Reflections.GetAppVersion()}");
            LoggerHelper.Information($"Copyright (c) 2022-{DateTime.Today.Year} Mitsubishi Sukanta Truck and Bus Corporation");
            LoggerHelper.Information("******************************************************************");
        }
    }
}
