# Stop the script if any command fails
$ErrorActionPreference = "Stop"

# -----------------------------------------------------------------------------
# Setup variables for deployment
# -----------------------------------------------------------------------------
$TemplatePath = "C:\Source\AvalonPizza\base-stack.template"

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
# Build and deploy the CloudFormation stack
# -----------------------------------------------------------------------------
Write-Host "Starting deployment for Avalon Pizza Base Infrastructure..." -ForegroundColor Cyan

# Retrieve the database username from AWS Systems Manager Parameter Store to pass as a parameter during deployment
# DBInstance master username cant be dynamic, so we have to fetch it from parameter store and pass it as a parameter during deployment
$dbUser = aws ssm get-parameter --name "/pizzaapi/DbUser" --query "Parameter.Value" --output text

aws cloudformation deploy `
  --template-file "$TemplatePath" `
  --stack-name "AvalonPizza-Base-Stack" `
  --capabilities CAPABILITY_IAM `
  --region "ca-central-1" `
  --parameter-overrides "DbUser=$dbUser"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deployment complete!" -ForegroundColor Green
} else {
    Write-Host "Deployment failed!" -ForegroundColor Red
}