resource "azurerm_cosmosdb_account" "postoffice" {
  name                = "cosmos-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  location            = data.azurerm_resource_group.postoffice.location
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  # To enable global failover change to true and uncomment second geo_location
  enable_automatic_failover = false

  consistency_policy {
    consistency_level = "Session"
  }
  
  geo_location {
    location          = data.azurerm_resource_group.postoffice.location
    failover_priority = 0
  }

  tags                = data.azurerm_resource_group.postoffice.tags
}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "messages"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  account_name        = azurerm_cosmosdb_account.postoffice.name
}

resource "azurerm_cosmosdb_sql_container" "collection_timeseries" {
  name                = "timeseries"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.postoffice.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/RecipientMarketParticipantmRID"
}

resource "azurerm_cosmosdb_sql_container" "collection_marketdata" {
  name                = "marketdata"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.postoffice.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/RecipientMarketParticipantmRID"
}

resource "azurerm_cosmosdb_sql_container" "collection_aggregations" {
  name                = "aggregations"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.postoffice.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/RecipientMarketParticipantmRID"
}