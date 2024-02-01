//*********************************************************************************************
//* File             :   IRedisConnection.cs
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

using StackExchange.Redis;
using System;

namespace Sukanta.DataBus.Redis
{
    /// <summary>
    /// Contract
    /// </summary>
    public interface IRedisConnection : IDisposable
    {
        /// <summary>
        /// Is Connected to Redis ?
        /// </summary>
        bool IsConnected();

        /// <summary>
        /// Try Connect
        /// </summary>
        /// <returns></returns>
        bool TryConnect();

        /// <summary>
        /// Get the connecition for Redis
        /// </summary>
        /// <returns></returns>
        IConnectionMultiplexer GetConnection();
    }
}
