module "sbn_inbound" {
  source              = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-namespace?ref=1.0.0"
  name                = "sbn-inbound-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  location            = data.azurerm_resource_group.postoffice.location
  sku                 = "basic"
  tags                = data.azurerm_resource_group.postoffice.tags
}

module "sbt_marketdata" {
  source              = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-topic?ref=renetnielsen/add-service-bus-topic"
  name                = "marketdata"
  namespace_name      = module.sbn_inbound.name
  resource_group_name = data.azurerm_resource_group.postoffice.name
  dependencies        = [module.sbn_inbound]
}

module "sbnar_marketdata_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-topic-auth-rule?ref=renetnielsen/add-service-bus-topic"
  name                      = "sbnar-inbound-listener"
  namespace_name            = module.sbn_inbound.name
  topic_name                = module.sbt_marketdata.name
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  listen                    = true
  dependencies              = [module.sbt_marketdata]
}

module "sbnar_marketdata_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-topic-auth-rule?ref=renetnielsen/add-service-bus-topic"
  name                      = "sbnar-inbound-sender"
  namespace_name            = module.sbn_inbound.name
  topic_name                = module.sbt_marketdata.name
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  send                      = true
  dependencies              = [module.sbt_marketdata]
}