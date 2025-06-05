variable "domain_name" {
  description = "The domain name for the application"
  type        = string
}

variable "environment" {
  description = "The environment (staging/prod)"
  type        = string
}

variable "kms_key_arn" {
  description = "The ARN of the KMS key for encryption"
  type        = string
}

variable "secret_arn" {
  description = "The ARN of the secret in AWS Secrets Manager"
  type        = string
}

variable "certificate_password" {
  description = "The password for the SSL certificate"
  type        = string
  sensitive   = true
} 