# Stop the script if any command fails
$ErrorActionPreference = "Stop"



Write-Host "Validating CloudFormation template..." -ForegroundColor Yellow

aws cloudformation validate-template --template-body file://base-stack.template

if ($LASTEXITCODE -ne 0) {
    Write-Error "Template validation failed. Please check your JSON syntax."
    exit 1
}

Write-Host "Validation successful!" -ForegroundColor Green



# Verify AWS Connectivity
Write-Host "Verifying AWS credentials..." -ForegroundColor Yellow

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



Write-Host "Starting deployment for Avalon Pizza Base Infrastructure..." -ForegroundColor Cyan

$TemplatePath = Join-Path -Path $PSScriptRoot -ChildPath "..\base-stack.template"
# Retrieve the database username from AWS Systems Manager Parameter Store
$dbUser = aws ssm get-parameter --name "/pizzaapi/DbUser" --query "Parameter.Value" --output text

# Deploy the CloudFormation stack
aws cloudformation deploy `
  --template-file $TemplatePath `
  --stack-name AvalonPizza-Base-Stack `
  --capabilities CAPABILITY_IAM `
  --region ca-central-1 `
  --parameter-overrides DbUser=$dbUser

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deployment complete!" -ForegroundColor Green
} else {
    Write-Host "Deployment failed!" -ForegroundColor Red
}