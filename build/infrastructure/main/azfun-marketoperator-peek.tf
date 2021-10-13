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
module "azfun_marketoperator_peek" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.8.0"
  name                                      = "azfun-marketoperator-peek-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.postoffice.name
  location                                  = data.azurerm_resource_group.postoffice.location
  storage_account_access_key                = module.azfun_marketoperator_peek_stor.primary_access_key
  storage_account_name                      = module.azfun_marketoperator_peek_stor.name
  app_service_plan_id                       = module.azfun_marketoperator_peek_plan.id
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
    BlobStorageConnectionString         = module.stor_marketoperator_response.primary_connection_string
    BlobStorageContainerName            = module.container_postoffice_reply.name
    ServiceBusConnectionString          = azurerm_servicebus_queue_authorization_rule.sbnar_integrationevents_messagehub.primary_connection_string
    StorageAccountConnectionString      = module.stor_marketoperator_response.primary_connection_string
  }
  dependencies                              = [
    module.azfun_marketoperator_peek_plan.dependent_on,
    module.azfun_marketoperator_peek_stor.dependent_on,
    module.stor_marketoperator_response.dependent_on,
  ]
}

module "azfun_marketoperator_peek_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.8.0"
  name                = "asp-marketoperator-peek-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  location            = data.azurerm_resource_group.postoffice.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.postoffice.tags
}

module "azfun_marketoperator_peek_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.8.0"
  name                      = "stor${random_string.marketoperator.result}"
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  location                  = data.azurerm_resource_group.postoffice.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.postoffice.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "marketoperator" {
  length  = 10
  special = false
  upper   = false
}