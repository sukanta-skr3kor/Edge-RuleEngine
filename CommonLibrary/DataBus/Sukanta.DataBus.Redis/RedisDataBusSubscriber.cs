//*********************************************************************************************
//* File             :   RedisDataBusSubscriber.cs
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
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Sukanta.DataBus.Redis
{
    /// <summary>
    /// IRedisDataBusSubscriber inerface
    /// </summary>
    public interface IRedisDataBusSubscriber : IDataBusSubscribe
    {
        /// <summary>
        /// Data received from publisher apps
        /// </summary>
        BlockingCollection<RedisValue> DataBusMessages { get; }

        /// <summary>
        ///  If data available in DataBus
        /// </summary>
        bool HasData();
    }

    /// <summary>
    /// Redis Data Subscriber
    /// </summary>
    public class RedisDataBusSubscriber : RedisDataBus, IRedisDataBusSubscriber
    {
        /// <summary>
        /// Data received from publisher apps
        /// </summary>
        public BlockingCollection<RedisValue> DataBusMessages { get; }

        /// <summary>
        ///  If data available in DataBus
        /// </summary>
        public bool HasData() => DataBusMessages?.Count > 0;

        /// <summary>
        /// Subscribe Toic
        /// </summary>
        public string SubscribeTopic { get; set; }

        /// <summary>
        /// Redis DataBus Subscriber
        /// </summary>
        /// <param name="ServerConnString"></param>
        public RedisDataBusSubscriber(string ServerConnString = null, string subscribeToic = null) : base(ServerConnString)
        {
            DataBusMessages = new BlockingCollection<RedisValue>();
            SubscribeTopic = subscribeToic;
        }

        /// <summary>
        /// SubscribeToDataBusAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Task SubscribeToDataBusAsync<T>(string topic = null) where T : IDataBusMessage
        {
            try
            {
                if (!string.IsNullOrEmpty(topic))
                {
                    SubscribeTopic = topic;
                }

                IConnectionMultiplexer connection = redisConnection.GetConnection();

                RedisChannel subscribeChannel = new RedisChannel(SubscribeTopic, RedisChannel.PatternMode.Auto);

                ISubscriber subscriber = connection.GetSubscriber();

                subscriber.Subscribe(subscribeChannel, (channel, message) =>
                {
                    DataBusMessages.TryAdd(message);
                });

                return Task.CompletedTask;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// SubscribeToDataBusAsync
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Task SubscribeToDataBusAsync(string topic)
        {
            try
            {
                IConnectionMultiplexer connection = redisConnection.GetConnection();

                RedisChannel subscribeChannel = new RedisChannel(topic, RedisChannel.PatternMode.Auto);

                ISubscriber subscriber = connection.GetSubscriber();

                subscriber.Subscribe(subscribeChannel, (channel, message) =>
                {
                    DataBusMessages.TryAdd(message);
                });

                return Task.CompletedTask;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        /// <summary>
        /// GetData as DataBusMessage type from adapters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public DataBusMessage GetDataBusMessage()
        {
            RedisValue redisValue;
            DataBusMessage dataBusMessage = new DataBusMessage();

            try
            {
                if (DataBusMessages.TryTake(out redisValue))
                {
                    return JsonConvert.DeserializeObject<DataBusMessage>(redisValue);
                }

                return dataBusMessage;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get jon message 
        /// </summary>
        /// <returns></returns>
        public string GetDataBusMessageJson()
        {
            RedisValue redisValue;

            try
            {
                if (DataBusMessages.TryTake(out redisValue))
                {
                    return JsonConvert.SerializeObject(redisValue);
                }

                return "#NODATA";
            }
            catch
            {
                throw;
            }
        }
    }
}
