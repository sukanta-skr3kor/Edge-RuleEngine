//*********************************************************************************************
//* File             :   ControlCommand.cs
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
using Sukanta.Edge.RuleEngine.Model;
using System.Threading.Tasks;

namespace Sukanta.Edge.RuleEngine.CommandControl
{
    /// <summary>
    /// Command messages for machine and other apps
    /// </summary>
    public class ControlCommand : IControlCommand
    {
        /// <summary>
        /// RedisDataBusPublisher 
        /// </summary>
        private IRedisDataBusPublisher _redisDataBusPublisher;

        /// <summary>
        /// Rule engine configuration settings
        /// </summary>
        private RuleEngineSettings _ruleEngineSettings { get; set; }

        /// <summary>
        /// Command
        /// </summary>
        /// <param name="redisDataBusPublisher"></param>
        /// <param name="settings"></param>
        public ControlCommand(IRedisDataBusPublisher redisDataBusPublisher, RuleEngineSettings settings)
        {
            _ruleEngineSettings = settings;
            _redisDataBusPublisher = redisDataBusPublisher;
        }

        /// <summary>
        /// SendCommand
        /// </summary>
        /// <param name="ParameterId"></param>
        /// <param name="MachineId"></param>
        /// <param name="commandMessage"></param>
        /// <param name="IsMachineCommand"></param>
        /// <returns></returns>
        public async Task SendCommandAsync(CommandMessage commandMessage)
        {
            if (_ruleEngineSettings.DataBusSettings.CommandMessageEnabled)
            {
                await _redisDataBusPublisher.PublishCommandMessageAsync(commandMessage, _ruleEngineSettings.DataBusSettings.PublishTopic).ConfigureAwait(false); ;
            }
        }
    }
}
