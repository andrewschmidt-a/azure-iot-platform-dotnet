// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.Common.Services.Filters;
using Mmm.Platform.IoT.DeviceTelemetry.Services;
using Mmm.Platform.IoT.DeviceTelemetry.WebService.v1.Controllers.Helpers;
using Mmm.Platform.IoT.DeviceTelemetry.WebService.v1.Models;

namespace Mmm.Platform.IoT.DeviceTelemetry.WebService.v1.Controllers
{
    [Route("v1/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class MessagesController : Controller
    {
        private const int DEVICE_LIMIT = 1000;

        private readonly IMessages messageService;
        private readonly ILogger _logger;

        public MessagesController(
            IMessages messageService,
            ILogger<MessagesController> logger)
        {
            this.messageService = messageService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize("ReadAll")]
        public async Task<MessageListApiModel> GetAsync(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] string order,
            [FromQuery] int? skip,
            [FromQuery] int? limit,
            [FromQuery] string devices)
        {
            string[] deviceIds = new string[0];
            if (devices != null)
            {
                deviceIds = devices.Split(',');
            }

            return await this.ListMessagesHelper(from, to, order, skip, limit, deviceIds);
        }

        [HttpPost]
        [Authorize("ReadAll")]
        public async Task<MessageListApiModel> PostAsync([FromBody] QueryApiModel body)
        {
            string[] deviceIds = body.Devices == null
                ? new string[0]
                : body.Devices.ToArray();

            return await this.ListMessagesHelper(body.From, body.To, body.Order, body.Skip, body.Limit, deviceIds);
        }

        private async Task<MessageListApiModel> ListMessagesHelper(
            string from,
            string to,
            string order,
            int? skip,
            int? limit,
            string[] deviceIds)
        {
            DateTimeOffset? fromDate = DateHelper.ParseDate(from);
            DateTimeOffset? toDate = DateHelper.ParseDate(to);

            if (order == null) order = "asc";
            if (skip == null) skip = 0;
            if (limit == null) limit = 1000;

            // TODO: move this logic to the storage engine, depending on the
            // storage type the limit will be different. DEVICE_LIMIT is CosmosDb
            // limit for the IN clause.
            if (deviceIds.Length > DEVICE_LIMIT)
            {
                _logger.LogWarning("The client requested too many devices {count}", deviceIds.Length);
                throw new BadRequestException("The number of devices cannot exceed " + DEVICE_LIMIT);
            }

            MessageList messageList = await this.messageService.ListAsync(
                fromDate,
                toDate,
                order,
                skip.Value,
                limit.Value,
                deviceIds);

            return new MessageListApiModel(messageList);
        }
    }
}
