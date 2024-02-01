//*********************************************************************************************
//* File             :   RuleEngineSettings.cs
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

namespace Sukanta.Edge.RuleEngine.Model
{
    /// <summary>
    /// RuleEngine Settings
    /// </summary>
    public class RuleEngineSettings
    {
        /// <summary>
        /// Notification to server ?
        /// </summary>
        public bool NotificationEnabled { get; set; }

        /// <summary>
        /// Http port number
        /// </summary>
        public int HttpPort { get; set; }

        /// <summary>
        /// When set to true https connection and redirection will be enabled
        /// </summary>
        public bool UseHttps { get; set; }

        /// <summary>
        /// Enable http
        /// </summary>
        public bool UseHttp { get; set; }

        /// <summary>
        /// Https port number
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Enable mutual TLS
        /// </summary>
        public bool UseMutualTls { get; set; }

        /// <summary>
        /// TLS port number
        /// </summary>
        public int MutualTlsPort { get; set; }

        /// <summary>
        /// Binding IP Address
        /// </summary>
        public string Binding { get; set; } = "localhost";

        /// <summary>
        /// Signalr Hub Url on servr
        /// </summary>
        public string SignalrHubUrl { get; set; }

        /// <summary>
        /// Use certificate ?
        /// </summary>
        public bool UseCertificate { get; set; }

        /// <summary>
        /// Certificate file
        /// </summary>
        public string CertificateFile { get; set; }

        /// <summary>
        /// Certificate Password
        /// </summary>
        public string CertificatePassword { get; set; }

        /// <summary>
        /// SimpleRule
        /// </summary>
        public SimpleRule SimpleRule { get; set; }

        /// <summary>
        /// ComplexRule
        /// </summary>
        public ComplexRule ComplexRule { get; set; }

        /// <summary>
        /// DataBus Settings
        /// </summary>
        public DataBusSettings DataBusSettings { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        public Parameters[] Parameters { get; set; }
    }


    /// <summary>
    /// SimpleRule engine settings
    /// </summary>
    public class SimpleRule
    {
        /// <summary>
        /// is rule engine enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Rule File location
        /// </summary>
        public string RuleFilePath { get; set; }

        /// <summary>
        /// Rule Execution Interval
        /// </summary>
        public int RuleExecutionSeconds { get; set; } = 10; //default 10 second
    }


    /// <summary>
    /// ComplexRule engine settings
    /// </summary>
    public class ComplexRule
    {
        /// <summary>
        /// is rule engine enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Number of parametrs max the rule engine to operate on
        /// </summary>
        public int NumberOfParamterToAnalyze { get; set; }

        /// <summary>
        /// Rule File location
        /// </summary>
        public string RuleFilePath { get; set; }

        /// <summary>
        /// Rule Execution Interval
        /// </summary>
        public int RuleExecutionSeconds { get; set; } = 10; //default 10 second
    }


    /// <summary>
    /// DataBus Settings
    /// </summary>
    public class DataBusSettings
    {
        /// <summary>
        /// Databus type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// DataBus Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is Databus enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Redis Server url
        /// </summary>
        public string Server { get; set; } = "localhost:6379";

        /// <summary>
        /// Subscribe topic name
        /// </summary>
        public string SubscribeTopic { get; set; } = "datamessage";

        /// <summary>
        /// Publish topic name
        /// </summary>
        public string PublishTopic { get; set; } = "commandmessage";

        /// <summary>
        /// If redis DB store enabled ?
        /// </summary>
        public bool DBPersistEnabled { get; set; }

        /// <summary>
        /// If redis stream enabled ?
        /// </summary>
        public bool DBStreamEnabled { get; set; }

        /// <summary>
        /// Publish command messages ?
        /// </summary>
        public bool CommandMessageEnabled { get; set; }

        /// <summary>
        /// Redis Stream length
        /// </summary>
        public int StreamLength { get; set; } = 1000;

        /// <summary>
        /// Data read interval from databus
        /// </summary>
        public int DataReadIntervalSeconds { get; set; }
    }


    /// <summary>
    /// parameters
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// ID of the parameter
        /// </summary>
        public string Id { get; set; }
    }
}
