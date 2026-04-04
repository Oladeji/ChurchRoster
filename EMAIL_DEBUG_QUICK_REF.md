# 🚀 Quick Email Debug Commands

## 1️⃣ Check Email Configuration
```bash
curl http://localhost:8080/api/diagnostics/email-config
```

**Expected Response:**
```json
{
  "smtpHost": "smtp-relay.brevo.com",
  "smtpPort": "587",
  "username": "✓ SET (hidden)",
  "password": "✓ SET (hidden)",
  "senderEmail": "your-email@church.com",
  "senderName": "Church Ministry Roster",
  "frontendUrl": "http://localhost:5173",
  "isConfigured": true
}
```

## 2️⃣ Send Test Email
```bash
curl -X POST http://localhost:8080/api/diagnostics/test-email \
  -H "Content-Type: application/json" \
  -d "{\"toEmail\":\"YOUR_EMAIL_HERE\",\"toName\":\"Test User\"}"
```

**PowerShell Version:**
```powershell
Invoke-RestMethod -Uri "http://localhost:8080/api/diagnostics/test-email" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"toEmail":"YOUR_EMAIL_HERE","toName":"Test User"}'
```

## 3️⃣ Watch Logs in Real-Time

**Option A: Visual Studio Output Window**
1. View → Output
2. Select "Debug" from dropdown
3. Look for lines with emojis: 📧 ✅ ❌ ⚠️

**Option B: Terminal**
```powershell
cd backend/ChurchRoster.Api
dotnet run
# Watch the console output
```

## 4️⃣ Send Real Invitation via API

```bash
curl -X POST http://localhost:8080/api/invitations/send \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"member@example.com\",\"name\":\"John Doe\",\"phone\":\"555-1234\",\"role\":\"Member\"}"
```

**PowerShell Version:**
```powershell
$body = @{
    email = "member@example.com"
    name = "John Doe"
    phone = "555-1234"
    role = "Member"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8080/api/invitations/send" `
  -Method POST `
  -ContentType "application/json" `
  -Body $body
```

## 🔍 Log Patterns to Look For

### ✅ Success Pattern:
```
📧 API ENDPOINT: POST /api/invitations/send
=== SendInvitationAsync started ===
✅ No existing user or pending invitation found
✅ Invitation saved to database
📧 Attempting to send invitation email
=== Starting SendInvitationEmailAsync ===
>>> SendEmailAsync called
Creating SMTP client...
✅✅✅ EMAIL SENT SUCCESSFULLY ✅✅✅
```

### ❌ Credential Error Pattern:
```
❌ SMTP credentials are not configured!
  Username configured: False
  Password configured: False
```
**Fix:** Update appsettings.json with Brevo credentials

### ❌ Authentication Error Pattern:
```
❌ SMTP Exception:
  Status Code: GeneralFailure
  Message: authentication failed
```
**Fix:** Wrong username/password - check Brevo SMTP settings

### ❌ Connection Error Pattern:
```
❌ SMTP Exception:
  Message: Unable to connect to remote server
```
**Fix:** Check internet connection, firewall, or SMTP server/port

## 📧 Brevo Setup Checklist

- [ ] Login to [Brevo](https://app.brevo.com)
- [ ] Go to Settings → SMTP & API
- [ ] Copy your SMTP Login (username)
- [ ] Generate/copy SMTP API Key (password)
- [ ] Go to Senders
- [ ] Add your sender email
- [ ] Verify sender email (check your inbox)
- [ ] Update appsettings.json with credentials
- [ ] Restart your application

## 🎯 Most Common Issues

1. **Using account password instead of SMTP API key** ← Most common!
   - Password field needs the SMTP API key, not your login password

2. **Sender email not verified in Brevo**
   - Check Senders section and verify your email

3. **Missing configuration in appsettings.json**
   - Make sure EmailSettings section is complete

4. **Wrong SMTP server or port**
   - Should be: smtp-relay.brevo.com:587

## 💡 Pro Tips

- Test with `/api/diagnostics/test-email` first before real invitations
- Check Brevo dashboard for email statistics and errors
- Free Brevo plan allows 300 emails/day
- Emails appear in Brevo dashboard under Statistics → Email
- Check spam folder if email doesn't arrive

## 🆘 Still Not Working?

Run this diagnostic and share the output:

```powershell
# Check configuration
Invoke-RestMethod "http://localhost:8080/api/diagnostics/email-config"

# Try test email
Invoke-RestMethod -Uri "http://localhost:8080/api/diagnostics/test-email" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"toEmail":"your-email@example.com","toName":"Test"}'

# Copy the ENTIRE log output from your console/Visual Studio Output window
```

Then check `EMAIL_TROUBLESHOOTING.md` for detailed debugging steps.
