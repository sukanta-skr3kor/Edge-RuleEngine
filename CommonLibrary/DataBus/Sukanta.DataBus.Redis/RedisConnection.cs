//*********************************************************************************************
//* File             :   RedisConnection.cs
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
    /// Redis Connection
    /// </summary>
    public class RedisConnection : IRedisConnection
    {
        /// <summary>
        /// Connect object for Redis
        /// </summary>
        private Lazy<IConnectionMultiplexer> _connection;

        /// <summary>
        /// Redis Conn string
        /// </summary>
        private string _serverConnectionString;

        /// <summary>
        /// IS conneted to redis ?
        /// </summary>
        public bool IsConnected()
        {
            try
            {
                return (bool)(_connection?.Value?.IsConnected);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Redis Connection, Lazy initilization
        /// </summary>
        /// <param name="serverConnectionString"></param>
        public RedisConnection(string serverConnectionString)
        {
            _serverConnectionString = serverConnectionString;
            _connection = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_serverConnectionString), true);
        }

        /// <summary>
        /// Try Connect to Redis again
        /// </summary>
        /// <returns></returns>
        public bool TryConnect()
        {
            try
            {
                _connection = null;

                _connection = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_serverConnectionString), true);

                return (bool)(_connection?.Value?.IsConnected);
            }
            catch (RedisConnectionException exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// Get Connection Object(Singleton instance)
        /// </summary>
        /// <returns></returns>
        public IConnectionMultiplexer GetConnection()
        {
            if ((bool)!_connection?.IsValueCreated)
            {
                TryConnect();
            }

            return _connection?.Value;
        }

        /// <summary>
        /// Dispose connection object ade free resources
        /// </summary>
        public void Dispose()
        {
            _connection?.Value?.Dispose();
        }
    }
}
