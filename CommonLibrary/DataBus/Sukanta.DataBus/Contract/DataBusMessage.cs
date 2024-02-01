//*********************************************************************************************
//* File             :   DataBusMessage.cs
//* Author           :   Rout, Sukanta 
//* Date             :   2/1/2024
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

using Newtonsoft.Json;
using System;

namespace Sukanta.DataBus.Abstraction
{
    /// <summary>
    /// DataBus message contract
    /// </summary>
    public interface IDataBusMessage
    {
        string Id { get; set; }

        string Value { get; set; }

        DateTime Time { get; set; }

        string Source { get; set; }
    }

    /// <summary>
    /// DataBus Message
    /// </summary>
    public class DataBusMessage : IDataBusMessage
    {
        /// <summary>
        /// Parameter Id
        /// </summary>
        [JsonProperty]
        public string Id { get; set; }

        /// <summary>
        ///Parameter Value
        /// </summary>
        [JsonProperty]
        public string Value { get; set; }

        /// <summary>
        /// Message creation time in 
        /// </summary>
        [JsonProperty]
        public DateTime Time { get; set; }

        /// <summary>
        /// Message Name
        /// </summary>
        [JsonProperty]
        public string Source { get; set; }

        /// <summary>
        /// Contr
        /// </summary>
        [JsonConstructor]
        public DataBusMessage()
        {
            Time = DateTime.Now;
        }
    }
}
