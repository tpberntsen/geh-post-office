# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
resource "azurerm_cosmosdb_account" "post_office" {
  name                = "cosmos-messages-${var.project}-${var.organisation}-${var.environment}"
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
  name                = "post-office"
  resource_group_name = data.azurerm_resource_group.postoffice.name
  account_name        = azurerm_cosmosdb_account.post_office.name
}

resource "azurerm_cosmosdb_sql_container" "collection_dataavailable" {
  name                = "dataavailable"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.post_office.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/recipient"
}

resource "azurerm_cosmosdb_sql_container" "collection_bundles" {
  name                = "bundles"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.post_office.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/recipient"
}