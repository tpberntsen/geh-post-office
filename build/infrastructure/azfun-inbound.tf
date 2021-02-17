module "azfun_inbound" {
  source                                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/function-app?ref=1.0.0"
  name                                      = "azfun-inbound-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.postoffice.name
  location                                  = data.azurerm_resource_group.postoffice.location
  storage_account_access_key                = module.azfun_inbound_stor.primary_access_key
  storage_account_name                      = module.azfun_inbound_stor.name
  app_service_plan_id                       = module.azfun_inbound_plan.id
  application_insights_instrumentation_key  = module.appi_postoffice.instrumentation_key
  tags                                      = data.azurerm_resource_group.postoffice.tags
  app_settings                              = {
    MESSAGES_DB_CONNECTION_STRING                     = azurerm_cosmosdb_account.messages.endpoint,
    MESSAGES_DB_NAME                                  = azurerm_cosmosdb_sql_database.db.name,
    INBOUND_QUEUE_MARKETDATA_TOPIC_NAME               = module.sbt_marketdata.name
    INBOUND_QUEUE_MARKETDATA_SUBSCRIPTION_NAME        = module.sbtaur_marketdata_subscription.name
    INBOUND_QUEUE_MARKETDATA_CONNECTION_STRING        = module.sbtaur_marketdata_listener.primary_connection_string
    INBOUND_QUEUE_AGGREGATIONS_TOPIC_NAME             = module.sbt_aggregations.name
    INBOUND_QUEUE_AGGREGATIONS_SUBSCRIPTION_NAME      = module.sbtaur_aggregations_subscription.name
    INBOUND_QUEUE_AGGREGATIONS_CONNECTION_STRING      = module.sbtaur_aggregations_listener.primary_connection_string
    INBOUND_QUEUE_TIMESERIES_TOPIC_NAME               = module.sbt_timeseries.name
    INBOUND_QUEUE_TIMESERIES_SUBSCRIPTION_NAME        = module.sbtaur_timeseries_subscription.name
    INBOUND_QUEUE_TIMESERIES_CONNECTION_STRING        = module.sbtaur_timeseries_listener.primary_connection_string
    "MESSAGES_DB_TYPE_CONTAINER_MAP:changeofsupplier" = module.collection_marketdata.name
    "MESSAGES_DB_TYPE_CONTAINER_MAP:timeseries"       = module.collection_timeseries.name
  }
  dependencies                              = [
    module.appi_postoffice.dependent_on,
    module.azfun_inbound_plan.dependent_on,
    module.azfun_inbound_stor.dependent_on,
    module.azurerm_cosmosdb_account.dependent_on,
    module.sbt_marketdata.dependent_on,
    module.sbtaur_marketdata_subscription.dependent_on,
    module.sbtaur_marketdata_listener.dependent_on,
    module.sbt_aggregations.dependent_on,
    module.sbtaur_aggregations_subscription.dependent_on,
    module.sbtaur_aggregations_listener.dependent_on,
    module.sbt_timeseries.dependent_on,
    module.sbtaur_timeseries_subscription.dependent_on,
    module.sbtaur_timeseries_listener.dependent_on,
  ]
}

module "azfun_inbound_plan" {
  source              = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/app-service-plan?ref=1.0.0"
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
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/storage-account?ref=1.0.0"
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