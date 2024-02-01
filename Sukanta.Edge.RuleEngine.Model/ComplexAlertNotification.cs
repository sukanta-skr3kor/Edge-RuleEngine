//*********************************************************************************************
//* File             :   ComplexAlertNotification.cs
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

namespace Sukanta.Edge.RuleEngine.Model
{
    public class ComplexAlert
    {
        public bool HasAlert { get; set; } = true;

        public string RuleName { get; set; }

        public string Rule { get; set; }

        public string ParameterAndValues { get; set; }

        public string Time { get; set; }
    }

    public class ComplexAlertNotification : ComplexAlert
    {

    }
}
