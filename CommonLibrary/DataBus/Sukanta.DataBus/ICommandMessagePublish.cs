//*********************************************************************************************
//* File             :   ICommandMessagePublish.cs
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
    public interface ICommandMessagePublish
    {
        /// <summary>
        /// PublishCommandMessageAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandMessage"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task PublishCommandMessageAsync<T>(T commandMessage, string topic) where T : CommandMessage;
    }
}