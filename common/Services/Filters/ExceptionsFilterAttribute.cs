// <copyright file="ExceptionsFilterAttribute.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.Filters
{
    /// <summary>
    /// Detect all the unhandled exceptions returned by the API controllers
    /// and decorate the response accordingly, managing the HTTP status code
    /// and preparing a JSON response with useful error details.
    /// When including the stack trace, split the text in multiple lines
    /// for an easier parsing.
    /// </summary>
    public class ExceptionsFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger logger;

        public ExceptionsFilterAttribute(ILogger<ExceptionsFilterAttribute> logger)
        {
            this.logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ResourceNotFoundException)
            {
                context.Result = this.GetResponse(HttpStatusCode.NotFound, context.Exception);
            }
            else if (context.Exception is ConflictingResourceException
                     || context.Exception is ResourceOutOfDateException)
            {
                context.Result = this.GetResponse(HttpStatusCode.Conflict, context.Exception);
            }
            else if (context.Exception is BadRequestException
                     || context.Exception is InvalidInputException)
            {
                context.Result = this.GetResponse(HttpStatusCode.BadRequest, context.Exception);
            }
            else if (context.Exception is InvalidConfigurationException)
            {
                context.Result = this.GetResponse(HttpStatusCode.InternalServerError, context.Exception);
            }
            else if (context.Exception is NotAuthorizedException
                     || context.Exception is NoAuthorizationException)
            {
                context.Result = this.GetResponse(HttpStatusCode.Forbidden, context.Exception);
            }
            else if (context.Exception != null)
            {
                context.Result = this.GetResponse(HttpStatusCode.InternalServerError, context.Exception, true);
            }
            else
            {
                logger.LogError(context.Exception, "Unknown exception");
                base.OnException(context);
            }
        }

        private ObjectResult GetResponse(
            HttpStatusCode code,
            Exception e,
            bool stackTrace = false)
        {
            var error = new Dictionary<string, object>
            {
                ["Message"] = "An error has occurred.",
                ["ExceptionMessage"] = e.Message,
                ["ExceptionType"] = e.GetType().FullName,
            };

            if (stackTrace)
            {
                error["StackTrace"] = e.StackTrace?.Split(new[] { "\n" }, StringSplitOptions.None);

                if (e.InnerException != null)
                {
                    e = e.InnerException;
                    error["InnerExceptionMessage"] = e.Message;
                    error["InnerExceptionType"] = e.GetType().FullName;
                    error["InnerExceptionStackTrace"] = e.StackTrace?.Split(new[] { "\n" }, StringSplitOptions.None);
                }
            }

            var result = new ObjectResult(error);
            result.StatusCode = (int)code;
            result.Formatters.Add(new JsonOutputFormatter(new JsonSerializerSettings(), ArrayPool<char>.Shared));

            logger.LogError(e, "Status code was {statusCode}", result.StatusCode);

            return result;
        }
    }
}