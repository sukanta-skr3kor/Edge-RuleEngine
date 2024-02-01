//*********************************************************************************************
//* File             :   IDataBusPublish.cs
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
    /// Publish interface
    /// </summary>
    public interface IDataBusPublish : IDataBus
    {
        /// <summary>
        /// Publish To DataBus Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task PublishDataBusMessageAsync<T>(T dataItem, string topic) where T : IDataBusMessage;

        /// <summary>
        ///  Publish To DataBus Async
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task PublishDataBusMessageAsync(object dataItem, string topic);

        /// <summary>
        /// {Key : value} pair message publish
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task PublishKeyValueMessageAsync<T>(T dataItem, string topic) where T : IDataBusMessage;
    }
}
