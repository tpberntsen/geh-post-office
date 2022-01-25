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
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;

namespace Energinet.DataHub.PostOffice.EntryPoint.MarketOperator
{
    public sealed class ActorProvider : IActorProvider
    {
        private readonly ActorDbConfig _actorDbConfig;

        public ActorProvider(ActorDbConfig actorDbConfig)
        {
            _actorDbConfig = actorDbConfig;
        }

        public async Task<Actor> GetActorAsync(Guid actorId)
        {
            const string param = "ACTOR_ID";
            const string query = @"SELECT TOP 1 [Id]
                            ,[IdentificationType]
                            ,[IdentificationNumber]
                            ,[Roles]
                        FROM  [dbo].[ActorInfo]
                        WHERE Id = @" + param;

            await using var connection = new SqlConnection(_actorDbConfig.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection)
            {
                Parameters = { new SqlParameter(param, actorId) }
            };

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var record = ((IDataRecord)reader)!;

                return new Actor(
                    record.GetGuid(0),
                    record.GetInt32(1).ToString(CultureInfo.InvariantCulture),
                    record.GetString(2),
                    record.GetString(3));
            }

            throw new InvalidOperationException("Actor not found");
        }
    }
}
