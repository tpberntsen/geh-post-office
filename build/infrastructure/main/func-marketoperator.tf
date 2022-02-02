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
module "func_marketoperator" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=6.0.0"

  name                                      = "marketoperator"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = module.plan_shared.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  vnet_integration_subnet_id                = module.vnet_integrations.id
  private_endpoint_subnet_id                = module.private_endpoints_subnet.id
  private_dns_resource_group_name           = data.azurerm_key_vault_secret.vnet_shared_rg_name.value
  always_on                                 = true
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE           = true
    WEBSITE_RUN_FROM_PACKAGE                  = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE       = true
    FUNCTIONS_WORKER_RUNTIME                  = "dotnet-isolated"
    # Endregion
    MESSAGES_DB_CONNECTION_STRING             = local.MESSAGES_DB_CONNECTION_STRING
    MESSAGES_DB_NAME                          = azurerm_cosmosdb_sql_database.db.name
    BlobStorageConnectionString               = data.azurerm_key_vault_secret.st_market_operator_response_primary_connection_string.value
    BlobStorageContainerName                  = data.azurerm_key_vault_secret.st_market_operator_response_postofficereply_container_name.value
    ServiceBusConnectionString                = data.azurerm_key_vault_secret.sb_domain_relay_transceiver_connection_string.value
    DATAAVAILABLE_QUEUE_CONNECTION_STRING     = data.azurerm_key_vault_secret.sb_domain_relay_transceiver_connection_string.value
    DATAAVAILABLE_QUEUE_NAME                  = data.azurerm_key_vault_secret.sbq_data_available_name.value
    DEQUEUE_CLEANUP_QUEUE_NAME                = data.azurerm_key_vault_secret.sbq_messagehub_dequeue_cleanup_name.value
    LOG_DB_NAME                               = azurerm_cosmosdb_sql_database.log_db.name
    LOG_DB_CONTAINER                          = azurerm_cosmosdb_sql_container.collection_logs.name
    RequestResponseLogConnectionString        = data.azurerm_key_vault_secret.st_market_operator_logs_primary_connection_string.value
    RequestResponseLogContainerName           = data.azurerm_key_vault_secret.st_market_operator_logs_container_name.value
    B2C_TENANT_ID                             = data.azurerm_key_vault_secret.b2c_tenant_id.value
    BACKEND_SERVICE_APP_ID                    = data.azurerm_key_vault_secret.backend_service_app_id.value
    SQL_ACTOR_DB_CONNECTION_STRING            = local.SQL_ACTOR_DB_CONNECTION_STRING
  }

  tags                                      = azurerm_resource_group.this.tags
}