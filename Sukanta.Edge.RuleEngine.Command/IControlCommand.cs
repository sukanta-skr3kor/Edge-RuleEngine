//*********************************************************************************************
//* File             :   IControlCommand.cs
//* Author           :   Rout, Sukanta 
//* Date             :   10/1/2024
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
using System.Threading.Tasks;

namespace Sukanta.Edge.RuleEngine.CommandControl
{
    public interface IControlCommand
    {
        Task SendCommandAsync(CommandMessage commandMessage);
    }
}