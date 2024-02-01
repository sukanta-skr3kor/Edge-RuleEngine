//*********************************************************************************************
//* File             :   DataValidationException.cs
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

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Sukanta.Edge.RuleEngine.Common
{
    /// <summary>
    /// Data validation exception
    /// </summary>
    [Serializable]
    public class DataValidationException : ValidationException
    {
        /// <summary>
        /// DataValidationException
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DataValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// DataValidationException
        /// </summary>
        /// <param name="message"></param>
        public DataValidationException(string message) : base(message) { }
    }
}
