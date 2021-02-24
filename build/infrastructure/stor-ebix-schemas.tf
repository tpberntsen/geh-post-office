module "stor_schemas" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/storage-account?ref=1.3.0"
  name                      = "schemas${lower(var.organisation)}${lower(var.environment)}"
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  location                  = data.azurerm_resource_group.postoffice.location
  account_replication_type  = "LRS"
  access_tier               = "Hot"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.postoffice.tags
}

module "stor_ebixschemas" {
  source                = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/storage-container?ref=1.3.0"
  container_name        = "ebix"
  storage_account_name  = module.stor_schemas.name
  container_access_type = "private"
  dependencies          = [
    module.stor_schemas.dependent_on
  ]
}