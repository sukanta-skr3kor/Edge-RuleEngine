//*********************************************************************************************
//* File             :   RedisDataBus.cs
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
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Sukanta.DataBus.Redis
{
    /// <summary>
    /// RedisDataBus Abstraction
    /// </summary>
    public class RedisDataBus : IDataBus, IRedisDbOperation
    {
        /// <summary>
        /// STREAM Name prefix
        /// </summary>
        private const string STREAM = "Stream";
        private const string SEPARATOR = "_";

        /// <summary>
        /// Nae for the DataBus
        /// </summary>
        string IDataBus.DataBusName { get; set; } = "Redis Pub-Sub";

        /// <summary>
        /// Comunication Bus type
        /// </summary>
        public CommunicationBus CommunicationMode => CommunicationBus.Redis;

        /// <summary>
        /// Redis Connection objet
        /// </summary>
        public IRedisConnection redisConnection { get; private set; }

        /// <summary>
        /// Is Connected to Redis
        /// </summary>
        public bool IsConnected => redisConnection.IsConnected();

        /// <summary>
        /// Redis connection string
        /// </summary>
        public string Server { get; set; } = "localhost:6379";

        /// <summary>
        /// RedisDataBus
        /// </summary>
        /// <param name="ServerConnString"></param>
        public RedisDataBus(string ServerConnString = null)
        {
            if (!string.IsNullOrEmpty(ServerConnString))
            {
                Server = ServerConnString;
            }

            redisConnection = new RedisConnection(Server);
        }

        /// <summary>
        /// Connect to databus
        /// </summary>
        /// <returns></returns>
        public bool TryConnect()
        {
            try
            {
                redisConnection = null;
                redisConnection = new RedisConnection(Server);
                return redisConnection.IsConnected();
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Insert To Redis DB 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task InsertToDbAsync(string id, string value)
        {
            try
            {
                IDatabase db = redisConnection.GetConnection().GetDatabase();
                await db.StringSetAsync(id, value);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get from Redis DB 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RedisValue> GetFromDbAsync(string id)
        {
            try
            {
                IDatabase db = redisConnection.GetConnection().GetDatabase();
                return await db.StringGetAsync(id);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get and Set Db
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<string> GetSetDbAsync(string id, string value)
        {
            try
            {
                IDatabase db = redisConnection.GetConnection().GetDatabase();
                return await db.StringGetSetAsync(id, value);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// AddStreamAsync
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="streamLength"></param>
        /// <param name="streamSourceType"></param>
        /// <returns></returns>
        public async Task AddStreamAsync(DataBusMessage dataBusMessage, int streamLength, StreamSourceType streamSourceType = StreamSourceType.DataService)
        {
            try
            {
                string streamSourceName = dataBusMessage.Id + SEPARATOR + streamSourceType.ToString() + STREAM;

                NameValueEntry[] values = new NameValueEntry[]
                {
                    new NameValueEntry("Id", dataBusMessage.Id),
                    new NameValueEntry("Value", dataBusMessage.Value),
                    new NameValueEntry("Time", dataBusMessage.Time.ToString())
                };

                IDatabase db = redisConnection.GetConnection().GetDatabase();
                await db.StreamAddAsync(streamSourceName, values, null, streamLength);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// AddStreamAsync
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="count"></param>
        /// <param name="streamSourceType"></param>
        /// <returns></returns>
        public async Task<StreamEntry[]> ReadStreamAsync(string streamSourceName, int count, StreamSourceType streamSourceType = StreamSourceType.DataService)
        {
            try
            {
                streamSourceName = streamSourceName + SEPARATOR + streamSourceType.ToString() + STREAM;

                IDatabase db = redisConnection.GetConnection().GetDatabase();

                return await db.StreamRangeAsync(streamSourceName, minId: "0-0", maxId: "+", count: count, messageOrder: Order.Descending);
            }
            catch
            {
                throw;
            }
        }

    }
}
