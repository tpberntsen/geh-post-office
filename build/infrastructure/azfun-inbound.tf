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
    POSTOFFICE_DB_CONNECTION_STRING     = azurerm_cosmosdb_account.postoffice.endpoint,
    POSTOFFICE_DB_KEY                   = azurerm_cosmosdb_account.postoffice.primary_key
    INBOUND_QUEUE_MARKETDATA_TOPIC_NAME = module.sbt_marketdata.name
  }
  dependencies                              = [
    module.azfun_inbound_plan.dependent_on,
    module.azfun_inbound_stor.dependent_on,
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