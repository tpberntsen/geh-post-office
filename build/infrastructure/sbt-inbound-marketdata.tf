module "sbt_marketdata" {
  source              = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-topic?ref=1.2.0"
  name                = "marketdata"
  namespace_name      = module.sbn_inbound.name
  resource_group_name = data.azurerm_resource_group.postoffice.name
  dependencies        = [module.sbn_inbound]
}

module "sbtaur_marketdata_subscription" {
  source                    = "git::https://github.com/Energinet-DataHub/green-energy-hub-core.git//terraform/modules/service-bus-subscription?ref=1.2.0"
  name                      = "default"
  namespace_name            = module.sbn_inbound.name
  topic_name                = module.sbt_marketdata.name
  resource_group_name       = data.azurerm_resource_group.postoffice.name
  max_delivery_count        = 1
  dependencies              = [module.sbt_marketdata]
}