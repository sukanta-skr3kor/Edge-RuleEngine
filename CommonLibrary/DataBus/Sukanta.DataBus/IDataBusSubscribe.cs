//*********************************************************************************************
//* File             :   IDataBusSubscribe.cs
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

using System.Threading.Tasks;

namespace Sukanta.DataBus.Abstraction
{
    /// <summary>
    /// Subscribe Interface
    /// </summary>
    public interface IDataBusSubscribe : IDataBus
    {
        /// <summary>
        /// SubscribeToDataBusAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task SubscribeToDataBusAsync<T>(string topic) where T : IDataBusMessage;

        /// <summary>
        /// SubscribeToDataBusAsync
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task SubscribeToDataBusAsync(string topic);

        /// <summary>
        /// GetData as DataBusMessage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        DataBusMessage GetDataBusMessage();

        /// <summary>
        /// Get json message
        /// </summary>
        /// <returns></returns>
        string GetDataBusMessageJson();
    }
}
