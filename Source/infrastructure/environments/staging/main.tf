provider "aws" {
  region     = var.aws_region
  access_key = var.aws_access_key
  secret_key = var.aws_secret_key
}

module "networking" {
  source = "../../modules/networking"

  environment = "staging"
}

module "certificate" {
  source = "../../modules/certificate"

  environment   = "staging"
  domain_name   = var.domain_name
}

module "compute" {
  source = "../../modules/compute"

  environment          = "staging"
  subnet_id           = module.networking.public_subnet_id
  security_group_id   = module.networking.security_group_id
  kms_key_arn         = var.kms_key_arn
  secret_arn          = var.secret_arn
  certificate_password = var.certificate_password
}

module "main" {
  source = "../../modules/main"

  environment          = "staging"
  domain_name         = var.domain_name
  kms_key_arn         = var.kms_key_arn
  secret_arn          = var.secret_arn
  certificate_password = var.certificate_password
} 