# Stop the script if any command fails
$ErrorActionPreference = "Stop"

# -----------------------------------------------------------------------------
# Setup variables for deployment
# -----------------------------------------------------------------------------
$apiProjectPath = "C:\Source\AvalonPizza\avalonpizza.server"
$TemplatePath = "$apiProjectPath\serverless.template"
$ConfigurationPath = "$apiProjectPath\aws-lambda-tools-defaults.json"
$stackName = "AvalonPizza-Api-Stack"

# -----------------------------------------------------------------------------
# Validate the CloudFormation template before proceeding
# -----------------------------------------------------------------------------
Write-Host "Validating CloudFormation template..." -ForegroundColor Cyan

aws cloudformation validate-template --template-body "file://$TemplatePath"

if ($LASTEXITCODE -ne 0) { Write-Error "Template validation failed."; exit 1 }
Write-Host "Validation successful!" -ForegroundColor Green

# -----------------------------------------------------------------------------
# Verify AWS credentials and permissions
# -----------------------------------------------------------------------------
Write-Host "Verifying AWS credentials..." -ForegroundColor Cyan

try {
    $callerIdentity = aws sts get-caller-identity --output json | ConvertFrom-Json
    Write-Host "Successfully authenticated as:" -ForegroundColor Green
    Write-Host "UserId: $($callerIdentity.UserId)"
    Write-Host "Account: $($callerIdentity.Account)"
    Write-Host "ARN: $($callerIdentity.Arn)"
}
catch {
    Write-Error "AWS Authentication failed. Please check your credentials."
    exit 1
}

# -----------------------------------------------------------------------------
# Build the .NET project and deploy using AWS .NET Global Tools
# -----------------------------------------------------------------------------
Write-Host "Starting build, optimization, and deploy process for $StackName..." -ForegroundColor Cyan

# Clean previous build artifacts to ensure a fresh deployment
Remove-Item -Path "$apiProjectPath\bin", "$apiProjectPath\obj" -Recurse -Force -ErrorAction SilentlyContinue

# The tool automatically pulls Stack, S3 Bucket, Region, and Template configs from aws-lambda-tools-defaults.json file
dotnet lambda deploy-serverless --config-file "$ConfigurationPath"
if ($LASTEXITCODE -ne 0) { Write-Error "Deployment failed!" exit 1 }

Write-Host "Deployment completed successfully!" -ForegroundColor Green