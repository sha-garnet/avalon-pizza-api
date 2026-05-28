Write-Host "Validating CloudFormation template..." -ForegroundColor Yellow

aws cloudformation validate-template --template-body file://serverless.template

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
    Write-Host "Account: $($callerIdentity.UserId)"
    Write-Host "Account: $($callerIdentity.Account)"
    Write-Host "ARN: $($callerIdentity.Arn)"
}
catch {
    Write-Error "AWS Authentication failed. Please check your credentials."
    exit 1
}

$S3Bucket = "avalon-pizza-deploy-sandbox-318724428478-ca-central-1"
$StackName = "AvalonPizza-Api-Stack"

Write-Host "Starting build and deploy process for $StackName..." -ForegroundColor Cyan

# Build the .NET project
dotnet build --configuration Release
# SAM will see the output and use it when it builds the deployment package
sam build
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed!"; exit }

# Deploy the application
# SAM handles the package and upload to S3 automatically behind the scenes
sam deploy `
  --template-file serverless.template `
  --stack-name $StackName `
  --s3-bucket $S3Bucket `
  --region ca-central-1 `
  --capabilities CAPABILITY_IAM `  
  --no-confirm-changeset

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deployment successful!" -ForegroundColor Green
    
    # Print the API URL after deployment
    $apiUrl = aws cloudformation describe-stacks --stack-name $StackName --query "Stacks[0].Outputs[?OutputKey=='ApiURL'].OutputValue" --output text
    Write-Host "API Endpoint: $apiUrl" -ForegroundColor Cyan

} else {
    Write-Host "Deployment failed!" -ForegroundColor Red
}