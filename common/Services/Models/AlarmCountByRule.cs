// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    public class AlarmCountByRule
    {
        public int Count { get; set; }
        public string Status { get; set; }
        public DateTimeOffset MessageTime { get; set; }
        public Rule Rule { get; set; }

        public AlarmCountByRule(
            int count,
            string status,
            DateTimeOffset messageTime,
            Rule rule)
        {
            this.Count = count;
            this.Status = status;
            this.MessageTime = messageTime;
            this.Rule = rule;
        }
    }
}
