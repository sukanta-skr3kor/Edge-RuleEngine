//*********************************************************************************************
//* File             :   Extensions.cs
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

using Microsoft.AspNetCore.Mvc;

namespace Sukanta.Edge.RuleEngine.Common
{
    /// <summary>
    /// Extension methods for the adapter service
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Send an Not ok result
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static IActionResult KnowOperationError(this ControllerBase controller, string message)
        {
            return controller.BadRequest(new ApiResponse() { IsOk = false, Message = message });
        }

        /// <summary>
        /// Generates a standard non content ok response
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static IActionResult OkResponse(this ControllerBase controller, string message)
        {
            return controller.Ok(new ApiResponse() { IsOk = true, Message = message });
        }

        /// <summary>
        /// Generates a standard content response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        internal static IActionResult Ok<T>(this ControllerBase controller, T content)
        {
            return controller.Ok(new ApiContentResponse<T>() { IsOk = true, Content = content });
        }

    }
}
