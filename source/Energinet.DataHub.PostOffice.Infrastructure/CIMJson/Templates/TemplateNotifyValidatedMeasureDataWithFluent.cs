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
    internal class TemplateNotifyValidatedMeasureDataWithFluent : BaseJsonTemplateFluent
    {
        public TemplateNotifyValidatedMeasureDataWithFluent()
            : base("NotifyValidatedMeasureData_MarketDocument") { }
        protected override void Generate(Utf8JsonWriter jsonWriter, XmlReader reader)
        {
          CimJsonBuilder
                .Create()
                .WithXmlReader(
                    x => x
                    .AddString(builder => builder
                        .WithName(ElementNames.MRid))
                    .AddString(builder => builder
                        .WithName(ElementNames.Type)
                        .WithValueWrappedInProperty())
                    .AddString(builder => builder
                        .WithName(ElementNames.ProcessProcessType)
                        .WithValueWrappedInProperty())
                    .AddString(builder => builder
                        .WithName(ElementNames.BusinessSectorType)
                        .IsOptional()
                        .WithValueWrappedInProperty())
                    .AddString(builder => builder
                        .WithName(ElementNames.SenderMarketParticipantmRid)
                        .WithValueWrappedInProperty()
                        .WithAttributes(abuilder => abuilder
                            .AddString(sbuilder => sbuilder
                                .WithName(ElementNames.CodingScheme))))
                    .AddString(builder => builder
                        .WithName(ElementNames.SenderMarketParticipantmarketRoletype)
                        .WithValueWrappedInProperty())
                    .AddString(builder => builder
                        .WithName(ElementNames.ReceiverMarketParticipantmRid)
                        .WithAttributes(abuilder => abuilder
                            .AddString(sbuilder => sbuilder
                                .WithName(ElementNames.CodingScheme)))
                        .WithValueWrappedInProperty())
                    .AddString(builder => builder
                        .WithName(ElementNames.ReceiverMarketParticipantmarketRoletype)
                        .WithValueWrappedInProperty())
                    .AddString(builder => builder
                        .WithName(ElementNames.CreatedDateTime))
                    .AddArray(abuilder => abuilder
                        .WithName(ElementNames.Series)
                        .IsOptional()
                        .AddString(s => s
                            .WithName(ElementNames.MRid))
                        .AddString(s => s
                            .WithName(ElementNames.OriginalTransactionIdReferenceSeriesmRid)
                            .IsOptional())
                        .AddString(s => s
                            .WithName(ElementNames.MarketEvaluationPointmRid)
                            .WithAttributes(a => a
                                .AddString(ab => ab
                                    .WithName(ElementNames.CodingScheme)))
                            .WithValueWrappedInProperty())
                        .AddString(s => s
                            .WithName(ElementNames.MarketEvaluationPointtype)
                            .WithValueWrappedInProperty())
                        .AddString(s => s
                            .WithName(ElementNames.RegistrationDateAndOrTimedateTime)
                            .IsOptional())
                        .AddString(s => s
                            .WithName(ElementNames.InDomainmRid)
                            .IsOptional()
                            .WithAttributes(a => a
                                .AddString(ab => ab
                                    .WithName(ElementNames.CodingScheme)))
                            .WithValueWrappedInProperty())
                        .AddString(s => s
                            .WithName(ElementNames.OutDomainmRid)
                            .IsOptional()
                            .WithAttributes(a => a
                                .AddString(ab => ab
                                    .WithName(ElementNames.CodingScheme)))
                            .WithValueWrappedInProperty())
                        .AddString(s => s
                            .WithName(ElementNames.Product)
                            .IsOptional())
                        .AddString(s => s
                            .WithName(ElementNames.MeasureUnitname)
                            .WithValueWrappedInProperty())
                        .AddNested(n => n
                            .WithName(ElementNames.Period)
                            .AddString(s => s
                                .WithName(ElementNames.Resolution))
                            .AddNested(n2 => n2
                                .WithName(ElementNames.TimeInterval)
                                .AddString(s2 => s2
                                    .WithName(ElementNames.Start)
                                    .WithValueWrappedInProperty())
                                .AddString(s2 => s2
                                    .WithName(ElementNames.End)
                                    .WithValueWrappedInProperty()))
                            .AddNested(n2 => n2
                                .WithName(ElementNames.Point)
                                .AddInteger(s2 => s2
                                    .WithName(ElementNames.Position)
                                    .WithValueWrappedInProperty())
                                .AddInteger(s2 => s2
                                    .WithName(ElementNames.Quantity)
                                    .IsOptional())
                                .AddString(s2 => s2
                                    .WithName(ElementNames.Quality)
                                    .WithValueWrappedInProperty()
                                    .IsOptional()))))
                    .AddString(s => s
                        .WithName("jjtest")),
                    reader)
                .Build(jsonWriter);
        }

        private static class ElementNames
        {
            public const string MRid = "mRID";
            public const string Series = "Series";
            public const string Type = "type";
            public const string ProcessProcessType = "process.processType";
            public const string BusinessSectorType = "businessSector.type";
            public const string SenderMarketParticipantmRid = "sender_MarketParticipant.mRID";
            public const string SenderMarketParticipantmarketRoletype = "sender_MarketParticipant.marketRole.type";
            public const string ReceiverMarketParticipantmRid = "receiver_MarketParticipant.mRID";
            public const string ReceiverMarketParticipantmarketRoletype = "receiver_MarketParticipant.marketRole.type";
            public const string CreatedDateTime = "createdDateTime";
            public const string OriginalTransactionIdReferenceSeriesmRid = "originalTransactionIDReference_Series.mRID";
            public const string RegistrationDateAndOrTimedateTime = "registration_DateAndOrTime.dateTime";
            public const string InDomainmRid = "in_Domain.mRID";
            public const string OutDomainmRid = "out_Domain.mRID";
            public const string Product = "product";
            public const string MeasureUnitname = "measure_Unit.name";
            public const string Period = "Period";
            public const string Resolution = "resolution";
            public const string TimeInterval = "timeInterval";
            public const string Point = "Point";
            public const string MarketEvaluationPointmRid = "marketEvaluationPoint.mRID";
            public const string MarketEvaluationPointtype = "marketEvaluationPoint.type";
            public const string Start = "start";
            public const string End = "end";
            public const string Position = "position";
            public const string Quantity = "quantity";
            public const string Quality = "quality";
            public const string CodingScheme = "codingScheme";
        }
    }
}
