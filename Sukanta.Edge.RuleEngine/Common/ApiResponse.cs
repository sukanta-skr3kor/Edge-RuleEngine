//*********************************************************************************************
//* File             :   ApiResponse.cs
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

using Newtonsoft.Json;

namespace Sukanta.Edge.RuleEngine.Common
{
    /// <summary>
    /// General api response class
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Indicates if api execution was successful
        /// </summary>
        [JsonProperty("isOk")]
        public bool IsOk { get; set; }

        /// <summary>
        /// Any response message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Api response message
    /// </summary>
    public class ApiContentResponse<T> : ApiResponse
    {
        /// <summary>
        /// Payload of the api
        /// </summary>
        [JsonProperty("content")]
        public T Content { get; set; }
    }
}
