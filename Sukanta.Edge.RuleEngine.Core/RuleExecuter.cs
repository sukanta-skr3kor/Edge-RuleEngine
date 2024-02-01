//*********************************************************************************************
//* File             :   RuleExecuter.cs
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
using Sukanta.Edge.RuleEngine.Data;
using Sukanta.Edge.RuleEngine.Model;
using Sukanta.Edge.RuleEngine.NotificationHub;
using Sukanta.LoggerLib;
using Newtonsoft.Json;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace Sukanta.Edge.RuleEngine.Core
{
    /// <summary>
    /// Simple RuleExecuter
    /// </summary>
    public class RuleExecuter
    {
        private Task RuleExecutionTask;
        private CancellationTokenSource _ruleExecutionCts;
        private readonly CancellationToken _shutdownToken;

        /// <summary>
        /// SignalR Client
        /// </summary>
        private AlertNotificationClient _notifcationClient;

        /// <summary>
        /// RuleDataProvider 
        /// </summary>
        private IRuleDataProvider _ruleDataProvider;

        /// <summary>
        /// RedisDataBus
        /// </summary>
        private RedisDataBus _redisDataBus;

        /// <summary>
        /// ControlCommand
        /// </summary>
        private IControlCommand _controlCommand;

        /// <summary>
        /// Work flow data
        /// </summary>
        private List<Workflow> _workflows;

        /// <summary>
        /// Rule engine configuration settings
        /// </summary>
        private RuleEngineSettings _ruleEngineSettings { get; set; }

        /// <summary>
        /// Simple RuleExecuter
        /// </summary>
        /// <param name="ruleDataProvider"></param>
        /// <param name="redisDataBus"></param>
        /// <param name="controlCommand"></param>
        /// <param name="alertNotificationClient"></param>
        /// <param name="settings"></param>
        public RuleExecuter(IRuleDataProvider ruleDataProvider, RedisDataBus redisDataBus, ControlCommand controlCommand,
            AlertNotificationClient alertNotificationClient, RuleEngineSettings settings)
        {
            _ruleEngineSettings = settings;
            _ruleDataProvider = ruleDataProvider;
            _redisDataBus = redisDataBus;
            _notifcationClient = alertNotificationClient;
            _controlCommand = controlCommand;

            _ruleExecutionCts = new CancellationTokenSource();
            _shutdownToken = _ruleExecutionCts.Token;
        }

        /// <summary>
        /// InitializeWorkflow
        /// </summary>
        /// <returns></returns>
        private bool InitializeWorkflow()
        {
            try
            {
                LoggerHelper.Information("Loading single parameter Workflow into RuleEngine...");

                string[] files = Directory.GetFiles(Path.Combine(_ruleEngineSettings.SimpleRule.RuleFilePath), "rule.json", SearchOption.AllDirectories);

                if (files == null || files.Length == 0)
                {
                    throw new Exception("Rules not found.");
                }

                string fileData = File.ReadAllText(files[0]);

                _workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData);

                LoggerHelper.Information("single parameter rule Workflow initialized");

                return true;
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, $"Simple rule Workflow configuration error");
            }
            return false;
        }

        /// <summary>
        /// start rule task
        /// </summary>
        public void StartExecution()
        {
            LoggerHelper.Information("Starting Simple Rule Executer...");

            if (InitializeWorkflow())
            {
                RuleExecutionTask = Task.Run(() => ExecuteRule(_shutdownToken), _shutdownToken);
                LoggerHelper.Information("Simple Rule Executer Started");
            }
            else
            {
                LoggerHelper.Error("Simple Rule Executer failed to start ,as there is mistake in rule.json(configuraion) file");
            }
        }


        /// <summary>
        /// ExecuteRule
        /// </summary>
        /// <param name="shutdownToken"></param>
        private void ExecuteRule(CancellationToken shutdownToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            int CollectionInterval = _ruleEngineSettings.SimpleRule.RuleExecutionSeconds * 1000;

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        //Start the watch
                        stopwatch.Start();

                        //Get the dynamic raw data for the workflow
                        (dynamic, DataBusMessage) inputData = _ruleDataProvider.GetInputParameterValue();

                        //Get the workflow
                        RulesEngine.RulesEngine ruleEngine = new RulesEngine.RulesEngine(_workflows.ToArray(), null);

                        if (inputData.Item1 != null && inputData.Item2 != null)
                        {
                            foreach (Workflow workflow in _workflows)
                            {
                                //Get rule name to execute
                                string ruleName = workflow.WorkflowName;

                                //execute rule
                                List<RuleResultTree> resultList = ruleEngine.ExecuteAllRulesAsync(ruleName, inputData.Item1).Result;

                                if (resultList != null && resultList.Count > 0)
                                {
                                    //Get rule execution result, if success
                                    if (resultList[0].IsSuccess)
                                    {
                                        //Success condition
                                        resultList.OnSuccess((eventName) =>
                                        {
                                            LoggerHelper.Information($"{ruleName} simple rule passed the condition");

                                            //Send signalr notification to server
                                            if (_ruleEngineSettings.NotificationEnabled)
                                            {
                                                SendNotificationToServer(inputData.Item2, workflow);
                                            }

                                            //Update RedisDB
                                            if (_ruleEngineSettings.DataBusSettings.DBPersistEnabled)
                                            {
                                                UpdateToRedisDB(inputData.Item2, workflow);
                                            }

                                            //Command message
                                            if (_ruleEngineSettings.DataBusSettings.CommandMessageEnabled)
                                            {
                                                SendCommandMessage(inputData.Item2);
                                            }
                                        });

                                        //On fail means parameter conditions are ok
                                        resultList.OnFail(() =>
                                        {
                                            if (_ruleEngineSettings.NotificationEnabled)
                                            {
                                                _notifcationClient.SendParameterStatus(inputData.Item2.Id, ParameterStatus.Healthy.ToString()).ConfigureAwait(false);
                                            }
                                        });
                                    }
                                }
                            }
                        }

                        //stop watch after reading
                        stopwatch.Stop();

                        //calculate time taken t read all parameters
                        long executionTime = stopwatch.ElapsedMilliseconds;

                        //Reset stopwatch
                        stopwatch.Reset();

                        long sleepTime = CollectionInterval - executionTime;
                        TimeSpan sleepTimeBeforeNextCollection = TimeSpan.FromMilliseconds(sleepTime);

                        //Check sleep time shoul be > 0
                        if (sleepTimeBeforeNextCollection.Milliseconds > 0 && sleepTimeBeforeNextCollection.Milliseconds < CollectionInterval)
                        {
                            //Sleep for the specified interval
                            await Task.Delay(sleepTimeBeforeNextCollection);
                        }
                        else
                        {
                            await Task.Delay(0);//no sleep
                        }
                    }
                    catch (Exception exp)
                    {
                        LoggerHelper.Error(exp, $"Error executing workflow for Simple Rule engine");
                    }
                    finally
                    {
                        //Reset stopwatch
                        stopwatch.Reset();
                    }
                }
            }).ConfigureAwait(false);
        }


        /// <summary>
        /// SendUpdateToRedisDBAsync
        /// </summary>
        /// <param name="Item2"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private void UpdateToRedisDB(DataBusMessage dataBusMessage, Workflow workflow)
        {
            try
            {
                //Add to DB
                _redisDataBus.InsertToDbAsync(dataBusMessage.Id, dataBusMessage.Value + Constant.SEPARATOR +
                   workflow.Rules.FirstOrDefault().Expression + Constant.SEPARATOR + dataBusMessage.Time.ToString()).ConfigureAwait(false);

                if (_ruleEngineSettings.DataBusSettings.DBStreamEnabled)
                {
                    //Add to stream
                    _redisDataBus.AddStreamAsync(dataBusMessage, _ruleEngineSettings.DataBusSettings.StreamLength, StreamSourceType.RuleEngine).ConfigureAwait(false);
                    LoggerHelper.Information($"Data updated to RedisDB for parameter '{dataBusMessage.Id}'");
                }
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, "Error inserting to RedisDB for Simple workflow.");
            }
        }


        /// <summary>
        /// SendUpdateToServerAsync
        /// </summary>
        /// <param name="dataBusMessage"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        private void SendNotificationToServer(DataBusMessage dataBusMessage, Workflow workflow)
        {
            string min = string.Empty;
            string max = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(workflow.Rules.FirstOrDefault().Expression))
                {
                    try
                    {
                        min = Regex.Split(workflow.Rules.FirstOrDefault().Expression, @"\D+")[1];
                        max = Regex.Split(workflow.Rules.FirstOrDefault().Expression, @"\D+")[2];
                    }
                    catch { }
                }

                ParameterAlert parameterAlert = new ParameterAlert()
                {
                    ParameterName = dataBusMessage.Id,
                    Value = dataBusMessage.Value,
                    Time = dataBusMessage.Time.ToString(),
                    LowLimit = min,
                    HighLimit = max,
                    Message = $"{dataBusMessage.Id} Not Ok"
                };

                //Publish to server over signalR
                _notifcationClient.SendParameterStatus(dataBusMessage.Id, ParameterStatus.NotOk.ToString()).ConfigureAwait(false);

                //Publish to server over signalR
                _notifcationClient.SendAlertForParameter(dataBusMessage.Id, parameterAlert).ConfigureAwait(false);

                LoggerHelper.Information($"Notification sent to server for parameter '{dataBusMessage.Id}'");
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, "Error Sendig Notification to Server");
            }
        }

        /// <summary>
        /// SendCommandmessage
        /// </summary>
        /// <param name="dataBusMessage"></param>
        /// <returns></returns>
        private void SendCommandMessage(DataBusMessage dataBusMessage)
        {
            try
            {
                CommandMessage commandMessage = new CommandMessage()
                {
                    CommandAction = CommandAction.NOK.ToString(),
                    ParameterId = dataBusMessage.Id,
                    CommandType = CommandType.Device.ToString(),
                    MachineId = dataBusMessage.Source
                };

                _controlCommand.SendCommandAsync(commandMessage).ConfigureAwait(false);

                LoggerHelper.Information($"Command message sent to databus for parameter '{dataBusMessage.Id}'");
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, "Error Sendig CommandMessage");
            }
        }

        /// <summary>
        /// Stop.
        /// </summary>
        public void Stop()
        {
            try
            {
                _ruleExecutionCts?.Cancel();

                //wait till task finishes
                RuleExecutionTask?.Wait();
                RuleExecutionTask = null;

                _ruleExecutionCts?.Dispose();
                _ruleExecutionCts = null;
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, "Error while shutting simple rule executer.");
            }

            Task.Delay(100);
        }
    }
}

