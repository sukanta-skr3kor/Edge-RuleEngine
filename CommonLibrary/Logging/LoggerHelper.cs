//*********************************************************************************************
//* File             :   LoggerHelper.cs
//* Author           :   Rout, Sukanta 
//* Date             :   2/1/2024
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
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sukanta.LoggerLib
{
    /// <summary>
    /// Logger api
    /// </summary>
    public static class LoggerHelper
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        private static ILogger Logger;

        /// <summary>
        /// skip level
        /// </summary>
        private static int skip { get; set; }

        /// <summary>
        /// eventId
        /// </summary>
        private static int eventId { get; set; }

        /// <summary>
        /// ConfigLogger
        /// </summary>
        /// <returns></returns>
        public static Logger ConfigureLogger()
        {
            skip = 2;
            eventId = 1000;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))//For linux
                {
                    return new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                  .WriteTo.File(path: "Logs/AdapterService.Log",
                        rollingInterval: RollingInterval.Infinite,
                        rollOnFileSizeLimit: true,
                        fileSizeLimitBytes: 10_000_000,
                        retainedFileCountLimit: 31,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1),
                        outputTemplate: eventId > 0 ? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}"
                        : "{Timestamp:yyyy-MM-dd HH:mm:ss} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}\n"
                        )
                  .CreateLogger();
                }
                else//For windows
                {
                    return new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                            .CreateLogger();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// ConfigWatchDogLogger
        /// </summary>
        /// <returns></returns>
        public static Logger ConfigWatchDogLogger()
        {
            skip = 2;
            eventId = 1;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))//For linux
            {
                return new LoggerConfiguration()
              .MinimumLevel.Debug()
              .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
              .WriteTo.File(path: "Logs/WatchDog.Log",
                    rollingInterval: RollingInterval.Infinite,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10_000_000,
                    retainedFileCountLimit: 31,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    outputTemplate: eventId > 0 ? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}"
                    : "{Timestamp:yyyy-MM-dd HH:mm:ss} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}\n"
                    )

              .CreateLogger();
            }
            else//for windwos
            {
                return new LoggerConfiguration()
                .MinimumLevel.Debug()
                   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                  .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}\n")
                 .WriteTo.File(path: "Logs/WatchDog.Log",
            rollingInterval: RollingInterval.Infinite,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 10_000_000,
            retainedFileCountLimit: 31,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1),
            outputTemplate: eventId > 0 ? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}"
            : "{Timestamp:yyyy-MM-dd HH:mm:ss} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}\n")
                 .CreateLogger();
            }
        }


        /// <summary>
        /// Configure the logger withe the eventid of each module
        /// </summary>
        /// <param name="adaptername"></param>
        /// <param name="adapterPath"></param>
        /// <returns></returns>
        public static Logger ConfigLogger(string adaptername, string type)
        {
            skip = 4;

            //Log for linux and windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(path: $"Logs/{adaptername}" + ".log",
                    rollingInterval: RollingInterval.Infinite,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10_000_000,
                    retainedFileCountLimit: 31,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    outputTemplate: eventId > 0 ? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}"
                    : "{Timestamp:yyyy-MM-dd HH:mm:ss} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}\n"
                    )
                .CreateLogger();
            }
            else//for windows
            {
                return new LoggerConfiguration()
               .MinimumLevel.Debug()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.ColoredConsole(
                   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}\n"
                   )
               .WriteTo.File(path: $"Logs/{adaptername}" + ".log",
                   rollingInterval: RollingInterval.Infinite,
                   rollOnFileSizeLimit: true,
                   fileSizeLimitBytes: 10_000_000,
                   retainedFileCountLimit: 31,
                   shared: true,
                   flushToDiskInterval: TimeSpan.FromSeconds(1),
                   outputTemplate: eventId > 0 ? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}"
                   : "{Timestamp:yyyy-MM-dd HH:mm:ss} [{EventId}] [{Level}] [{SourceContext}.{Method}] {Message}{NewLine}{Exception}\n"
                   )
               .CreateLogger();
            }
        }

        /// <summary>
        /// Initialize the library
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Initialize()
        {
            try
            {
                MethodBase methodBase = new StackFrame(skip, false).GetMethod();

                if (methodBase?.DeclaringType != null && (methodBase.DeclaringType.Name.Contains("<") || methodBase.DeclaringType.Name.Contains(">")
                    || methodBase.Name.Contains("<") || methodBase.Name.Contains(">")))
                {
                    skip = skip == 2 ? 4 : 2;
                    methodBase = new StackFrame(skip, false).GetMethod();
                }

                if (methodBase?.DeclaringType != null && methodBase.DeclaringType.FullName.ToLower().Contains("sukanta"))
                {
                    string MethodName = methodBase.Name;

                    if (eventId > 0)
                    {
                        Logger = Log.ForContext(Serilog.Core.Constants.SourceContextPropertyName, methodBase.DeclaringType.Name)
                                      .ForContext(LogMessage.METHOD_CONTEXT, MethodName == ".ctor" ? "Contructor" : MethodName).ForContext("EventId", eventId);
                    }
                    else
                    {
                        Logger = Log.ForContext(Serilog.Core.Constants.SourceContextPropertyName, methodBase.DeclaringType.Name)
                                      .ForContext(LogMessage.METHOD_CONTEXT, MethodName == ".ctor" ? "Contructor" : MethodName);
                    }
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Log Information
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Information(string message)
        {
            try
            {
                Initialize();
                Logger.Information(string.Format(LogMessage.LOG_INFO_MSG, message));
            }
            catch
            { }
        }

        /// <summary>
        /// Log Error
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(string message)
        {
            try
            {
                Initialize();
                Logger.Error(message);
            }
            catch
            { }
        }

        /// <summary>
        /// Log Error
        /// </summary>
        /// <param name="exp"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(Exception exp)
        {
            try
            {
                Initialize();
                Logger.Error(exp, LogMessage.ERROR_MSG);
            }
            catch
            { }
        }

        /// <summary>
        /// Log Error
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Error(Exception exp, string message)
        {
            try
            {
                Initialize();
                Logger.Error(exp, string.Format(LogMessage.ERROR_WITH_MSG, message));
            }
            catch
            { }
        }

        /// <summary>
        /// Log Warning
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Warning(string message)
        {
            try
            {
                Initialize();
                Logger.Warning(string.Format(LogMessage.WARNING_MSG, message));
            }
            catch
            { }
        }
    }
}
