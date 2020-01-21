// Copyright (c) Microsoft. All rights reserved.

namespace Mmm.Platform.IoT.AsaManager.Services.Models.Rules
{
    public partial class RuleReferenceDataModel
    {
        private struct Condition
        {
            internal string Calculation { get; set; }
            internal string Field { get; set; }
            internal string Operator { get; set; }
            internal string Value { get; set; }
        }
    }
}
