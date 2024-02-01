//*********************************************************************************************
//* File             :   MultiParameterRuleExecuter.cs
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
using System.Threading;
using System.Threading.Tasks;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace Sukanta.Edge.RuleEngine.Core
{
    /// <summary>
    /// Complex rule executer
    /// </summary>
    public class MultiParameterRuleExecuter
    {
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
        /// Work flow data
        /// </summary>
        private List<Workflow> _workflows;

        /// <summary>
        /// Rule engine configuration settings
        /// </summary>
        private RuleEngineSettings _ruleEngineSettings { get; set; }

        private Task RuleExecutionTask;
        private CancellationTokenSource _ruleExecutionCts;
        private readonly CancellationToken _shutdownToken;
        private string _ruleExpression = string.Empty;
        private const string SEPARATOR = " | ";

        /// <summary>
        /// MultiParameter/Complex RuleExecuter
        /// </summary>
        /// <param name="ruleDataProvider"></param>
        /// <param name="redisDataBusPublisher"></param>
        /// <param name="redisDataBus"></param>
        /// <param name="alertNotificationClient"></param>
        /// <param name="settings"></param>
        public MultiParameterRuleExecuter(IRuleDataProvider ruleDataProvider, RedisDataBus redisDataBus,
             AlertNotificationClient alertNotificationClient, RuleEngineSettings settings)
        {
            _ruleEngineSettings = settings;
            _ruleDataProvider = ruleDataProvider;
            _redisDataBus = redisDataBus;
            _notifcationClient = alertNotificationClient;

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
                LoggerHelper.Information("Loading multiparameter workflow into RuleEngine...");

                string[] files = Directory.GetFiles(Path.Combine(_ruleEngineSettings.ComplexRule.RuleFilePath), "rule.json", SearchOption.AllDirectories);

                if (files == null || files.Length == 0)
                {
                    throw new Exception("Rules not found.");
                }

                string fileData = File.ReadAllText(files[0]);

                _workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData);

                _ruleExpression = _workflows[0].Rules.Where(x => x.Enabled).Select(x => x.Expression).FirstOrDefault();

                LoggerHelper.Information("Multiparameter rule Workflow initialized");

                return true;
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, $"Complex rule Workflow configuration error");
            }

            return false;
        }

        /// <summary>
        /// start rule task
        /// </summary>
        public void StartExecution()
        {
            LoggerHelper.Information("Starting Complex Rule Executer...");

            if (InitializeWorkflow())
            {
                RuleExecutionTask = Task.Run(() => ExecuteRule(_shutdownToken), _shutdownToken);

                LoggerHelper.Information("Complex Rule Executer Started");
            }
            else
            {
                LoggerHelper.Error("Complex Rule Executer failed to start ,as therr is mistake in rule.json(configuraion) file");
            }
        }

        /// <summary>
        /// ExecuteRule
        /// </summary>
        /// <param name="shutdownToken"></param>
        private void ExecuteRule(CancellationToken shutdownToken)
        {
            string rule = string.Empty;
            string dbData = string.Empty;
            DateTime time = DateTime.Now;
            string ruleName = string.Empty;
            string workflowName = string.Empty;
            dynamic[] ruleInputParameters = null;
            List<DataBusMessage> dataBusMessages = null;
            Stopwatch stopwatch = new Stopwatch();
            string inputData1 = string.Empty, inputData2 = string.Empty, inputData3 = string.Empty, inputData4 = string.Empty, inputData5 = string.Empty;
            int CollectionInterval = _ruleEngineSettings.SimpleRule.RuleExecutionSeconds * 1000;

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        //Start the watch
                        stopwatch.Start();

                        (dynamic[], List<DataBusMessage>) data = _ruleDataProvider.GetInputParametersValues(_ruleEngineSettings, _ruleExpression);
                        ruleInputParameters = data.Item1;
                        dataBusMessages = data.Item2;

                        //Rule execution starts here
                        if (ruleInputParameters != null && dataBusMessages != null)
                        {
                            //Get the workflow
                            RulesEngine.RulesEngine ruleEngine = new RulesEngine.RulesEngine(_workflows.ToArray(), null);

                            foreach (Workflow workflow in _workflows)
                            {
                                //Get workflow name to execute
                                workflowName = workflow.WorkflowName;

                                //Rule Name
                                ruleName = workflow.Rules.FirstOrDefault().RuleName;

                                //execute rule
                                List<RuleResultTree> resultList = ruleEngine.ExecuteAllRulesAsync(workflowName, ruleInputParameters).Result;

                                if (resultList != null && resultList.Count > 0)
                                {
                                    //Get rule execution result, if success
                                    if (resultList[0].IsSuccess)
                                    {
                                        dbData = string.Empty;

                                        //Success condition
                                        resultList.OnSuccess(async (eventName) =>
                                        {
                                            LoggerHelper.Information($"{ruleName} complex rule passed the condition");

                                            //Add to DB
                                            if (_ruleEngineSettings.DataBusSettings.DBPersistEnabled || _ruleEngineSettings.NotificationEnabled)
                                            {
                                                rule = workflow.Rules.FirstOrDefault().Expression;

                                                if (dataBusMessages.Count >= 1)
                                                {
                                                    time = dataBusMessages[0].Time;
                                                    inputData1 = dataBusMessages[0].Value + SEPARATOR;
                                                    dbData = dataBusMessages[0].Value;
                                                }
                                                if (dataBusMessages.Count >= 2)
                                                {
                                                    time = dataBusMessages[1].Time;
                                                    inputData2 = dataBusMessages[1].Value + SEPARATOR;
                                                    dbData = inputData1 + dataBusMessages[1].Value;
                                                }
                                                if (dataBusMessages.Count >= 3)
                                                {
                                                    time = dataBusMessages[2].Time;
                                                    inputData3 = dataBusMessages[2].Value + SEPARATOR;
                                                    dbData = inputData1 + inputData2 + dataBusMessages[2].Value;
                                                }
                                                if (dataBusMessages.Count >= 4)
                                                {
                                                    time = dataBusMessages[3].Time;
                                                    inputData4 = dataBusMessages[3].Value + SEPARATOR;
                                                    dbData = inputData1 + inputData2 + inputData3 + dataBusMessages[3].Value;
                                                }
                                                if (dataBusMessages.Count >= 5)//Max 5 parameters supported
                                                {
                                                    time = dataBusMessages[4].Time;
                                                    inputData5 = dataBusMessages[4].Value;
                                                    dbData = inputData1 + inputData2 + inputData3 + inputData4 + inputData5;
                                                }

                                                //Send notification to server  over signalR
                                                if (_ruleEngineSettings.NotificationEnabled)
                                                {
                                                    SendNotficationToServer(dbData, time.ToString(), workflow);
                                                }

                                                //Insert in RedisDB
                                                if (!string.IsNullOrEmpty(dbData) && _ruleEngineSettings.DataBusSettings.DBPersistEnabled)
                                                {
                                                    UpdateToRedisDBAsync(ruleName, workflow.WorkflowName, dbData + Constant.SEPARATOR + rule
                                                           + Constant.SEPARATOR + dataBusMessages[0].Time + Constant.SEPARATOR
                                                           + workflow.Rules.FirstOrDefault().Expression);
                                                }
                                            }
                                        });
                                    }
                                }
                            }

                            ruleInputParameters = null;
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
                        LoggerHelper.Error(exp, $"Error executing workflow for Complex Rule engine");
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
        /// UpdateToRedisDBAsync
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="WorkflowName"></param>
        /// <param name="data"></param>
        private void UpdateToRedisDBAsync(string ruleName, string WorkflowName, string data)
        {
            try
            {
                _redisDataBus.InsertToDbAsync(ruleName, data).ConfigureAwait(false);
                LoggerHelper.Information($"Data updated to RedisDB for complex workflow '{WorkflowName}.");
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, $"Error inserting data to RedisDb for complex workflow");
            }
        }

        /// <summary>
        /// SendNotficationToServer over signalr
        /// </summary>
        /// <param name="parameterAndValue"></param>
        /// <param name="time"></param>
        /// <param name="workflow"></param>
        private void SendNotficationToServer(string parameterAndValue, string time, Workflow workflow)
        {
            try
            {
                ComplexAlertNotification complexAlertNotification = new ComplexAlertNotification()
                {
                    RuleName = workflow.Rules.FirstOrDefault().RuleName,
                    Rule = workflow.Rules.FirstOrDefault().Expression,
                    Time = time,
                    ParameterAndValues = parameterAndValue
                };

                //Publish to server over signalR
                _notifcationClient.SendComplexAlert(complexAlertNotification).ConfigureAwait(false);

                LoggerHelper.Information($"Notification sent to server for complex workflow '{workflow.WorkflowName}'");
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, "Error Sending Notfication to Server");
            }
        }

        /// <summary>
        ///Stop rule executer
        /// </summary>
        /// <param name="hubClient"></param>
        /// <returns></returns>
        public void Stop()
        {
            try
            {
                //wait till task finishes
                _ruleExecutionCts?.Cancel();

                RuleExecutionTask?.Wait();
                RuleExecutionTask = null;

                _ruleExecutionCts?.Dispose();
                _ruleExecutionCts = null;
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, "Error while shutting complex rule executer.");
            }
            Task.Delay(100);
        }
    }
}

