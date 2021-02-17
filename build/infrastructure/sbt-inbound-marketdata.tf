module "sbt_marketdata" {
  source              = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-topic?ref=renetnielsen/add-service-bus-topic"
  name                = "marketdata"
  namespace_name      = module.sbn_inbound.name
  resource_group_name = data.azurerm_resource_group.postoffice.name
  dependencies        = [module.sbn_inbound]
}

module "sbtaur_marketdata_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-topic-auth-rule?ref=renetnielsen/add-service-bus-topic"
  name                      = "sbtaur-inbound-listener"
  namespace_name            = module.sbn_inbound.name
  topic_name                = module.sbt_marketdata.name
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  listen                    = true
  dependencies              = [module.sbt_marketdata]
}

module "sbtaur_marketdata_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-topic-auth-rule?ref=renetnielsen/add-service-bus-topic"
  name                      = "sbtaur-inbound-sender"
  namespace_name            = module.sbn_inbound.name
  topic_name                = module.sbt_marketdata.name
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  send                      = true
  dependencies              = [module.sbt_marketdata]
}

module "sbtaur_marketdata_subscription" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-subscription?ref=renetnielsen/add-service-bus-topic"
  name                      = "default"
  namespace_name            = module.sbn_inbound.name
  topic_name                = module.sbt_marketdata.name
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  max_delivery_count        = 1
  dependencies              = [module.sbt_marketdata]
}