//*********************************************************************************************
//* File             :   Alert.cs
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
    public class Alert
    {
        public string ParameterName { get; set; }

        public bool HasAlert { get; set; } = true;

        public string Value { get; set; }

        public string Time { get; set; }

        public string Message { get; set; }
    }
}
