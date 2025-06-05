variable "domain_name" {
  description = "The domain name for the application"
  type        = string
}

variable "instance_ip" {
  description = "The IP address of the EC2 instance"
  type        = string
}

variable "domain_validation_options" {
  description = "Domain validation options for ACM certificate"
  type = list(object({
    domain_name           = string
    resource_record_name  = string
    resource_record_value = string
    resource_record_type  = string
  }))
  default = []
}

variable "certificate_arn" {
  description = "The ARN of the ACM certificate"
  type        = string
} 