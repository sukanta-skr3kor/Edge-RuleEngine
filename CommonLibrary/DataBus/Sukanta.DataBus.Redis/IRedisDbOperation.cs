//*********************************************************************************************
//* File             :   IRedisDbOperation.cs
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
    /// DB contracts
    /// </summary>
    public interface IRedisDbOperation
    {
        //Key-Value read/write
        Task InsertToDbAsync(string id, string value);
        Task<RedisValue> GetFromDbAsync(string id);
        Task<string> GetSetDbAsync(string id, string value);

        //Stream read/write
        Task AddStreamAsync(DataBusMessage dataBusMessage, int streamLength, StreamSourceType streamSourceType);
        Task<StreamEntry[]> ReadStreamAsync(string streamSourceName, int count, StreamSourceType streamSourceType);
    }

    /// <summary>
    /// Stream Source Types
    /// </summary>
    public enum StreamSourceType
    {
        RuleEngine,
        DataService
    }
}
