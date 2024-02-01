//*********************************************************************************************
//* File             :   AssemblyContext.cs
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
using System.Reflection;
using System.Runtime.Loader;

namespace Sukanta.Reflection
{
    /// <summary>
    /// AssemblyContext to load binary on context
    /// </summary>
    public class AssemblyContext : AssemblyLoadContext
    {
        /// <summary>
        /// 
        /// </summary>
        public AssemblyContext()
        {
        }

        /// <summary>
        /// Load assembly
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
