//*********************************************************************************************
//* File             :   AlertNotificationHub.cs
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
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sukanta.Edge.RuleEngine.NotificationHub
{
    /// <summary>
    /// Notification server Hub
    /// </summary>
    public class AlertNotificationHub : Hub
    {
        /// <summary>
        /// SendAlert For Single Parameter from SimpleRule
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterAlertMsg"></param>
        /// <returns></returns>
        public async Task SendAlertForParameter(string parameterName, ParameterAlert parameterAlertMsg)
        {
            try
            {
                if (Clients != null)
                {
                    await Clients.All.SendAsync("ParameterAlert", parameterName, parameterAlertMsg);
                }
            }
            catch { }
        }

        /// <summary>
        /// Combind Alert for all arameters 
        /// </summary>
        /// <param name="parameterAlertMsgs"></param>
        /// <returns></returns>
        public async Task SendCombinedAlert(List<ParameterAlert> parameterAlertMsgs)
        {
            try
            {
                if (Clients != null)
                {
                    await Clients.All.SendAsync("AllParameterAlert", parameterAlertMsgs);
                }
            }
            catch { }
        }

        /// <summary>
        /// SendParameterStatus
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public async Task SendParameterStatus(string ParameterName, string Status)
        {
            try
            {
                if (Clients != null)
                {
                    await Clients.All.SendAsync("ParamterStatus", ParameterName, Status);
                }
            }
            catch { }
        }


        /// <summary>
        /// SendComplexAlert for complex rule
        /// </summary>
        /// <param name="complexAlertNotification"></param>
        /// <returns></returns>
        public async Task SendComplexAlert(ComplexAlertNotification complexAlertNotification)
        {
            try
            {
                if (Clients != null)
                {
                    await Clients.All.SendAsync("ComplexAlert", complexAlertNotification);
                }
            }
            catch { }
        }

    }
}
