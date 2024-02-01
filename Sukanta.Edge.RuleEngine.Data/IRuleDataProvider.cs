//*********************************************************************************************
//* File             :   IRuleDataProvider.cs
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
using Sukanta.Edge.RuleEngine.Model;
using System.Collections.Generic;

namespace Sukanta.Edge.RuleEngine.Data
{
    /// <summary>
    /// IRuleDataProvider
    /// </summary>
    public interface IRuleDataProvider
    {
        /// <summary>
        /// GetInputParameterValue
        /// </summary>
        /// <returns></returns>
        (dynamic, DataBusMessage) GetInputParameterValue();

        /// <summary>
        /// GetInputParametersValues
        /// </summary>
        /// <param name="ruleEngineSettings"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        (dynamic[], List<DataBusMessage>) GetInputParametersValues(RuleEngineSettings ruleEngineSettings, string rule);
    }
}
