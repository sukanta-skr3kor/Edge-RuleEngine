//*********************************************************************************************
//* File             :   AlertNotificationClient.cs
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

using Sukanta.Edge.RuleEngine.Model;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Sukanta.Edge.RuleEngine.NotificationHub
{
    /// <summary>
    /// Alert NotificationHub Client
    /// </summary>
    public class AlertNotificationClient : IDisposable, IAlertNotification
    {
        /// <summary>
        /// NotificationHub connector
        /// </summary>
        public HubConnection NotificationHub { get; set; }

        /// <summary>
        /// RuleEngine Settings
        /// </summary>
        private RuleEngineSettings _ruleEngineSettings { get; set; }

        /// <summary>
        /// AlertNotificationClient
        /// </summary>
        /// <param name="ruleEngineSettings"></param>
        public AlertNotificationClient(RuleEngineSettings ruleEngineSettings)
        {
            _ruleEngineSettings = ruleEngineSettings;
            Initialize();
        }

        /// <summary>
        /// Initialize Hub
        /// </summary>
        public void Initialize()
        {
            try
            {
                if (NotificationHub == null)
                {
                    BuildNotificationHub();
                }
                else if (NotificationHub.State == HubConnectionState.Disconnected)
                {
                    NotificationHub.DisposeAsync();
                    NotificationHub = null;
                    BuildNotificationHub();
                }
            }
            catch { }
        }

        /// <summary>
        /// Init Hub if disconnected
        /// </summary>
        private bool IsHubInitialized()
        {
            if (NotificationHub == null || NotificationHub.State == HubConnectionState.Disconnected)
            {
                Initialize();
            }

            return NotificationHub.State == HubConnectionState.Connected;
        }

        /// <summary>
        /// BuildNotificationHub
        /// </summary>
        private void BuildNotificationHub()
        {
            IHubConnectionBuilder builder = new HubConnectionBuilder()
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })//Auto reconnect
                .WithUrl(_ruleEngineSettings.SignalrHubUrl, options =>
                {
                    //Certificate handling 
                    if (_ruleEngineSettings.UseCertificate)
                    {
                        if (!string.IsNullOrEmpty(_ruleEngineSettings.CertificateFile))
                        {
                            X509Certificate2 certificateFile = new X509Certificate2(_ruleEngineSettings.CertificateFile);
                            options.ClientCertificates.Add(certificateFile);
                        }
                    }
                });

            //Build hub
            NotificationHub = builder.Build();

            //On Close connection retry to connect
            NotificationHub.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await NotificationHub.StartAsync();
            };

            //Start hub client
            NotificationHub.StartAsync();
        }

        /// <summary>
        /// Dispose Hub Client
        /// </summary>
        public void Dispose()
        {
            if (NotificationHub != null)
            {
                NotificationHub.DisposeAsync();
                NotificationHub = null;
            }
        }

        /// <summary>
        /// SendAlertForParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterAlertMsg"></param>
        /// <returns></returns>
        public async Task SendAlertForParameter(string parameterName, ParameterAlert parameterAlertMsg)
        {
            if (IsHubInitialized())
            {
                await NotificationHub.InvokeAsync("SendAlertForParameter", parameterName, parameterAlertMsg).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// SendCombinedAlert
        /// </summary>
        /// <param name="parameterAlertMsgs"></param>
        /// <returns></returns>
        public async Task SendCombinedAlert(List<ParameterAlert> parameterAlertMsgs)
        {
            if (IsHubInitialized())
            {
                await NotificationHub.InvokeAsync("SendCombinedAlert", parameterAlertMsgs).ConfigureAwait(false);
                // await NotificationHub.SendAsync("AllParameterAlert", parameterAlertMsgs).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// SendParameterStatus
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public async Task SendParameterStatus(string ParameterName, string Status)
        {
            if (IsHubInitialized())
            {
                await NotificationHub.InvokeAsync("SendParameterStatus", ParameterName, Status).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// SendComplexAlert for complex rule
        /// </summary>
        /// <param name="complexAlertNotification"></param>
        /// <returns></returns>
        public async Task SendComplexAlert(ComplexAlertNotification complexAlertNotification)
        {
            if (IsHubInitialized())
            {
                await NotificationHub.InvokeAsync("SendComplexAlert", complexAlertNotification).ConfigureAwait(false);
            }
        }
    }
}
