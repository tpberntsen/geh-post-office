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
module "azfun_inbound" {
  source                                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/function-app?ref=1.3.0"
  name                                      = "azfun-inbound-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.postoffice.name
  location                                  = data.azurerm_resource_group.postoffice.location
  storage_account_access_key                = module.azfun_inbound_stor.primary_access_key
  storage_account_name                      = module.azfun_inbound_stor.name
  app_service_plan_id                       = module.azfun_inbound_plan.id
  application_insights_instrumentation_key  = module.appi_postoffice.instrumentation_key
  tags                                      = data.azurerm_resource_group.postoffice.tags
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                   = true
    WEBSITE_RUN_FROM_PACKAGE                          = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE               = true
    FUNCTIONS_WORKER_RUNTIME                          = "dotnet"
    # Endregion
    MESSAGES_DB_CONNECTION_STRING                     = local.message_db_connection_string,
    MESSAGES_DB_NAME                                  = azurerm_cosmosdb_sql_database.db.name,
    INBOUND_QUEUE_MARKETDATA_TOPIC_NAME               = module.sbt_marketdata.name
    INBOUND_QUEUE_MARKETDATA_SUBSCRIPTION_NAME        = module.sbtaur_marketdata_subscription.name
    INBOUND_QUEUE_CONNECTION_STRING                   = module.sbnar_inbound_listener.primary_connection_string
    INBOUND_QUEUE_AGGREGATIONS_TOPIC_NAME             = module.sbt_aggregations.name
    INBOUND_QUEUE_AGGREGATIONS_SUBSCRIPTION_NAME      = module.sbtaur_aggregations_subscription.name
    INBOUND_QUEUE_TIMESERIES_TOPIC_NAME               = module.sbt_timeseries.name
    INBOUND_QUEUE_TIMESERIES_SUBSCRIPTION_NAME        = module.sbtaur_timeseries_subscription.name
  }
  dependencies                              = [
    module.appi_postoffice.dependent_on,
    module.azfun_inbound_plan.dependent_on,
    module.azfun_inbound_stor.dependent_on,
    module.sbt_marketdata.dependent_on,
    module.sbtaur_marketdata_subscription.dependent_on,
    module.sbt_aggregations.dependent_on,
    module.sbtaur_aggregations_subscription.dependent_on,
    module.sbt_timeseries.dependent_on,
    module.sbtaur_timeseries_subscription.dependent_on,
  ]
}

module "azfun_inbound_plan" {
  source              = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/app-service-plan?ref=1.3.0"
  name                = "asp-inbound-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  location            = data.azurerm_resource_group.postoffice.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.postoffice.tags
}

module "azfun_inbound_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/storage-account?ref=1.3.0"
  name                      = "stor${random_string.inbound.result}"
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  location                  = data.azurerm_resource_group.postoffice.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.postoffice.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "inbound" {
  length  = 10
  special = false
  upper   = false
}