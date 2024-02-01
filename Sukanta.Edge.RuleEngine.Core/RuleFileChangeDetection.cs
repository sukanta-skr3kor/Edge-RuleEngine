//*********************************************************************************************
//* File             :   RuleFileChangeDetection.cs
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
using System.IO;

namespace Sukanta.Edge.RuleEngine.Core
{
    public static class RuleFileChangeDetection
    {
        public static bool isRuleFileChanged = false;

        public static void Detecthange(string filePath)
        {
            using FileSystemWatcher watcher = new FileSystemWatcher(filePath);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;

            watcher.EnableRaisingEvents = true;
            watcher.Filter = "*.json";
            watcher.IncludeSubdirectories = true;
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                isRuleFileChanged = true;
            }
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            isRuleFileChanged = true;
        }
    }

}
