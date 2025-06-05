resource "aws_acm_certificate" "app" {
  domain_name       = var.domain_name
  subject_alternative_names = ["*.${var.domain_name}"]  # Tillåter alla subdomäner
  validation_method = "DNS"

  lifecycle {
    create_before_destroy = true
  }

  tags = {
    Name        = "${var.environment}-certificate"
    Environment = var.environment
  }
}

resource "aws_acm_certificate_validation" "app" {
  certificate_arn         = aws_acm_certificate.app.arn
  validation_record_fqdns = [for record in aws_acm_certificate.app.domain_validation_options : record.resource_record_name]
}

# Store certificate in Secrets Manager
resource "aws_secretsmanager_secret" "certificate" {
  name        = "${var.environment}/certificate"
  description = "SSL Certificate for ${var.domain_name} and subdomains"
  kms_key_id  = aws_kms_key.certificate.arn

  tags = {
    Environment = var.environment
  }
}

resource "aws_secretsmanager_secret_version" "certificate" {
  secret_id     = aws_secretsmanager_secret.certificate.id
  secret_string = jsonencode({
    certificate = base64encode(aws_acm_certificate.app.certificate_pem)
    private_key = base64encode(aws_acm_certificate.app.private_key_pem)
  })
}

# KMS key for encrypting the certificate
resource "aws_kms_key" "certificate" {
  description             = "KMS key for encrypting SSL certificate"
  deletion_window_in_days = 7
  enable_key_rotation     = true

  tags = {
    Environment = var.environment
  }
}

resource "aws_kms_alias" "certificate" {
  name          = "alias/${var.environment}-certificate-key"
  target_key_id = aws_kms_key.certificate.key_id
} 