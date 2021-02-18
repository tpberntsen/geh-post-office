locals {
  message_db_connection_string = "AccountEndpoint=${azurerm_cosmosdb_account.messages.endpoint}/;AccountKey=${azurerm_cosmosdb_account.messages.primary_key};"
}