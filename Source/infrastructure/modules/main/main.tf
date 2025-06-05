module "ec2" {
  source = "../ec2"
  // ... existing ec2 configuration ...
}

module "acm" {
  source = "../acm"
  domain_name = var.domain_name
  environment = var.environment
}

module "dns" {
  source = "../dns"

  domain_name = var.domain_name
  instance_ip = module.ec2.instance_public_ip
  domain_validation_options = module.acm.domain_validation_options
  certificate_arn = module.acm.certificate_arn
} 