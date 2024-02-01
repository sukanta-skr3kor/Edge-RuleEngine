//*********************************************************************************************
//* File             :   RuleExecuterService.cs
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

using Sukanta.DataBus.Abstraction;
using Sukanta.DataBus.Redis;
using Sukanta.Edge.RuleEngine.CommandControl;
using Sukanta.Edge.RuleEngine.Core;
using Sukanta.Edge.RuleEngine.Data;
using Sukanta.Edge.RuleEngine.Model;
using Sukanta.Edge.RuleEngine.NotificationHub;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Sukanta.Edge.RuleEngine.Services
{
    /// <summary>
    /// RuleExecuter Background Service
    /// </summary>
    public class RuleExecuterService : BackgroundService
    {
        /// <summary>
        /// SignalR Client
        /// </summary>
        private AlertNotificationClient _alertNotifiationClient;

        /// <summary>
        /// RuleDataProvider
        /// </summary>
        private IRuleDataProvider _ruleDataProvider;

        /// <summary>
        /// RedisDataBusSubscriber 
        /// </summary>
        private IRedisDataBusSubscriber _redisDataBusSubscriber;

        /// <summary>
        /// Single paramter RuleExecuter
        /// </summary>
        private RuleExecuter _ruleExecuter;

        /// <summary>
        /// MultiParameter RuleExecuter 
        /// </summary>
        private MultiParameterRuleExecuter _multiParameterRuleExecuter;

        /// <summary>
        /// RedisDataBus
        /// </summary>
        private RedisDataBus _redisDataBus;

        /// <summary>
        /// Rule engine configuration settings
        /// </summary>
        private RuleEngineSettings _ruleEngineSettings { get; set; }

        /// <summary>
        /// ControlCommand
        /// </summary>
        private ControlCommand _controlCommand;

        /// <summary>
        /// RuleExecuter background service
        /// </summary>
        /// <param name="ruleDataProvider"></param>
        /// <param name="redisDataBusSubscriber"></param>
        /// <param name="redisDataBus"></param>
        /// <param name="controlCommand"></param>
        /// <param name="alertNotificationClient"></param>
        /// <param name="settings"></param>
        public RuleExecuterService(IRuleDataProvider ruleDataProvider, IRedisDataBusSubscriber redisDataBusSubscriber, RedisDataBus redisDataBus,
                     ControlCommand controlCommand, AlertNotificationClient alertNotificationClient, IOptions<RuleEngineSettings> settings)
        {
            _ruleEngineSettings = settings.Value;
            _redisDataBusSubscriber = redisDataBusSubscriber;
            _redisDataBus = redisDataBus;
            _ruleDataProvider = ruleDataProvider;
            _alertNotifiationClient = alertNotificationClient;
            _controlCommand = controlCommand;

            _ruleExecuter = new RuleExecuter(_ruleDataProvider, _redisDataBus, _controlCommand, _alertNotifiationClient, _ruleEngineSettings);

            _multiParameterRuleExecuter = new MultiParameterRuleExecuter(_ruleDataProvider, _redisDataBus, _alertNotifiationClient, _ruleEngineSettings);

            if (_redisDataBusSubscriber.IsConnected)
            {
                _redisDataBusSubscriber.SubscribeToDataBusAsync<DataBusMessage>(_ruleEngineSettings.DataBusSettings.SubscribeTopic);
            }
            else
            {
                LoggerLib.LoggerHelper.Error("Connection to DataBus failed!!!");
            }
        }

        /// <summary>
        /// ExecuteAsync
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_ruleEngineSettings.SimpleRule.Enabled)
            {
                _ruleExecuter?.StartExecution();
            }
            else
            {
                LoggerLib.LoggerHelper.Information("Simple ParameterAnalysis Rule Execution is disabled.");
            }

            if (_ruleEngineSettings.ComplexRule.Enabled)
            {
                _multiParameterRuleExecuter?.StartExecution();
            }
            else
            {
                LoggerLib.LoggerHelper.Information("Complex ParameterAnalysis Rule Execution is disabled.");
            }

            await Task.Delay(100);
        }

        /// <summary>
        /// Start Async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_ruleEngineSettings.SimpleRule.Enabled)
            {
                _ruleExecuter?.StartExecution();
            }
            else
            {
                LoggerLib.LoggerHelper.Information("Simple ParameterAnalysis Rule Execution is disabled.");
            }

            if (_ruleEngineSettings.ComplexRule.Enabled)
            {
                _multiParameterRuleExecuter?.StartExecution();
            }
            else
            {
                LoggerLib.LoggerHelper.Information("Complex ParameterAnalysis Rule Execution is disabled.");
            }

            await Task.Delay(1000);
        }

        /// <summary>
        /// Stop Async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_ruleEngineSettings.SimpleRule.Enabled)
            {
                _ruleExecuter?.Stop();
            }

            if (_ruleEngineSettings.ComplexRule.Enabled)
            {
                _multiParameterRuleExecuter?.Stop();
            }

            LoggerLib.LoggerHelper.Information("RuleEngine stopped");

            await Task.Delay(1000);
        }

    }
}
