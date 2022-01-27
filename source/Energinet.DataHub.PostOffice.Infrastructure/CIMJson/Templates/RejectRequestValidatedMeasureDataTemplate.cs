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
    internal class RejectRequestValidatedMeasureDataTemplate : BaseJsonTemplate
    {
        public RejectRequestValidatedMeasureDataTemplate()
            : base("RejectRequestValidatedMeasureData_MarketDocument") { }
        protected override void Generate(Utf8JsonWriter jsonWriter, XmlReader reader)
        {
          CimJsonBuilder
                .Create()
                .WithXmlReader(
                    x => x
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.MRid))
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.Type)
                            .WithValueWrappedInProperty())
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.ProcessProcessType)
                            .WithValueWrappedInProperty())
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.BusinessSectorType)
                            .IsOptional()
                            .WithValueWrappedInProperty())
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.SenderMarketParticipantmRid)
                            .WithValueWrappedInProperty()
                            .WithAttributes(ab => ab
                                .AddString(asb => asb
                                    .WithName(ElementNames.Attributes.CodingScheme))))
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.SenderMarketParticipantmarketRoletype))
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.ReceiverMarketParticipantmRid)
                            .WithValueWrappedInProperty()
                            .WithAttributes(ab => ab
                                .AddString(asb => asb
                                    .WithName(ElementNames.Attributes.CodingScheme))))
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.ReceiverMarketParticipantmarketRoletype)
                            .WithValueWrappedInProperty())
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.CreatedDateTime))
                        .AddString(sb => sb
                            .WithName(ElementNames.MarketDocument.ReasonCode))
                        .AddArray(ab => ab
                            .WithName(ElementNames.MarketDocument.SeriesElement)
                            .AddString(sb => sb
                                .WithName(ElementNames.Series.MRid))
                            .AddString(sb => sb
                                .WithName(ElementNames.Series.OriginalTransactionIdReferenceSeries)
                                .IsOptional())
                            .AddArray(iarb => iarb
                                .WithName(ElementNames.Series.ReasonElement)
                                .AddString(sb => sb
                                    .WithName(ElementNames.Reason.Code))
                                .AddString(sb => sb
                                    .WithName(ElementNames.Reason.Text)
                                    .IsOptional()))
                            .AddString(sb => sb
                                .WithName(ElementNames.Series.MarketEvaluationPointmRid)
                                .WithAttributes(ab => ab
                                    .AddString(asb => asb
                                        .WithName(ElementNames.Attributes.CodingScheme)))
                                .IsOptional())),
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
                public const string SeriesElement = "Series";
            }

            public static class Series
            {
                public const string MRid = "mRID";
                public const string OriginalTransactionIdReferenceSeries = "originalTransactionIDReference_Series.mRID";
                public const string MarketEvaluationPointmRid = "marketEvaluationPoint.mRID";
                public const string ReasonElement = "Reason";
            }

            public static class Reason
            {
                public const string Code = "code";
                public const string Text = "text";
            }
        }
    }
}
