module "azfun_inboundapi" {
  source                                    = "../modules/function-app"
  name                                      = "azfun-inboundapi-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.datahub.name
  location                                  = data.azurerm_resource_group.datahub.location
  storage_account_access_key                = module.azfun_inboundapi_stor.primary_access_key
  storage_account_name                      = module.azfun_inboundapi_stor.name
  app_service_plan_id                       = module.azfun_inboundapi_plan.id
  application_insights_instrumentation_key  = module.appi_postoffice.instrumentation_key
  tags                                      = data.azurerm_resource_group.datahub.tags
  app_settings                              = {
    POSTOFFICE_DB_CONNECTION_STRING = azurerm_cosmosdb_account.postoffice.endpoint,
    POSTOFFICE_DB_KEY               = azurerm_cosmosdb_account.postoffice.primary_key
  }
  dependencies                              = [
    module.azfun_inboundapi_plan.dependent_on,
    module.azfun_inboundapi_stor.dependent_on,
  ]
}

module "azfun_inboundapi_plan" {
  source              = "../modules/app-service-plan"
  name                = "asp-inboundapi-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.datahub.name
  location            = data.azurerm_resource_group.datahub.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.datahub.tags
}

module "azfun_inboundapi_stor" {
  source                    = "../modules/storage-account"
  name                      = "stor${random_string.inboundapi.result}${var.organisation}${lower(var.environment)}"
  resource_group_name       = data.azurerm_resource_group.datahub.name
  location                  = data.azurerm_resource_group.datahub.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.datahub.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "inboundapi" {
  length  = 5
  special = false
  upper   = false
}