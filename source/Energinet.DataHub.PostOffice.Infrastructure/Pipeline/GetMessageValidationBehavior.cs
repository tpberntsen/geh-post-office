// Copyright 2020 Energinet DataHub A/S
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Energinet.DataHub.PostOffice.Infrastructure.Pipeline
{
    public class GetMessagePipelineValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IValidator<TRequest> _validator;

        public GetMessagePipelineValidationBehavior(IValidator<TRequest> validator)
            => _validator = validator;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (next is null) throw new ArgumentNullException(nameof(next));

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(
                    $"Cannot validate request because: {validationResult.Errors.First()}");
            }

            return await next().ConfigureAwait(false);
        }
    }
}
