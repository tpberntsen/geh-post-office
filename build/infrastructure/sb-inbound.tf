module "sbn_inbound" {
  source              = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-namespace?ref=1.2.0"
  name                = "sbn-inbound-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  location            = data.azurerm_resource_group.postoffice.location
  sku                 = "basic"
  tags                = data.azurerm_resource_group.postoffice.tags
}

module "sbnar_inbound_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbnar-inbound-listener"
  namespace_name            = module.sbn_inbound.name
  topic_name                = module.sbt_inbound.name
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  listen                    = true
  dependencies              = [module.sbt_inbound]
}

module "sbnar_inbound_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbnar-inbound-sender"
  namespace_name            = module.sbn_inbound.name
  topic_name                = module.sbt_inbound.name
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  send                      = true
  dependencies              = [module.sbt_inbound]
}