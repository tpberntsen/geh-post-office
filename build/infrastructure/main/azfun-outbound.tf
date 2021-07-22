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
module "azfun_outbound" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.3.0"
  name                                      = "azfun-outbound-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.postoffice.name
  location                                  = data.azurerm_resource_group.postoffice.location
  storage_account_access_key                = module.azfun_outbound_stor.primary_access_key
  storage_account_name                      = module.azfun_outbound_stor.name
  app_service_plan_id                       = module.azfun_outbound_plan.id
  application_insights_instrumentation_key  = module.appi_postoffice.instrumentation_key
  tags                                      = data.azurerm_resource_group.postoffice.tags
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE     = true
    WEBSITE_RUN_FROM_PACKAGE            = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = true
    FUNCTIONS_WORKER_RUNTIME            = "dotnet-isolated"
    # Endregion
    MESSAGES_DB_CONNECTION_STRING       = local.message_db_connection_string
    MESSAGES_DB_NAME                    = azurerm_cosmosdb_sql_database.db.name
    MESSAGE_DB_CONTAINERS               = "${azurerm_cosmosdb_sql_container.collection_marketdata.name},${azurerm_cosmosdb_sql_container.collection_timeseries.name},${azurerm_cosmosdb_sql_container.collection_aggregations.name},${azurerm_cosmosdb_sql_container.collection_dataavailable.name}"
    SCHEMAS_STORAGE_CONNECTION_STRING   = module.stor_schemas.primary_access_key
    SCHEMAS_STORAGE_CONTAINER_NAME      = module.container_schemas.name
    "BlobStorageConnectionString"       = module.stor_outbound_getmessage.primary_access_key
    "BlobStorageContainerName"          = module.container_getmessage_reply.name
    "ServiceBusConnectionString"        = module.sbn_outbound.name
    "ServiceBus_DataRequest_Return_Queue" = module.sbq_messagereply.name
    "StorageAccountConnectionString"      = module.stor_outbound_getmessage.primary_access_key
  }
  dependencies                              = [
    module.azfun_outbound_plan.dependent_on,
    module.azfun_outbound_stor.dependent_on,
    module.stor_schemas.dependent_on,
    module.stor_outbound_getmessage.dependent_on,
  ]
}

module "azfun_outbound_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.3.0"
  name                = "asp-outbound-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  location            = data.azurerm_resource_group.postoffice.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.postoffice.tags
}

module "azfun_outbound_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.3.0"
  name                      = "stor${random_string.outbound.result}"
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  location                  = data.azurerm_resource_group.postoffice.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.postoffice.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "outbound" {
  length  = 10
  special = false
  upper   = false
}