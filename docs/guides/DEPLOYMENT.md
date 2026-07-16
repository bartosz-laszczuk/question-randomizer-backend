# Deployment Guide

## Question Randomizer Backend - Deployment Documentation

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Development](#local-development)
3. [Docker Deployment](#docker-deployment)
4. [Cloud Deployment](#cloud-deployment)
5. [Environment Configuration](#environment-configuration)
6. [Database Setup](#database-setup)
7. [Monitoring & Logging](#monitoring--logging)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

- **.NET 10 SDK** - `dotnet --version` should show 10.x.x
- **Docker Desktop** - For containerized deployment
- **Firebase Project** - With Firestore enabled
- **Git** - For source control

### Optional

- **Azure CLI** - For Azure deployment
- **AWS CLI** - For AWS deployment
- **kubectl** - For Kubernetes deployment

---

## Local Development

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/question-randomizer-backend.git
cd question-randomizer-backend
```

### 2. Configure Firebase

1. Get service account key from [Firebase Console](https://console.firebase.google.com/)
2. Save as `firebase-dev-credentials.json` in project root
3. **Add to `.gitignore`** (already configured)

### 3. Update Configuration

Update `appsettings.Development.json`:

```json
{
  "Firebase": {
    "ProjectId": "your-dev-project-id",
    "CredentialsPath": "firebase-dev-credentials.json"
  }
}
```

### 4. Run Locally

**Controllers API (Port 5000):**
```bash
cd src/QuestionRandomizer.Api.Controllers
dotnet run
# Browse to: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

**Minimal API (Port 5001):**
```bash
cd src/QuestionRandomizer.Api.MinimalApi
dotnet run
# Browse to: http://localhost:5001
# Swagger UI: http://localhost:5001/swagger
```

---

## Docker Deployment

### Build Docker Images

**Build Controllers API:**
```bash
docker build -t question-randomizer-controllers:latest \
  -f src/QuestionRandomizer.Api.Controllers/Dockerfile .
```

**Build Minimal API:**
```bash
docker build -t question-randomizer-minimal:latest \
  -f src/QuestionRandomizer.Api.MinimalApi/Dockerfile .
```

### Run with Docker Compose

1. **Create `.env` file** (copy from `.env.example`):
```bash
cp .env.example .env
```

2. **Edit `.env` file:**
```env
FIREBASE_PROJECT_ID=your-project-id
AGENT_SERVICE_URL=http://localhost:3002
CORS_ORIGIN=http://localhost:4200
ASPNETCORE_ENVIRONMENT=Production
```

3. **Place Firebase credentials:**
```bash
# Copy your Firebase credentials to project root
cp /path/to/your/firebase-credentials.json ./firebase-credentials.json
```

4. **Start services:**
```bash
docker-compose up -d
```

5. **Verify services:**
```bash
# Check service health
curl http://localhost:5000/health
curl http://localhost:5001/health

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

---

## Cloud Deployment

### Azure App Service

#### 1. Prerequisites
- Azure account with active subscription
- Azure CLI installed and logged in

#### 2. Create Resources
```bash
# Variables
RESOURCE_GROUP="question-randomizer-rg"
LOCATION="eastus"
APP_SERVICE_PLAN="question-randomizer-plan"
APP_NAME_CONTROLLERS="question-randomizer-controllers"
APP_NAME_MINIMAL="question-randomizer-minimal"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create App Service Plan (Linux, B1 tier)
az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --is-linux \
  --sku B1

# Create Web App for Controllers API
az webapp create \
  --name $APP_NAME_CONTROLLERS \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --runtime "DOTNETCORE:10.0"

# Create Web App for Minimal API
az webapp create \
  --name $APP_NAME_MINIMAL \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --runtime "DOTNETCORE:10.0"
```

#### 3. Configure Environment Variables
```bash
# Controllers API
az webapp config appsettings set \
  --name $APP_NAME_CONTROLLERS \
  --resource-group $RESOURCE_GROUP \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    Firebase__ProjectId=your-project-id \
    AgentService__BaseUrl=https://your-agent-service.com \
    Cors__AllowedOrigins__0=https://your-frontend.com

# Minimal API
az webapp config appsettings set \
  --name $APP_NAME_MINIMAL \
  --resource-group $RESOURCE_GROUP \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    Firebase__ProjectId=your-project-id \
    AgentService__BaseUrl=https://your-agent-service.com \
    Cors__AllowedOrigins__0=https://your-frontend.com
```

#### 4. Deploy
```bash
# Build and publish Controllers API
cd src/QuestionRandomizer.Api.Controllers
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip *
az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME_CONTROLLERS \
  --src ../deploy.zip

# Build and publish Minimal API
cd ../../QuestionRandomizer.Api.MinimalApi
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip *
az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME_MINIMAL \
  --src ../deploy.zip
```

### AWS Elastic Beanstalk

#### 1. Prerequisites
- AWS account
- AWS CLI configured
- EB CLI installed

#### 2. Initialize Application
```bash
# Initialize Elastic Beanstalk
eb init -p "64bit Amazon Linux 2023 v3.1.0 running .NET 10" question-randomizer --region us-east-1

# Create environment
eb create question-randomizer-env \
  --instance-type t3.small \
  --scale 1 \
  --envvars \
    ASPNETCORE_ENVIRONMENT=Production,\
    Firebase__ProjectId=your-project-id,\
    AgentService__BaseUrl=https://your-agent-service.com
```

#### 3. Deploy
```bash
# Deploy Controllers API
cd src/QuestionRandomizer.Api.Controllers
eb deploy

# Deploy Minimal API
cd ../QuestionRandomizer.Api.MinimalApi
eb deploy
```

### Kubernetes (K8s)

#### 1. Create Deployment Manifest (`k8s/deployment.yml`)
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: question-randomizer-controllers
spec:
  replicas: 2
  selector:
    matchLabels:
      app: question-randomizer-controllers
  template:
    metadata:
      labels:
        app: question-randomizer-controllers
    spec:
      containers:
      - name: api
        image: question-randomizer-controllers:latest
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: Firebase__ProjectId
          valueFrom:
            secretKeyRef:
              name: firebase-secrets
              key: project-id
```

#### 2. Deploy
```bash
# Apply deployment
kubectl apply -f k8s/deployment.yml

# Expose service
kubectl expose deployment question-randomizer-controllers \
  --type=LoadBalancer \
  --port=80 \
  --target-port=5000

# Check status
kubectl get pods
kubectl get services
```

---

## Environment Configuration

### Development
- **Port:** 5000 (Controllers), 5001 (Minimal API)
- **CORS:** http://localhost:4200
- **Logging:** Debug level
- **Swagger:** Enabled
- **Firebase:** Dev project

### Staging
- **Port:** 80/443 (via load balancer)
- **CORS:** https://staging.yourdomain.com
- **Logging:** Information level
- **Swagger:** Enabled (authenticated)
- **Firebase:** Staging project

### Production
- **Port:** 80/443 (via load balancer)
- **CORS:** https://yourdomain.com
- **Logging:** Warning level
- **Swagger:** Disabled
- **Firebase:** Production project

---

## Database Setup

### Firestore Configuration

1. **Create Firebase Project**
   - Go to [Firebase Console](https://console.firebase.google.com/)
   - Create new project or select existing
   - Enable Firestore Database

2. **Set Up Collections**
   Firestore will auto-create collections on first write. Collections:
   - `questions`
   - `categories`
   - `qualifications`
   - `conversations`
   - `messages`
   - `randomizations`

3. **Configure Security Rules**
   ```javascript
   rules_version = '2';
   service cloud.firestore {
     match /databases/{database}/documents {
       // Users can only access their own data
       match /questions/{questionId} {
         allow read, write: if request.auth != null &&
           request.auth.uid == resource.data.userId;
       }

       match /categories/{categoryId} {
         allow read, write: if request.auth != null &&
           request.auth.uid == resource.data.userId;
       }

       // ... (repeat for other collections)
     }
   }
   ```

4. **Generate Service Account Key**
   - Go to Project Settings → Service Accounts
   - Click "Generate New Private Key"
   - Download JSON file
   - Store securely (Azure Key Vault, AWS Secrets Manager)

---

## Monitoring & Logging

### Application Insights (Azure)

```bash
# Add Application Insights
dotnet add package Microsoft.ApplicationInsights.AspNetCore

# Configure in Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### CloudWatch (AWS)

```bash
# Add CloudWatch logging
dotnet add package AWS.Logger.AspNetCore

# Configure in appsettings.json
{
  "AWS.Logging": {
    "LogGroup": "question-randomizer",
    "Region": "us-east-1"
  }
}
```

### Health Checks

Health endpoint: `/health`

Returns:
- `200 OK` - Service healthy
- `503 Service Unavailable` - Service unhealthy

---

## Troubleshooting

### Issue: "Firebase credentials not found"

**Solution:**
```bash
# Verify file exists
ls firebase-credentials.json

# Verify path in appsettings
cat appsettings.Production.json | grep CredentialsPath

# For Docker, verify volume mount
docker-compose config
```

### Issue: "CORS errors in browser"

**Solution:**
```bash
# Check CORS configuration
# Ensure frontend URL is in AllowedOrigins
# Verify HTTPS in production

# Test CORS
curl -H "Origin: https://your-frontend.com" \
     -H "Access-Control-Request-Method: GET" \
     -H "Access-Control-Request-Headers: Content-Type" \
     -X OPTIONS \
     https://your-api.com/api/questions
```

### Issue: "Container won't start"

**Solution:**
```bash
# Check container logs
docker logs question-randomizer-controllers

# Check if port is in use
netstat -an | grep 5000

# Verify environment variables
docker exec question-randomizer-controllers env

# Run interactively for debugging
docker run -it --entrypoint /bin/bash question-randomizer-controllers:latest
```

### Issue: "Tests failing in CI/CD"

**Solution:**
```bash
# Run tests locally with same .NET version
dotnet --version

# Check test output
dotnet test --logger "console;verbosity=detailed"

# Verify packages restored
dotnet restore
```

---

## Rollback Procedure

### Docker Compose

```bash
# Stop current version
docker-compose down

# Pull previous version
docker pull question-randomizer-controllers:previous-tag

# Start with previous version
docker-compose up -d
```

### Azure App Service

```bash
# List deployment slots
az webapp deployment slot list --name $APP_NAME --resource-group $RESOURCE_GROUP

# Swap to previous slot
az webapp deployment slot swap \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --slot staging \
  --target-slot production
```

### Kubernetes

```bash
# Rollback deployment
kubectl rollout undo deployment/question-randomizer-controllers

# Check rollback status
kubectl rollout status deployment/question-randomizer-controllers
```

---

## Post-Deployment Checklist

- [ ] Health checks passing (`/health` returns 200)
- [ ] Swagger accessible (if enabled)
- [ ] Test endpoints with Postman/curl
- [ ] Check logs for errors
- [ ] Verify Firebase connection
- [ ] Test CORS with frontend
- [ ] Monitor CPU/Memory usage
- [ ] Set up alerts
- [ ] Document deployment in changelog

---

## Support

- **Issues:** https://github.com/your-org/question-randomizer-backend/issues
- **Documentation:** See `/docs` folder
- **Security:** security@yourdomain.com

---

**Last Updated:** 2025-11-30
