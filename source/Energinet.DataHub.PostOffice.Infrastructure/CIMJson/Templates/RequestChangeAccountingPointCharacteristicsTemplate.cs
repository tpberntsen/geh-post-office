// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System.Text.Json;
using System.Xml;
using Energinet.DataHub.PostOffice.Infrastructure.CIMJson.FluentCimJson.Builders.General;

namespace Energinet.DataHub.PostOffice.Infrastructure.CIMJson.Templates
{
    internal class RequestChangeAccountingPointCharacteristicsTemplate : BaseJsonTemplateFluent
    {
        public RequestChangeAccountingPointCharacteristicsTemplate()
            : base("RequestChangeAccountingPointCharacteristics_MarketDocument") { }
        protected override void Generate(Utf8JsonWriter jsonWriter, XmlReader reader)
        {
          CimJsonBuilder
                .Create()
                .WithXmlReader(
                    x => x
                            .AddString(sb => sb
                                .WithName(ElementNames.MarketDocument.MRid)
                                .WithAttributes(ab => ab
                                    .AddString(asb => asb
                                        .WithName(ElementNames.Attributes.CodingScheme)))
                                .IsOptional()),
                    reader)
                .Build(jsonWriter);
        }

        private static class ElementNames
        {
            public static class Attributes
            {
                public const string CodingScheme = "codingScheme";
            }

            public static class MarketDocument
            {
                public const string MRid = "mRID";
                public const string Type = "type";
                public const string ProcessProcessType = "process.processType";
                public const string BusinessSectorType = "businessSector.type";
                public const string SenderMarketParticipantmRid = "sender_MarketParticipant.mRID";
                public const string SenderMarketParticipantmarketRoletype = "sender_MarketParticipant.marketRole.type";
                public const string ReceiverMarketParticipantmRid = "receiver_MarketParticipant.mRID";
                public const string ReceiverMarketParticipantmarketRoletype = "receiver_MarketParticipant.marketRole.type";
                public const string CreatedDateTime = "createdDateTime";
                public const string ReasonCode = "reason.code";
                public const string MktActivityRecordElement = "MktActivityRecord";
            }

            public static class MktActivityRecord
            {
                public const string MRid = "mRID";
                public const string BusinessProcessReferenceMktActivityRecordmRid = "businessProcessReference_MktActivityRecord.mRID";
                public const string ValidityStartDateAndOrTimedateTime = "validityStart_DateAndOrTime.dateTime";
                public const string MarketEvaluationPoint = "MarketEvaluationPoint";
            }

            public static class MarketEvaluationPoint
            {
                public const string MRid = "mRID";
                public const string SettlementMethod = "settlementMethod";
                public const string MeteringMethod = "meteringMethod";
                public const string ConnectionState = "connectionState";
                public const string ReadCycle = "readCycle";
                public const string NetSettlementGroup = "netSettlementGroup";
                public const string NextReadingDate = "nextReadingDate";
                public const string MeteringGridAreaDomainmRid = "meteringGridArea_Domain.mRID";
                public const string InMeteringGridAreaDomainmRid = "inMeteringGridArea_Domain.mRID";
                public const string OutMeteringGridAreaDomainmRid = "outMeteringGridArea_Domain.mRID";
                public const string LinkedMarketEvaluationPointmRid = "linked_MarketEvaluationPoint.mRID";
                public const string PhysicalConnectionCapacity = "physicalConnectionCapacity";
                public const string MPConnectionType = "mPConnectionType";
                public const string DisconnectionMethod = "disconnectionMethod";
                public const string AssetMktPsrTypepsrType = "asset_MktPSRType.psrType";
                public const string ProductionObligation = "productionObligation";
                public const string ContractedConnectionCapacity = "contractedConnectionCapacity";
                public const string RatedCurrent = "ratedCurrent";
                public const string MetermRid = "meter.mRID";
                public const string SeriesElement = "Series";
                public const string Description = "description";
                public const string UsagePointLocationgeoInfoReference = "usagePointLocation.geoInfoReference";
                public const string UsagePointLocationmainAddress = "usagePointLocation.mainAddress";
                public const string UsagePointLocationactualAddressIndicator = "usagePointLocation.actualAddressIndicator";
                public const string ParentMarketEvaluationPointmRid = "parent_MarketEvaluationPoint.mRID";
            }

            public static class Series
            {
                public const string Product = "product";
                public const string EstimatedAnnualVolumeQuantityquantity = "estimatedAnnualVolume_Quantity.quantity";
                public const string QuantityMeasureUnitname = "quantity_Measure_Unit.name";
            }

            public static class StreetAddress
            {
                public const string StreetDetailElement = "streetDetail";
                public const string TownDetailElement = "townDetail";
                public const string PostalCode = "postalCode";
                public const string POBox = "poBox";
                public const string Language = "language";
            }

            public static class TownDetail
            {
                public const string Code = "code";
                public const string Name = "name";
                public const string Section = "section";
                public const string Country = "country";
            }

            public static class StreetDetail
            {
                public const string Code = "code";
                public const string Name = "name";
                public const string Number = "number";
                public const string FloorIdentification = "floorIdentification";
                public const string SuiteNumber = "suiteNumber";
            }
        }
    }
}
