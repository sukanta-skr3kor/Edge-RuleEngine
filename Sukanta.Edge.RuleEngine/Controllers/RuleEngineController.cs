//*********************************************************************************************
//* File             :   RuleEngineController.cs
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
using Sukanta.Edge.RuleEngine.Common;
using Sukanta.Edge.RuleEngine.Model;
using Sukanta.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RulesEngine.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Sukanta.Edge.RuleEngine.Controllers
{
    /// <summary>
    /// RuleEngineController
    /// </summary>
    [Route("api/v1/ruleengine")]
    [ApiController]
    public class RuleEngineController : ControllerBase
    {
        /// <summary>
        /// RedisDataBus
        /// </summary>
        private RedisDataBus _redisDataBus;

        /// <summary>
        /// RuleEngine Settings
        /// </summary>
        private RuleEngineSettings _settings;

        /// <summary>
        /// RuleEngineController
        /// </summary>
        /// <param name="redisDataBus"></param>
        /// <param name="settings"></param>
        public RuleEngineController(RedisDataBus redisDataBus, IOptions<RuleEngineSettings> settings)
        {
            _settings = settings.Value;
            _redisDataBus = redisDataBus;
        }

        /// <summary>
        /// Rule engine status
        /// </summary>
        /// <returns></returns>
        [HttpGet(@"status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        public IActionResult EngineStatus()
        {
            try
            {
                return this.Ok("ONLINE");
            }
            catch (Exception exp)
            {
                return this.KnowOperationError(exp.Message);
            }
        }

        /// <summary>
        /// Get All configured parameters
        /// </summary>
        /// <returns></returns>
        [HttpGet(@"parameters")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public IActionResult GetAllConfiguredParameters()
        {
            List<string> parameters = new List<string>();

            try
            {
                if (_settings.Parameters != null)
                {
                    if (_settings.Parameters.Length > 0)
                    {
                        foreach (Parameters prm in _settings.Parameters)
                        {
                            parameters.Add(prm.Id);
                        }
                    }
                }

                try
                {
                    DirectoryInfo baseDirectory = Reflections.GetEntryAssemblyLocation();

                    string[] files = Directory.GetFiles(Path.Combine(_settings.SimpleRule.RuleFilePath), "rule.json", SearchOption.AllDirectories);

                    if (files != null || files.Length != 0)
                    {
                        string fileData = System.IO.File.ReadAllText(files[0]);

                        List<Workflow> _workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData);

                        foreach (Workflow workflow in _workflows)
                        {
                            string rule = workflow.Rules.FirstOrDefault().Expression;

                            string parameterId = Regex.Split(rule, @"\s+")[0].Trim();

                            if (!parameters.Contains(parameterId))
                            {
                                parameters.Add(parameterId);
                            }
                        }
                    }
                }
                catch { }

                if (parameters.Count > 0)
                {
                    return this.Ok(parameters);
                }

                return NoContent();
            }
            catch (Exception exp)
            {
                return this.KnowOperationError(exp.Message);
            }
        }


        /// <summary>
        /// Get All configured rules
        /// </summary>
        /// <returns></returns>
        [HttpGet(@"rules")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [Produces(typeof(ApiContentResponse<List<RuleType>>))]
        public IActionResult GetAllConfiguredRules()
        {
            List<RuleType> rules = new List<RuleType>();

            try
            {
                DirectoryInfo baseDirectory = Reflections.GetEntryAssemblyLocation();

                string[] simpleRulefiles = Directory.GetFiles(Path.Combine(_settings.SimpleRule.RuleFilePath), "rule.json", SearchOption.AllDirectories);

                if (simpleRulefiles != null || simpleRulefiles.Length != 0)
                {
                    string fileData = System.IO.File.ReadAllText(simpleRulefiles[0]);

                    List<Workflow> _workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData);

                    foreach (Workflow workflow in _workflows)
                    {
                        RuleType ruleType = new RuleType()
                        {
                            Type = "Simple",
                            Name = workflow.Rules.FirstOrDefault().RuleName
                        };
                        rules.Add(ruleType);
                    }
                }

                string[] ComplexRulefiles = Directory.GetFiles(Path.Combine(_settings.ComplexRule.RuleFilePath), "rule.json", SearchOption.AllDirectories);

                if (ComplexRulefiles != null || ComplexRulefiles.Length != 0)
                {
                    string fileData = System.IO.File.ReadAllText(ComplexRulefiles[0]);

                    List<Workflow> _workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData);

                    foreach (Workflow workflow in _workflows)
                    {
                        RuleType ruleType = new RuleType()
                        {
                            Type = "Complex",
                            Name = workflow.Rules.FirstOrDefault().RuleName
                        };
                        rules.Add(ruleType);
                    }
                }

                if (rules.Count > 0)
                {
                    return this.Ok(rules.Distinct());
                }

                return NoContent();
            }
            catch (Exception exp)
            {
                return this.KnowOperationError(exp.Message);
            }
        }


        /// <summary>
        /// Alert for all parameters configured in rule engine
        /// </summary>
        /// <param name="parametername"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpGet(@"alert/history/{parametername}/{count}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [Produces(typeof(ApiContentResponse<List<Alert>>))]
        public IActionResult GetAlertHistoryForParameter(string parametername, int count = 10)
        {
            if (string.IsNullOrEmpty(parametername))
            {
                return this.BadRequest("parameter name required");
            }

            try
            {
                if (!_settings.DataBusSettings.DBStreamEnabled)
                {
                    return this.BadRequest("Aert stream is not enabled");
                }

                List<Alert> alertStream = new List<Alert>();

                parametername = parametername.Trim();

                StreamEntry[] streamEntries = _redisDataBus.ReadStreamAsync(parametername, count, StreamSourceType.RuleEngine).Result;

                if (streamEntries != null && streamEntries.Length > 0)
                {
                    foreach (StreamEntry stream in streamEntries)
                    {
                        if (stream.Values.Length >= 3)
                        {
                            alertStream.Add(new Alert()
                            {
                                ParameterName = stream.Values[0].Value.ToString(),
                                Value = stream.Values[1].Value.ToString(),
                                Time = stream.Values[2].Value.ToString(),
                                Message = $"{parametername} not within deined limit"
                            });
                        }
                    }

                    alertStream = alertStream.OrderBy(x => x.Time).ToList();
                    return this.Ok(alertStream);
                }

                return NoContent();
            }
            catch (DataValidationException exp)
            {
                return this.KnowOperationError(exp.Message);
            }
            catch (Exception exp)
            {
                return this.KnowOperationError(exp.Message);
            }
        }

        /// <summary>
        ///  Alert for a single parameter configured
        /// </summary>
        /// <param name="parametername"></param>
        /// <returns></returns>
        [HttpGet(@"alert/current/{parametername}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [Produces(typeof(ApiContentResponse<ParameterAlert>))]
        public IActionResult GetCurentAlertForParameter(string parametername)
        {
            if (string.IsNullOrEmpty(parametername))
            {
                return this.BadRequest("parameter name required");
            }

            try
            {
                if (!_settings.DataBusSettings.DBPersistEnabled)
                {
                    return this.BadRequest("Alert storage is not enabled");
                }

                parametername = parametername.Trim();

                RedisValue data = _redisDataBus.GetFromDbAsync(parametername).Result;

                if (data.HasValue)
                {
                    string min = string.Empty;
                    string max = string.Empty;
                    string value = string.Empty;
                    string timeOfAlert = string.Empty;
                    string alertRule = string.Empty;

                    try
                    {
                        string[] tempData = data.ToString()?.Split(Constant.SEPARATOR);

                        if (tempData.Length >= 2)
                        {
                            value = tempData[0];
                            alertRule = tempData[1];
                            timeOfAlert = tempData[2];

                            min = Regex.Split(alertRule, @"\D+")[1].ToString().Trim();
                            max = Regex.Split(alertRule, @"\D+")[2].ToString().Trim();
                        }
                    }
                    catch { }

                    ParameterAlert parameterAlert = new ParameterAlert()
                    {
                        ParameterName = parametername,
                        Value = value,
                        LowLimit = min,
                        HighLimit = max,
                        Time = timeOfAlert,
                        Message = $"{parametername} not within defined limit"
                    };

                    return this.Ok(parameterAlert);
                }

                return NoContent();
            }
            catch (DataValidationException exp)
            {
                return this.KnowOperationError(exp.Message);
            }
            catch (Exception exp)
            {
                return this.KnowOperationError(exp.Message);
            }
        }


        /// <summary>
        ///  Alert for a single parameter configured
        /// </summary>
        /// <param name="parametername"></param>
        /// <returns></returns>
        [HttpGet(@"alert/complex/{rulename}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [Produces(typeof(ApiContentResponse<ComplexAlert>))]
        public IActionResult GetCurentAlertForRule(string rulename)
        {
            if (string.IsNullOrEmpty(rulename))
            {
                return this.BadRequest("Rule name required");
            }

            if (_settings.Parameters.Any(x => x.Id == rulename))
            {
                return this.BadRequest("Rule name can not be a parameter name");
            }

            try
            {
                if (!_settings.DataBusSettings.DBPersistEnabled)
                {
                    return this.BadRequest("Alert storage is not enabled");
                }

                rulename = rulename.Trim();

                RedisValue data = _redisDataBus.GetFromDbAsync(rulename).Result;

                if (data.HasValue)
                {
                    string value = string.Empty;
                    string timeOfAlert = string.Empty;
                    string alertRule = string.Empty;

                    try
                    {
                        string[] tempData = data.ToString()?.Split(Constant.SEPARATOR);

                        if (tempData.Length >= 2)
                        {
                            value = tempData[0];
                            alertRule = tempData[1];
                            timeOfAlert = tempData[2];
                        }
                    }
                    catch { }

                    ComplexAlert complexAlertNotification = new ComplexAlert()
                    {
                        RuleName = rulename,
                        ParameterAndValues = value,
                        Time = timeOfAlert,
                        Rule = alertRule
                    };

                    return this.Ok(complexAlertNotification);
                }

                return NoContent();
            }
            catch (DataValidationException exp)
            {
                return this.KnowOperationError(exp.Message);
            }
            catch (Exception exp)
            {
                return this.KnowOperationError(exp.Message);
            }
        }

    }
}
