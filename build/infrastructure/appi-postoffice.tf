module "appi_postoffice" {
  source              = "../modules/application-insights" 

  name                = "appi-postoffice-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.datahub.name
  location            = data.azurerm_resource_group.datahub.location
  tags                = data.azurerm_resource_group.datahub.tags
}