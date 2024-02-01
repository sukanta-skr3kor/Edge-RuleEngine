//*********************************************************************************************
//* File             :   CommandMessage.cs
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
    /// Command Message
    /// </summary>
    public class CommandMessage
    {
        public string CommandType { get; set; }

        public string MachineId { get; set; }

        public string ParameterId { get; set; }

        public string CommandAction { get; set; }
    }

    /// <summary>
    /// Actions on parameter/Machine
    /// </summary>
    public enum CommandAction
    {
        ACK,//ack
        OK, //ok
        NOK, //not ok
        EMERGENCYSTOP,//Machine stop
    }

    /// <summary>
    /// CommandType
    /// </summary>
    public enum CommandType
    {
        Application,//command for application
        Device //command for device
    }
}
