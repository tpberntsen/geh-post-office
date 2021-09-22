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
module "azfun_dataavailable" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.8.0"
  name                                      = "azfun-dataavailable-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.postoffice.name
  location                                  = data.azurerm_resource_group.postoffice.location
  storage_account_access_key                = module.azfun_dataavailable_stor.primary_access_key
  storage_account_name                      = module.azfun_dataavailable_stor.name
  app_service_plan_id                       = module.azfun_dataavailable_plan.id
  application_insights_instrumentation_key  = module.appi_postoffice.instrumentation_key
  tags                                      = data.azurerm_resource_group.postoffice.tags
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                   = true
    WEBSITE_RUN_FROM_PACKAGE                          = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE               = true
    FUNCTIONS_WORKER_RUNTIME                          = "dotnet-isolated"
    # Endregion
    MESSAGES_DB_CONNECTION_STRING                     = local.message_db_connection_string,
    MESSAGES_DB_NAME                                  = azurerm_cosmosdb_sql_database.db.name,
    DATAAVAILABLE_QUEUE_CONNECTION_STRING             = module.sbnar_subdomain_listener.primary_connection_string
    DATAAVAILABLE_QUEUE_NAME                          = module.sbq_dataavailable.name
    ServiceBusConnectionString                        = module.sbnar_subdomain_listener.primary_connection_string
  }
  dependencies                              = [
    module.appi_postoffice.dependent_on,
    module.azfun_dataavailable_plan.dependent_on,
    module.azfun_dataavailable_stor.dependent_on,
    module.sbq_dataavailable.dependent_on,
  ]
}

module "azfun_dataavailable_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.8.0"
  name                = "asp-dataavailable-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  location            = data.azurerm_resource_group.postoffice.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.postoffice.tags
}

module "azfun_dataavailable_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.8.0"
  name                      = "stor${random_string.dataavailable.result}"
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  location                  = data.azurerm_resource_group.postoffice.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.postoffice.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "dataavailable" {
  length  = 10
  special = false
  upper   = false
}