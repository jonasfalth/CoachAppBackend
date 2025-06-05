output "certificate_arn" {
  description = "ARN of the certificate"
  value       = aws_acm_certificate.app.arn
}

output "certificate_validation_arn" {
  description = "ARN of the certificate validation"
  value       = aws_acm_certificate_validation.app.certificate_arn
}

output "kms_key_arn" {
  description = "ARN of the KMS key used to encrypt the certificate"
  value       = aws_kms_key.certificate.arn
}

output "secret_arn" {
  description = "ARN of the secret containing the certificate"
  value       = aws_secretsmanager_secret.certificate.arn
} 