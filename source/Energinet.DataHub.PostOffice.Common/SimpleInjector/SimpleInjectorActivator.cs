﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Energinet.DataHub.PostOffice.Utilities;
using Microsoft.Azure.Functions.Worker;
using SimpleInjector;

namespace Energinet.DataHub.PostOffice.Common.SimpleInjector
{
    public class SimpleInjectorActivator : IFunctionActivator
    {
        private readonly Container _container;

        public SimpleInjectorActivator(Container container)
        {
            _container = container;
        }

        public object CreateInstance(Type instanceType, FunctionContext context)
        {
            Guard.ThrowIfNull(instanceType, nameof(instanceType));

            return _container.GetInstance(instanceType);
        }
    }
}
