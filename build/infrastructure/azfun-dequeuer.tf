module "azfun_dequeuer" {
  source                                    = "../modules/function-app"
  name                                      = "azfun-dequeuer-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.postoffice.name
  location                                  = data.azurerm_resource_group.postoffice.location
  storage_account_access_key                = module.azfun_dequeuer_stor.primary_access_key
  storage_account_name                      = module.azfun_dequeuer_stor.name
  app_service_plan_id                       = module.azfun_dequeuer_plan.id
  application_insights_instrumentation_key  = module.appi_postoffice.instrumentation_key
  tags                                      = data.azurerm_resource_group.postoffice.tags
  app_settings                              = {
    POSTOFFICE_DB_CONNECTION_STRING = azurerm_cosmosdb_account.postoffice.endpoint,
    POSTOFFICE_DB_KEY               = azurerm_cosmosdb_account.postoffice.primary_key
  }
  dependencies                              = [
    module.azfun_dequeuer_plan.dependent_on,
    module.azfun_dequeuer_stor.dependent_on,
  ]
}

module "azfun_dequeuer_plan" {
  source              = "../modules/app-service-plan"
  name                = "asp-dequeuer-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  location            = data.azurerm_resource_group.postoffice.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.postoffice.tags
}

module "azfun_dequeuer_stor" {
  source                    = "../modules/storage-account"
  name                      = "stor${random_string.dequeuer.result}${var.organisation}${lower(var.environment)}"
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  location                  = data.azurerm_resource_group.postoffice.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.postoffice.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "dequeuer" {
  length  = 5
  special = false
  upper   = false
}