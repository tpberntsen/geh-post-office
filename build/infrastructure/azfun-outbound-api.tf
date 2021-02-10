module "azfun_outboundapi" {
  source                                    = "../modules/function-app"
  name                                      = "azfun-outboundapi-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.datahub.name
  location                                  = data.azurerm_resource_group.datahub.location
  storage_account_access_key                = module.azfun_outboundapi_stor.primary_access_key
  storage_account_name                      = module.azfun_outboundapi_stor.name
  app_service_plan_id                       = module.azfun_outboundapi_plan.id
  application_insights_instrumentation_key  = module.appi_postoffice.instrumentation_key
  tags                                      = data.azurerm_resource_group.datahub.tags
  app_settings                              = {
    POSTOFFICE_DB_CONNECTION_STRING = azurerm_cosmosdb_account.postoffice.endpoint,
    POSTOFFICE_DB_KEY               = azurerm_cosmosdb_account.postoffice.primary_key
  }
  dependencies                              = [
    module.azfun_outboundapi_plan.dependent_on,
    module.azfun_outboundapi_stor.dependent_on,
  ]
}

module "azfun_outboundapi_plan" {
  source              = "../modules/app-service-plan"
  name                = "asp-outboundapi-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.datahub.name
  location            = data.azurerm_resource_group.datahub.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.datahub.tags
}

module "azfun_outboundapi_stor" {
  source                    = "../modules/storage-account"
  name                      = "stor${random_string.outboundapi.result}${var.organisation}${lower(var.environment)}"
  resource_group_name       = data.azurerm_resource_group.datahub.name
  location                  = data.azurerm_resource_group.datahub.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.datahub.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "outboundapi" {
  length  = 5
  special = false
  upper   = false
}