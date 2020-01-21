// -----------------------------------------------------------------------
// <copyright file="EmptyEntitiesException.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Mmm.Platform.IoT.AsaManager.Services.Exceptions
{
    public class EmptyEntitiesException : Exception
    {
        public EmptyEntitiesException()
            : base()
        {
        }

        public EmptyEntitiesException(string message)
            : base(message)
        {
        }

        public EmptyEntitiesException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}