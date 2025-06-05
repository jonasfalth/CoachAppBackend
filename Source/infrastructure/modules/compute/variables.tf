variable "environment" {
  description = "Environment name (e.g. staging, prod)"
  type        = string
}

variable "subnet_id" {
  description = "ID of the subnet where the instance will be launched"
  type        = string
}

variable "security_group_id" {
  description = "ID of the security group for the instance"
  type        = string
}

variable "certificate_password" {
  description = "Password for the SSL certificate"
  type        = string
  sensitive   = true
}

variable "kms_key_arn" {
  description = "ARN of the KMS key used to encrypt the certificate"
  type        = string
}

variable "secret_arn" {
  description = "ARN of the secret containing the certificate"
  type        = string
} 