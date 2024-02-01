//*********************************************************************************************
//* File             :   RuleDataProvider.cs
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
using Sukanta.LoggerLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sukanta.Edge.RuleEngine.Data
{
    /// <summary>
    /// Rule Data Provider
    /// </summary>
    public class RuleDataProvider : IRuleDataProvider
    {
        private BlockingCollection<DataBusMessage> singleParamDataBusMessages;
        private BlockingCollection<DataBusMessage> multiParamDataBusMessages;
        private Task DataPersistTask = null;
        private CancellationTokenSource _dataPesistCts;
        private readonly CancellationToken _shutdownToken;
        private Dictionary<string, string> jsonDataDict = new Dictionary<string, string>();
        private static int parameterCounter = 0;
        private string _topic = string.Empty;
        private static bool isDataSubscribed = false;

        /// <summary>
        /// ExpandoObject Converter
        /// </summary>
        private ExpandoObjectConverter _converter;

        /// <summary>
        /// RedisDataBusSubscriber 
        /// </summary>
        private IRedisDataBusSubscriber _redisDataBusSubscriber;

        /// <summary>
        /// Rule engine configuration settings
        /// </summary>
        private RuleEngineSettings _ruleEngineSettings;


        /// <summary>
        /// Rule DataProvider
        /// </summary>
        /// <param name="redisDataBusSubscriber"></param>
        /// <param name="ruleEngineSettings"></param>
        /// <param name="topic"></param>
        public RuleDataProvider(IRedisDataBusSubscriber redisDataBusSubscriber, RuleEngineSettings ruleEngineSettings, string topic = null)
        {
            _redisDataBusSubscriber = redisDataBusSubscriber;
            _converter = new ExpandoObjectConverter();

            singleParamDataBusMessages = new BlockingCollection<DataBusMessage>();
            multiParamDataBusMessages = new BlockingCollection<DataBusMessage>();

            _dataPesistCts = new CancellationTokenSource();
            _shutdownToken = _dataPesistCts.Token;
            _ruleEngineSettings = ruleEngineSettings;

            if (string.IsNullOrEmpty(topic) && !string.IsNullOrEmpty(ruleEngineSettings.DataBusSettings.SubscribeTopic))
            {
                topic = ruleEngineSettings.DataBusSettings.SubscribeTopic;
                _topic = topic;
            }

            //Subscribe data message
            InitializeDataBusSystem();

            //Start data collection
            StartDataCollection();
        }

        /// <summary>
        /// Start DataCollection Task
        /// </summary>
        private void StartDataCollection()
        {
            try
            {
                DataPersistTask = Task.Run(() => CollectDataBusMessagesAsync(_shutdownToken).ConfigureAwait(false), _shutdownToken);
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, exp.Message);
            }
        }

        /// <summary>
        /// InitializeDataBusSystem
        /// </summary>
        private void InitializeDataBusSystem()
        {
            try
            {
                //Connect first
                if (!_redisDataBusSubscriber.IsConnected)
                {
                    if (_redisDataBusSubscriber.TryConnect())
                    {
                        LoggerHelper.Information("Connected to DataBus");
                    }
                }

                //Subscribe to message
                if (!isDataSubscribed)
                {
                    _redisDataBusSubscriber.SubscribeToDataBusAsync<DataBusMessage>(_topic).ConfigureAwait(false);
                    isDataSubscribed = true;
                    LoggerHelper.Information($"Connected to DataBus, topic '{_topic}' subscribed for message successfully");
                }
            }
            catch (Exception exp)
            {
                LoggerHelper.Error("Connection to DataBus failed");
                LoggerHelper.Error(exp, exp.Message);
            }
        }


        /// <summary>
        /// CollectDataBusMessagesAsync
        /// </summary>
        /// <returns></returns>
        private async Task CollectDataBusMessagesAsync(CancellationToken _shutdownToken)
        {
            DataBusMessage dataBusMessage = null;

            while (true)
            {
                try
                {
                    if (!isDataSubscribed)
                    {
                        InitializeDataBusSystem();
                    }

                    if (_redisDataBusSubscriber.HasData())
                    {
                        dataBusMessage = _redisDataBusSubscriber.GetDataBusMessage();

                        singleParamDataBusMessages.TryAdd(dataBusMessage);
                        await Task.Delay(5);
                        multiParamDataBusMessages.TryAdd(dataBusMessage);
                    }
                }
                catch (Exception exp)
                {
                    LoggerHelper.Error(exp, exp.Message);
                }
                //Delay loop
                await Task.Delay(_ruleEngineSettings.DataBusSettings.DataReadIntervalSeconds * 1000);
            }
        }

        /// <summary>
        /// GetData as dynamic input for a single parameter
        /// </summary>
        /// <returns></returns>
        public (dynamic, DataBusMessage) GetInputParameterValue()
        {
            (dynamic, DataBusMessage) data = (null, null);
            string jsonDataInput = string.Empty;
            DataBusMessage dataBusMessage = null;

            try
            {
                bool gotMessage = singleParamDataBusMessages.TryTake(out dataBusMessage);

                if (gotMessage)
                {
                    //Convert to son format for rules engine
                    jsonDataInput = $"{"{" + '"' + dataBusMessage.Id + '"'} : {dataBusMessage.Value + "}"}";

                    if (dataBusMessage != null && !string.IsNullOrEmpty(dataBusMessage.Id))
                    {
                        //Get the dynamic data
                        data.Item1 = JsonConvert.DeserializeObject<ExpandoObject>(jsonDataInput, _converter);
                        data.Item2 = new DataBusMessage()
                        {
                            Id = dataBusMessage.Id,
                            Value = dataBusMessage.Value,
                            Time = dataBusMessage.Time,
                            Source = dataBusMessage.Source
                        };
                    }
                }
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, exp.Message);
            }
            return data;
        }


        /// <summary>
        /// GetInputParaeters Values formultiple paraetrs at once
        /// Max 5 parametr is supported now
        /// </summary>
        /// <param name="ruleEngineSettings"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public (dynamic[], List<DataBusMessage>) GetInputParametersValues(RuleEngineSettings ruleEngineSettings, string rule)
        {
            dynamic input1 = null, input2 = null, input3 = null, input4 = null, input5 = null;

            dynamic[] data = null;
            string jsonDataInput = string.Empty;
            string dataBusJsonMessage = string.Empty;
            DataBusMessage dataBusMessage;
            string[] inputParameters;
            Dictionary<string, string> inputAndParameter = new Dictionary<string, string>();
            List<DataBusMessage> dataBusMessages = new List<DataBusMessage>();

            try
            {
                inputParameters = Regex.Split(rule, @"\s+").Where(x => x.Length > 5).Select(x => x).ToArray();

                foreach (string param in inputParameters)
                {
                    if (!string.IsNullOrEmpty(param))
                    {
                        string data1 = param.Split('.')[0];
                        string data2 = param.Split('.')[1].Split(' ')[0];

                        if (!inputAndParameter.ContainsKey(data1))
                        {
                            inputAndParameter.Add(data1, data2);
                        }
                    }
                }

                bool gotMessage = multiParamDataBusMessages.TryTake(out dataBusMessage);

                if (gotMessage)
                {
                    //Convert to son format for rules engine
                    dataBusJsonMessage = $"{"{" + '"' + dataBusMessage.Id + '"'} : {dataBusMessage.Value + "}"}";

                    if (!jsonDataDict.ContainsKey(dataBusMessage.Id))
                    {
                        jsonDataDict.Add(dataBusMessage.Id, dataBusJsonMessage);
                        parameterCounter++;
                    }

                    if (parameterCounter == ruleEngineSettings.ComplexRule.NumberOfParamterToAnalyze)
                    {
                        parameterCounter = 0;

                        foreach (KeyValuePair<string, string> jsonData in jsonDataDict)
                        {
                            //Get the dynamic raw data for the workflow
                            foreach (KeyValuePair<string, string> KV in inputAndParameter)
                            {
                                if (jsonData.Value.Contains(KV.Value) && KV.Key == Constants.INPUT1)
                                {
                                    input1 = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Value, _converter);
                                    dataBusMessages.Add(new DataBusMessage() { Id = jsonData.Key, Value = jsonData.Value });
                                }

                                else if (jsonData.Value.Contains(KV.Value) && KV.Key == Constants.INPUT2)
                                {
                                    input2 = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Value, _converter);
                                    dataBusMessages.Add(new DataBusMessage() { Id = jsonData.Key, Value = jsonData.Value });
                                }

                                else if (jsonData.Value.Contains(KV.Value) && KV.Key == Constants.INPUT3)
                                {
                                    input3 = JsonConvert.DeserializeObject<ExpandoObject>(jsonDataInput, _converter);
                                    dataBusMessages.Add(new DataBusMessage() { Id = jsonData.Key, Value = jsonData.Value });
                                }

                                else if (jsonData.Value.Contains(KV.Value) && KV.Key == Constants.INPUT4)
                                {
                                    input4 = JsonConvert.DeserializeObject<ExpandoObject>(jsonDataInput, _converter);
                                    dataBusMessages.Add(new DataBusMessage() { Id = jsonData.Key, Value = jsonData.Value });
                                }

                                else if (jsonData.Value.Contains(KV.Value) && KV.Key == Constants.INPUT5)
                                {
                                    input5 = JsonConvert.DeserializeObject<ExpandoObject>(jsonDataInput, _converter);
                                    dataBusMessages.Add(new DataBusMessage() { Id = jsonData.Key, Value = jsonData.Value });
                                }
                                else
                                {
                                    //Do nothing 
                                }
                            }

                            data = new dynamic[] { input1, input2, input3, input4, input5 };
                        }

                        //Clear the dictionary, Important !!!
                        jsonDataDict.Clear();
                    }
                }
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, exp.Message);
            }

            return (data, dataBusMessages);
        }

        /// <summary>
        ///Stop
        /// </summary>
        public void Stop()
        {
            try
            {
                //wait till task finishes
                _dataPesistCts?.Cancel();

                DataPersistTask?.Wait();
                DataPersistTask = null;

                _dataPesistCts?.Dispose();
                _dataPesistCts = null;
            }
            catch (Exception exp)
            {
                LoggerHelper.Error(exp, "Error while shutting simple rule executer.");
            }
            Task.Delay(100);
        }

        /// <summary>
        /// Constants
        /// </summary>
        public static class Constants
        {
            public const string INPUT1 = "input1";
            public const string INPUT2 = "input2";
            public const string INPUT3 = "input3";
            public const string INPUT4 = "input4";
            public const string INPUT5 = "input5";
        }

    }
}
