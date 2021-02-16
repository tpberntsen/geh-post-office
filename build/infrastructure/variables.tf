variable "resource_group_name" {
  type = string
}

variable "environment" {
  type          = string
  description   = "Enviroment that the infrastructure code is deployed into"
}

variable "project" {
  type          = string
  description   = "Project that is running the infrastructure code"
}

variable "organisation" {
  type          = string
  description   = "Organisation that is running the infrastructure code"
}

variable "current_spn_object_id" {
  type          = string
  description   = "Service Principal Object ID of the connection used to deploy the code"
}

variable "current_spn_id" {
  type          = string
  description   = "Service Principal ID of the connection used to deploy the code"
}

variable "current_spn_secret" {
  type          = string
  description   = "Service Principal secret of the connection used to deploy the code"
}

variable "current_tenant_id" {
  type          = string
  description   = "Tenant Id that the infrastructure code is deployed into"
}

variable "current_subscription_id" {
  type          = string
  description   = "The ID of the subscription that the infrastructure code is deployed into"
}