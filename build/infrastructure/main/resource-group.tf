# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
data "azurerm_resource_group" "postoffice" {
  name = var.resource_group_name
}

data "azurerm_servicebus_queue_authorization_rule" "sbnar_integrationevents_messagehub" {
  name                = var.azure_sharedresources_service_bus_namespace_auth_rule_name
  resource_group_name = var.azure_sharedresources_resource_group_name
  namespace_name      = var.azure_sharedresources_service_bus_namespace_name
}

data "azurerm_servicebus_namespace_auth_rule" "sbq_messagehub_dataavailable" {
  name                = var.azure_sharedresources_sbq_messagehub_dataavailable
  resource_group_name = var.azure_sharedresources_resource_group_name
  namespace_name      = var.azure_sharedresources_service_bus_namespace_name
}