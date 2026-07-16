# Security Audit Checklist

## Pre-Deployment Security Checklist

### 🔒 Authentication & Authorization

- [ ] **Firebase Authentication configured correctly**
  - [ ] Email/password authentication enabled
  - [ ] API keys restricted to specific domains
  - [ ] Admin SDK credentials secured

- [ ] **Authorization checks in place**
  - [ ] All endpoints require authentication (except health checks)
  - [ ] UserId verification on all CRUD operations
  - [ ] Users can only access their own data

- [ ] **Token validation**
  - [ ] Firebase tokens validated on every request
  - [ ] Token expiration handled properly
  - [ ] Invalid tokens return 401 Unauthorized

### 🔐 Secrets Management

- [ ] **No secrets in source code**
  - [ ] Firebase credentials NOT committed to Git
  - [ ] .gitignore includes all credential files
  - [ ] Environment variables used for sensitive config

- [ ] **Production secrets secured**
  - [ ] Firebase credentials stored in secure vault (Azure Key Vault, AWS Secrets Manager)
  - [ ] Environment variables set in deployment environment
  - [ ] .env files NOT deployed to production

- [ ] **API keys protected**
  - [ ] Agent Service URLs not hardcoded
  - [ ] API keys rotated regularly
  - [ ] Keys have minimum required permissions

### 🛡️ Input Validation

- [ ] **All user input validated**
  - [ ] FluentValidation rules on all commands
  - [ ] String length limits enforced
  - [ ] SQL injection prevented (N/A - using Firestore)
  - [ ] XSS prevention (API returns JSON, frontend responsibility)

- [ ] **Data sanitization**
  - [ ] User input trimmed/normalized
  - [ ] Special characters handled
  - [ ] File uploads validated (N/A currently)

### 🌐 CORS Configuration

- [ ] **CORS properly configured**
  - [ ] AllowedOrigins set to specific domains (NOT "*")
  - [ ] Development: http://localhost:4200
  - [ ] Staging: https://staging.yourdomain.com
  - [ ] Production: https://yourdomain.com

- [ ] **CORS headers validated**
  - [ ] Access-Control-Allow-Origin specific
  - [ ] Access-Control-Allow-Methods limited
  - [ ] Access-Control-Allow-Headers specific

### 🔒 HTTPS & Transport Security

- [ ] **TLS/SSL configured**
  - [ ] HTTPS enforced in production
  - [ ] Valid SSL certificates
  - [ ] HTTP redirects to HTTPS

- [ ] **Security headers**
  - [ ] Strict-Transport-Security header set
  - [ ] X-Content-Type-Options: nosniff
  - [ ] X-Frame-Options: DENY
  - [ ] Content-Security-Policy configured

### 📊 Logging & Monitoring

- [ ] **Sensitive data NOT logged**
  - [ ] Passwords never logged
  - [ ] Tokens not logged in plain text
  - [ ] PII (Personally Identifiable Information) sanitized

- [ ] **Security events logged**
  - [ ] Authentication failures
  - [ ] Authorization failures
  - [ ] Invalid input attempts
  - [ ] Suspicious activity patterns

- [ ] **Log retention**
  - [ ] Logs retained for compliance period
  - [ ] Logs protected from tampering
  - [ ] Log access restricted

### 🐳 Docker Security

- [ ] **Docker images secured**
  - [ ] Using official .NET base images
  - [ ] Images scanned for vulnerabilities
  - [ ] Multi-stage builds used (smaller attack surface)
  - [ ] Non-root user in containers

- [ ] **Container configuration**
  - [ ] Read-only file systems where possible
  - [ ] Resource limits set (CPU, memory)
  - [ ] Secrets passed via environment variables
  - [ ] Health checks configured

### 🔄 Dependency Management

- [ ] **Dependencies up to date**
  - [ ] .NET SDK on supported version (10.0 LTS)
  - [ ] NuGet packages updated regularly
  - [ ] Known vulnerabilities addressed

- [ ] **Dependency scanning**
  - [ ] dotnet list package --vulnerable run regularly
  - [ ] GitHub Dependabot enabled
  - [ ] Critical vulnerabilities patched immediately

### 🚨 Error Handling

- [ ] **Errors don't leak sensitive information**
  - [ ] Generic error messages to users
  - [ ] Detailed errors only in logs
  - [ ] Stack traces disabled in production

- [ ] **Exception handling**
  - [ ] Global exception handler configured
  - [ ] All endpoints have try-catch where needed
  - [ ] Unhandled exceptions logged

### 🔍 Code Review

- [ ] **Security review completed**
  - [ ] No hardcoded credentials
  - [ ] No commented-out security code
  - [ ] No debug endpoints in production
  - [ ] No TODO: security comments unresolved

- [ ] **Code analysis**
  - [ ] Static code analysis run (Roslyn Analyzers)
  - [ ] SonarQube or similar tool used
  - [ ] Security-specific rules enabled

### 📋 Compliance

- [ ] **Data protection**
  - [ ] GDPR compliance if applicable
  - [ ] User data deletion implemented
  - [ ] Data export functionality available
  - [ ] Privacy policy in place

- [ ] **Audit trail**
  - [ ] User actions logged
  - [ ] Data changes tracked
  - [ ] Audit logs immutable

### 🔄 CI/CD Pipeline Security

- [ ] **Pipeline secured**
  - [ ] Secrets stored in GitHub Secrets / CI/CD vault
  - [ ] Build artifacts scanned
  - [ ] Deployment requires approval
  - [ ] Access to pipeline restricted

- [ ] **Automated security checks**
  - [ ] SAST (Static Application Security Testing)
  - [ ] Dependency scanning
  - [ ] Container scanning
  - [ ] Unit/Integration tests run before deploy

### 🌍 Infrastructure Security

- [ ] **Firestore security rules**
  - [ ] Read/write rules enforce authentication
  - [ ] Users can only access their own data
  - [ ] Admin operations restricted
  - [ ] Security rules tested

- [ ] **Network security**
  - [ ] APIs only accessible via HTTPS
  - [ ] Internal services not exposed
  - [ ] Firewall rules configured
  - [ ] DDoS protection in place

### 🔧 Production Configuration

- [ ] **Environment variables**
  - [ ] ASPNETCORE_ENVIRONMENT=Production
  - [ ] Debug mode disabled
  - [ ] Detailed errors disabled
  - [ ] Swagger disabled in production

- [ ] **Performance & availability**
  - [ ] Rate limiting configured
  - [ ] Request size limits set
  - [ ] Timeout values appropriate
  - [ ] Health checks configured

---

## Critical Security Issues (Must Fix Before Production)

1. **Firebase Credentials**
   - ⚠️ NEVER commit firebase-credentials.json to Git
   - ⚠️ Store in secure vault in production
   - ⚠️ Rotate credentials regularly

2. **CORS Configuration**
   - ⚠️ NEVER use AllowedOrigins: ["*"] in production
   - ⚠️ Set specific frontend domain

3. **HTTPS**
   - ⚠️ MUST use HTTPS in production
   - ⚠️ Redirect HTTP to HTTPS

4. **Logging**
   - ⚠️ NEVER log passwords, tokens, or sensitive PII
   - ⚠️ Sanitize logs before storage

5. **Error Messages**
   - ⚠️ Disable detailed error messages in production
   - ⚠️ Return generic errors to users

---

## Security Testing

### Penetration Testing
- [ ] OWASP Top 10 vulnerabilities tested
- [ ] SQL Injection (N/A - using Firestore)
- [ ] XSS attacks tested
- [ ] CSRF attacks tested
- [ ] Authentication bypass attempts
- [ ] Authorization bypass attempts

### Automated Security Scanning
```bash
# Check for vulnerable packages
dotnet list package --vulnerable

# Run security code analysis
dotnet build /p:EnableNETAnalyzers=true /p:AnalysisLevel=latest

# Docker image scanning
docker scan question-randomizer-controllers:latest
docker scan question-randomizer-minimal:latest
```

---

## Post-Deployment Monitoring

- [ ] **Security monitoring in place**
  - [ ] Failed authentication attempts tracked
  - [ ] Unusual access patterns detected
  - [ ] Rate limit violations logged
  - [ ] Alert system configured

- [ ] **Incident response plan**
  - [ ] Security incident procedures documented
  - [ ] Contact list for security issues
  - [ ] Backup and recovery tested
  - [ ] Rollback procedure documented

---

## Security Contacts

- **Security Issues:** [security@yourdomain.com]
- **On-Call Engineer:** [your-phone-number]
- **Incident Response Team:** [team-contact]

---

## Last Security Audit

- **Date:** [YYYY-MM-DD]
- **Auditor:** [Name]
- **Results:** [Pass/Fail]
- **Issues Found:** [Number]
- **Issues Resolved:** [Number]
- **Next Audit:** [YYYY-MM-DD]

---

## References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [ASP.NET Core Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Firebase Security Rules](https://firebase.google.com/docs/firestore/security/get-started)
