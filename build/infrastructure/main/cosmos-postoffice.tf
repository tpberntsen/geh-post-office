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
  name                = "cosmos-messages-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  public_network_access_enabled = false
  is_virtual_network_filter_enabled = true
  # To enable global failover change to true and uncomment second geo_location
  enable_automatic_failover = false

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = azurerm_resource_group.this.location
    failover_priority = 0
  }

  tags                = azurerm_resource_group.this.tags

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "post-office"
  resource_group_name = azurerm_resource_group.this.name
  account_name        = azurerm_cosmosdb_account.post_office.name
}

resource "azurerm_cosmosdb_sql_container" "collection_catalog" {
  name                = "catalog"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.post_office.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/partitionKey"
}

resource "azurerm_cosmosdb_sql_container" "collection_cabinet" {
  name                = "cabinet"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.post_office.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/partitionKey"
}

resource "azurerm_cosmosdb_sql_container" "collection_idempotency" {
  name                = "idempotency"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.post_office.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/partitionKey"
}

resource "azurerm_cosmosdb_sql_container" "collection_bundle" {
  name                = "bundle"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.post_office.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/recipient"
}

resource "azurerm_cosmosdb_sql_trigger" "triggers_ensuresingleunacknowledgedbundle" {
  name         = "EnsureSingleUnacknowledgedBundle"
  container_id = azurerm_cosmosdb_sql_container.collection_bundle.id
  body         = <<EOT
    function trigger() {

    var context = getContext();
    var container = context.getCollection();
    var response = context.getResponse();
    var createdItem = response.getBody();

    var filterQuery = `
    SELECT * FROM bundle b
    WHERE b.recipient = '$${createdItem.recipient}' AND
          b.dequeued = false AND (
          b.origin = '$${createdItem.origin}' OR
         ((b.origin = 'MarketRoles' OR b.origin = 'Charges') AND '$${createdItem.origin}' = 'MeteringPoints') OR
         ((b.origin = 'Charges' OR b.origin = 'MeteringPoints') AND '$${createdItem.origin}' = 'MarketRoles') OR
         ((b.origin = 'MarketRoles' OR b.origin = 'MeteringPoints') AND '$${createdItem.origin}' = 'Charges'))`;

    var accept = container.queryDocuments(container.getSelfLink(), filterQuery, function(err, items, options)
    {
        if (err) throw err;
        if (items.length !== 0) throw 'SingleBundleViolation';
    });

    if (!accept) throw 'queryDocuments in trigger failed.'; }
      EOT
  operation    = "Create"
  type         = "Post"
}

#
# Private Endpoint for SQL subresource
#

resource "random_string" "sql" {
  length  = 5
  special = false
  upper   = false
}

resource "azurerm_private_endpoint" "cosmos_sql" {
    name                = "pe-cosmos${random_string.sql.result}-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
    location            = azurerm_resource_group.this.location
    resource_group_name = azurerm_resource_group.this.name
    subnet_id           = module.snet_internal_private_endpoints.id
    private_service_connection {
        is_manual_connection       = false
        name                       = "psc-01"
        private_connection_resource_id = azurerm_cosmosdb_account.post_office.id
        subresource_names          = ["sql"]
  }
}

# Create an A record pointing to the Cosmos SQL private endpoint
resource "azurerm_private_dns_a_record" "cosmosdb_sql" {
  name                = azurerm_cosmosdb_account.post_office.name
  zone_name           = "privatelink.documents.azure.com"
  resource_group_name = data.azurerm_key_vault_secret.pdns_resource_group_name.value
  ttl                 = 3600
  records             = [azurerm_private_endpoint.cosmos_sql.private_service_connection[0].private_ip_address]
}

# Create an A record pointing to the private endpoint with region location
resource "azurerm_private_dns_a_record" "cosmosdb_sql_location" {
  name                = "${azurerm_cosmosdb_account.post_office.name}-${azurerm_resource_group.this.location}"
  zone_name           = "privatelink.documents.azure.com"
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  records             = [azurerm_private_endpoint.cosmos_sql.private_service_connection[0].private_ip_address]
}