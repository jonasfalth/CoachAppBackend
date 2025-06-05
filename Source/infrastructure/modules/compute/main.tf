data "aws_ami" "windows" {
  most_recent = true
  owners      = ["amazon"]

  filter {
    name   = "name"
    values = ["Windows_Server-2022-Swedish-64Bit-Base-*"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
}

# IAM role for EC2 instance
resource "aws_iam_role" "ec2_role" {
  name = "${var.environment}-ec2-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      }
    ]
  })
}

# IAM policy for accessing Secrets Manager and KMS
resource "aws_iam_policy" "certificate_access" {
  name        = "${var.environment}-certificate-access"
  description = "Policy for accessing certificate in Secrets Manager"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "secretsmanager:GetSecretValue",
          "secretsmanager:DescribeSecret"
        ]
        Effect   = "Allow"
        Resource = var.secret_arn
      },
      {
        Action = [
          "kms:Decrypt",
          "kms:DescribeKey"
        ]
        Effect   = "Allow"
        Resource = var.kms_key_arn
      }
    ]
  })
}

# Attach policy to role
resource "aws_iam_role_policy_attachment" "certificate_access" {
  role       = aws_iam_role.ec2_role.name
  policy_arn = aws_iam_policy.certificate_access.arn
}

# Create instance profile
resource "aws_iam_instance_profile" "ec2_profile" {
  name = "${var.environment}-ec2-profile"
  role = aws_iam_role.ec2_role.name
}

# Get current region and account ID
data "aws_region" "current" {}
data "aws_caller_identity" "current" {}

resource "aws_instance" "app" {
  ami           = data.aws_ami.windows.id
  instance_type = "t2.micro"
  subnet_id     = var.subnet_id
  vpc_security_group_ids = [var.security_group_id]
  iam_instance_profile = aws_iam_instance_profile.ec2_profile.name

  root_block_device {
    volume_size = 30
    volume_type = "gp2"
  }

  user_data = <<-EOF
              <powershell>
              # Install IIS
              Install-WindowsFeature -Name Web-Server -IncludeManagementTools

              # Install .NET Core Hosting Bundle
              Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
              .\dotnet-install.ps1 -Channel 7.0 -Runtime dotnet -Version latest
              .\dotnet-install.ps1 -Channel 7.0 -Runtime aspnetcore -Version latest

              # Install URL Rewrite Module
              Invoke-WebRequest -Uri https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi -OutFile rewrite.msi
              Start-Process -FilePath msiexec.exe -ArgumentList "/i rewrite.msi /qn" -Wait

              # Create application directory
              New-Item -ItemType Directory -Force -Path C:\inetpub\wwwroot\app

              # Configure IIS
              Import-Module WebAdministration
              
              # Create new application pool
              $poolName = "AppPool"
              if (!(Test-Path "IIS:\AppPools\$poolName")) {
                  New-WebAppPool -Name $poolName
                  Set-ItemProperty "IIS:\AppPools\$poolName" -Name managedRuntimeVersion -Value ""
                  Set-ItemProperty "IIS:\AppPools\$poolName" -Name managedPipelineMode -Value 1
                  Set-ItemProperty "IIS:\AppPools\$poolName" -Name startMode -Value "AlwaysRunning"
              }

              # Create new website
              $siteName = "App"
              if (!(Test-Path "IIS:\Sites\$siteName")) {
                  New-Website -Name $siteName -PhysicalPath "C:\inetpub\wwwroot\app" -ApplicationPool $poolName -Force
              }

              # Get certificate from Secrets Manager
              $certPath = "C:\app\certificate.pfx"
              $secret = Get-SECSecretValue -SecretId "${var.secret_arn}"
              $secretObj = ConvertFrom-Json $secret.SecretString
              
              # Convert certificate and private key to PFX
              $certBytes = [Convert]::FromBase64String($secretObj.certificate)
              $keyBytes = [Convert]::FromBase64String($secretObj.private_key)
              
              # Create PFX file
              $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certBytes)
              $cert.Import($keyBytes, $null, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)
              $certBytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, "${var.certificate_password}")
              [System.IO.File]::WriteAllBytes($certPath, $certBytes)

              # Configure SSL
              $certPassword = ConvertTo-SecureString -String "${var.certificate_password}" -Force -AsPlainText
              $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certPath, $certPassword)
              $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("My", "LocalMachine")
              $store.Open("ReadWrite")
              $store.Add($cert)
              $store.Close()

              # Bind certificate to website
              $certThumbprint = $cert.Thumbprint
              New-WebBinding -Name $siteName -Protocol "https" -Port 443 -SslFlags 1
              $binding = Get-WebBinding -Name $siteName -Protocol "https"
              $binding.AddSslCertificate($certThumbprint, "My")

              # Configure web.config
              $webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments="C:\inetpub\wwwroot\app\CoachBackend.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
  </system.webServer>
</configuration>
"@
              Set-Content -Path "C:\inetpub\wwwroot\app\web.config" -Value $webConfig

              # Create logs directory
              New-Item -ItemType Directory -Force -Path "C:\inetpub\wwwroot\app\logs"

              # Restart IIS
              iisreset
              </powershell>
              EOF

  tags = {
    Name = "${var.environment}-app-server"
  }
} 