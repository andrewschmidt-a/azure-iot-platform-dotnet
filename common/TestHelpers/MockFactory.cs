// Copyright (c) Microsoft. All rights reserved.

using Mmm.Platform.IoT.Common.Services.Wrappers;
using Moq;

namespace Mmm.Platform.IoT.Common.TestHelpers
{
    public class MockFactory<T> : IFactory<T>
        where T : class
    {
        private readonly Mock<T> mock;

        public MockFactory(Mock<T> mock)
        {
            this.mock = mock;
        }

        public T Create()
        {
            return this.mock.Object;
        }
    }
}
