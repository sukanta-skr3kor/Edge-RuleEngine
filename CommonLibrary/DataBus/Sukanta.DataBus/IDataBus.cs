//*********************************************************************************************
//* File             :   IDataBus.cs
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

namespace Sukanta.DataBus.Abstraction
{
    /// <summary>
    /// Contract
    /// </summary>
    public interface IDataBus
    {
        /// <summary>
        /// DataBus Name
        /// </summary>
        string DataBusName { get; set; }

        /// <summary>
        /// Is the DataBus connected to broker ?
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// DataBus Server to connect(Redis, Mqtt, ZMQ)
        /// </summary>
        string Server { get; set; }

        /// <summary>
        /// Supported DataBus Types(Exp. Redis, Mqtt or ZMQ etc.)
        /// </summary>
        CommunicationBus CommunicationMode { get; }

        /// <summary>
        /// Connect to databus
        /// </summary>
        /// <returns></returns>
        bool TryConnect();
    }

    /// <summary>
    /// DataBus Types
    /// </summary>
    public enum CommunicationBus
    {
        Redis,
        Mqtt,
        ZMQ
    }
}
