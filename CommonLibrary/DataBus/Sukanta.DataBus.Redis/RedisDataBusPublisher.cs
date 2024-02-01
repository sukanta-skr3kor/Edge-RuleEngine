//*********************************************************************************************
//* File             :   RedisDataBusPublisher.cs
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
using Sukanta.DataBus.Abstraction;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Sukanta.DataBus.Redis
{
    /// <summary>
    /// IRedisDataBusPublisher inerface
    /// </summary>
    public interface IRedisDataBusPublisher : IDataBusPublish, ICommandMessagePublish
    {
    }

    /// <summary>
    /// Redis Data Publisher
    /// </summary>
    public class RedisDataBusPublisher : RedisDataBus, IRedisDataBusPublisher
    {
        /// <summary>
        /// RedisDataBusPublisher
        /// </summary>
        /// <param name="ServerConnString"></param>
        public RedisDataBusPublisher(string ServerConnString = null) : base(ServerConnString)
        { }

        /// <summary>
        /// PublishDataBusMessageAsync of type IDataBusMessage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Task PublishDataBusMessageAsync<T>(T dataItem, string topic) where T : IDataBusMessage
        {
            try
            {
                IConnectionMultiplexer connection = redisConnection.GetConnection();

                RedisChannel channel = new RedisChannel(topic, RedisChannel.PatternMode.Pattern);

                ISubscriber subscriber = connection.GetSubscriber();

                string jsonMessage = JsonConvert.SerializeObject(dataItem);

                if (connection.IsConnected)
                {
                    return subscriber.PublishAsync(channel, jsonMessage);
                }
                else
                {
                    throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Disconnected from Databus");
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }


        /// <summary>
        /// PublishDataBusMessageAsync of any type(object)
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Task PublishDataBusMessageAsync(object dataItem, string topic)
        {
            try
            {
                IConnectionMultiplexer connection = redisConnection.GetConnection();

                RedisChannel channel = new RedisChannel(topic, RedisChannel.PatternMode.Pattern);

                ISubscriber subscriber = connection.GetSubscriber();

                string jsonMessage = JsonConvert.SerializeObject(dataItem);

                if (connection.IsConnected)
                {
                    return subscriber.PublishAsync(channel, jsonMessage);
                }
                else
                {
                    throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Disconnected from Databus");
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// Command publisher
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandMessage"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Task PublishCommandMessageAsync<T>(T commandMessage, string topic) where T : CommandMessage
        {
            try
            {
                IConnectionMultiplexer connection = redisConnection.GetConnection();

                RedisChannel channel = new RedisChannel(topic, RedisChannel.PatternMode.Pattern);

                ISubscriber subscriber = connection.GetSubscriber();

                string jsonMessage = JsonConvert.SerializeObject(commandMessage);

                if (connection.IsConnected)
                {
                    return subscriber.PublishAsync(channel, jsonMessage);
                }
                else
                {
                    throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Disconnected from Databus");
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// PublishMessageAsync of key:value pair
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItem"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Task PublishKeyValueMessageAsync<T>(T dataItem, string topic) where T : IDataBusMessage
        {
            try
            {
                IConnectionMultiplexer connection = redisConnection.GetConnection();

                RedisChannel channel = new RedisChannel(topic, RedisChannel.PatternMode.Pattern);

                ISubscriber subscriber = connection.GetSubscriber();

                string jsonMessage = $"{"{" + '"' + dataItem.Id + '"'} : {dataItem.Value + "}"}";

                if (connection.IsConnected)
                {
                    return subscriber.PublishAsync(channel, jsonMessage);
                }
                else
                {
                    throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Disconnected from Databus");
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }


    }
}
