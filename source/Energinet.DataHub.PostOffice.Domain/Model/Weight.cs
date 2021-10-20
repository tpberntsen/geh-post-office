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

#pragma warning disable CA2225

namespace Energinet.DataHub.PostOffice.Domain.Model
{
    public readonly struct Weight : IEquatable<Weight>
    {
        public Weight(int value)
        {
            Value = value;
        }

        public int Value { get; init; }

        public static Weight operator +(Weight left, Weight right)
        {
            return new Weight(left.Value + right.Value);
        }

        public static Weight operator -(Weight left, Weight right)
        {
            return new Weight(left.Value - right.Value);
        }

        public static bool operator <(Weight left, Weight right)
        {
            return left.Value < right.Value;
        }

        public static bool operator <=(Weight left, Weight right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >(Weight left, Weight right)
        {
            return left.Value > right.Value;
        }

        public static bool operator >=(Weight left, Weight right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator ==(Weight left, Weight right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Weight left, Weight right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is Weight w && w.Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(Weight other)
        {
            return other.Value == Value;
        }
    }
#pragma warning restore CA2225
}
