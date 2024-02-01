//*********************************************************************************************
//* File             :   IAlertNotification.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sukanta.Edge.RuleEngine.NotificationHub
{
    /// <summary>
    /// AlertNotification contract
    /// </summary>
    public interface IAlertNotification
    {
        /// <summary>
        /// SendCombinedAlert for all parameters
        /// </summary>
        /// <param name="parameterAlertMsgs"></param>
        /// <returns></returns>
        Task SendCombinedAlert(List<ParameterAlert> parameterAlertMsgs);

        /// <summary>
        /// SendAlertForParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterAlertMsg"></param>
        /// <returns></returns>
        Task SendAlertForParameter(string parameterName, ParameterAlert parameterAlertMsg);

        /// <summary>
        /// SendParameterStatus
        /// </summary>
        /// <param name="ParameterName"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        Task SendParameterStatus(string ParameterName, string Status);

        /// <summary>
        /// Alert for comples rules
        /// </summary>
        /// <param name="complexAlertNotification"></param>
        /// <returns></returns>
        Task SendComplexAlert(ComplexAlertNotification complexAlertNotification);
    }
}