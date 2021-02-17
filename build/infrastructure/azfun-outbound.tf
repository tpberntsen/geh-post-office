module "azfun_outbound" {
  source                                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/function-app?ref=1.0.0"
  name                                      = "azfun-outbound-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.postoffice.name
  location                                  = data.azurerm_resource_group.postoffice.location
  storage_account_access_key                = module.azfun_outbound_stor.primary_access_key
  storage_account_name                      = module.azfun_outbound_stor.name
  app_service_plan_id                       = module.azfun_outbound_plan.id
  application_insights_instrumentation_key  = module.appi_postoffice.instrumentation_key
  tags                                      = data.azurerm_resource_group.postoffice.tags
  app_settings                              = {
    MESSAGES_DB_CONNECTION_STRING = azurerm_cosmosdb_account.messages.endpoint,
    MESSAGES_DB_NAME              = azurerm_cosmosdb_sql_database.db.name,
  }
  dependencies                              = [
    module.azfun_outbound_plan.dependent_on,
    module.azfun_outbound_stor.dependent_on,
  ]
}

module "azfun_outbound_plan" {
  source              = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/app-service-plan?ref=1.0.0"
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
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/storage-account?ref=1.0.0"
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