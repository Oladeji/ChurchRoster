# Email Invitation Troubleshooting Guide

## 🔍 Comprehensive Logging Added

I've added extensive logging throughout the email invitation system to help diagnose issues. Here's how to use it:

## 📊 Check Email Configuration

### 1. API Endpoint to View Configuration
```
GET http://localhost:8080/api/diagnostics/email-config
```

This will show you:
- SMTP Host
- SMTP Port
- Whether Username is configured (hidden for security)
- Whether Password is configured (hidden for security)
- Sender Email
- Sender Name
- Frontend URL
- Overall configuration status

### 2. Test Email Endpoint
```
POST http://localhost:8080/api/diagnostics/test-email
Content-Type: application/json

{
  "toEmail": "your-email@example.com",
  "toName": "Your Name"
}
```

This will:
- Attempt to send a test invitation email
- Show detailed logs of the entire process
- Return success/failure status

## 🔎 Log Levels and What to Look For

### When EmailService Initializes
Look for:
```
EmailService initialized with:
  SMTP Host: smtp-relay.brevo.com
  SMTP Port: 587
  From Email: your-configured-email
  Username Configured: True
  Password Configured: True
```

**⚠️ WARNING Signs:**
```
⚠️ SMTP credentials are not configured! Email sending will fail.
Please configure EmailSettings:Username and EmailSettings:Password in appsettings.json
```

### When Sending an Invitation
Look for this sequence:

1. **API Endpoint Entry:**
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📧 API ENDPOINT: POST /api/invitations/send
Email: user@example.com, Name: John Doe
```

2. **InvitationService Processing:**
```
=== SendInvitationAsync started ===
✅ No existing user or pending invitation found
Generated invitation token: Ab12Cd34...
✅ Invitation saved to database with ID: 5
📧 Attempting to send invitation email to user@example.com...
```

3. **EmailService Processing:**
```
=== Starting SendInvitationEmailAsync ===
Recipient: user@example.com, Name: John Doe
Accept URL: http://localhost:5173/accept-invitation?token=...
```

4. **SMTP Details:**
```
>>> SendEmailAsync called
  To: user@example.com (John Doe)
  Subject: You're Invited to Join Church Ministry Roster
Creating email message...
Email message created successfully
Creating SMTP client...
  Host: smtp-relay.brevo.com
  Port: 587
  Username: your-username
  SSL Enabled: true
Attempting to send email via SMTP...
```

5. **Success:**
```
✅✅✅ EMAIL SENT SUCCESSFULLY to user@example.com ✅✅✅
✅ Invitation email sent successfully to user@example.com
✅✅ Invitation email sent successfully to user@example.com
✅ Invitation created successfully: ID=5
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## ❌ Common Error Messages

### 1. Credentials Not Configured
```
❌ SMTP credentials are not configured!
  Username configured: False
  Password configured: False
```
**Fix:** Update `appsettings.json` with your Brevo credentials.

### 2. SMTP Authentication Failed
```
❌ SMTP Exception:
  Status Code: GeneralFailure
  Message: The SMTP server requires a secure connection...
```
**Fix:** Check your Brevo username and password are correct.

### 3. Failed Recipient
```
❌ SMTP Failed Recipient Exception:
  Failed Recipient: user@invalid-domain.com
  Status Code: MailboxUnavailable
```
**Fix:** Email address doesn't exist. Check the recipient email.

### 4. Connection Timeout
```
❌ SMTP Exception:
  Message: Unable to connect to remote server
```
**Fix:** 
- Check your internet connection
- Verify SMTP host and port are correct
- Check firewall settings

## 🔧 Configuration Checklist

### Step 1: Verify appsettings.json

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp-relay.brevo.com",
    "SmtpPort": 587,
    "SenderEmail": "your-verified-email@yourdomain.com",
    "SenderName": "Church Ministry Roster",
    "Username": "your-brevo-login-email",
    "Password": "your-brevo-smtp-api-key"
  },
  "App": {
    "FrontendUrl": "http://localhost:5173"
  }
}
```

### Step 2: Get Brevo SMTP Credentials

1. Log in to [Brevo](https://app.brevo.com)
2. Go to **Settings** → **SMTP & API**
3. Click **SMTP** tab
4. Copy your **Login** (Username)
5. Generate or copy your **SMTP Key** (Password)

**Important Notes:**
- The **Username** is your Brevo login email
- The **Password** is the **SMTP API key**, NOT your account password
- You must verify your sender email in Brevo before sending

### Step 3: Verify Sender Email in Brevo

1. Go to **Senders** in Brevo dashboard
2. Add and verify your sender email address
3. Check your email for verification link
4. Click the verification link

**Until verified, all emails will fail!**

## 🧪 Testing Process

### 1. Stop Your Application
Make sure the app is stopped to rebuild with new logging.

### 2. Rebuild and Start
```powershell
dotnet build
dotnet run --project backend/ChurchRoster.Api
```

### 3. Check Initialization Logs
Look for the EmailService initialization logs showing your configuration.

### 4. Test Configuration Endpoint
```bash
curl http://localhost:8080/api/diagnostics/email-config
```

### 5. Send Test Email
```bash
curl -X POST http://localhost:8080/api/diagnostics/test-email \
  -H "Content-Type: application/json" \
  -d '{"toEmail":"your-email@example.com","toName":"Test User"}'
```

### 6. Check Application Logs
Watch the console output for detailed logs showing each step.

### 7. Try Real Invitation
Once test email works, try sending a real invitation through the Members page.

## 🐛 Debugging Tips

### Enable Debug Logging
In `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "ChurchRoster": "Debug"
    }
  }
}
```

### Check Visual Studio Output Window
1. Go to **View** → **Output**
2. Select **Debug** from dropdown
3. Look for the detailed email logs

### Common Brevo Issues

1. **Using Account Password Instead of SMTP Key**
   - Don't use your Brevo account password
   - Generate a new SMTP API key in Settings → SMTP & API

2. **Unverified Sender Email**
   - Brevo requires sender email verification
   - Check Senders section in dashboard

3. **Free Plan Limitations**
   - Free plan: 300 emails/day
   - Check your usage in Brevo dashboard

4. **Wrong SMTP Server**
   - Make sure it's `smtp-relay.brevo.com` (not smtp-relay.sendinblue.com)
   - Port should be `587` with TLS/SSL enabled

## 📝 What Information to Provide if Still Not Working

If emails still aren't sending after following this guide, please provide:

1. **Configuration Status:**
   - Output from `/api/diagnostics/email-config`

2. **Test Email Result:**
   - Output from `/api/diagnostics/test-email`

3. **Application Logs:**
   - Copy the entire log output from when you send an invitation
   - Look for lines starting with ===, >>>, ✅, or ❌

4. **Brevo Account Status:**
   - Is your sender email verified?
   - Have you generated an SMTP API key?
   - What's your daily email quota usage?

## 🎯 Expected Successful Flow

When everything works correctly, you should see:

```
EmailService initialized with:
  SMTP Host: smtp-relay.brevo.com
  SMTP Port: 587
  Username Configured: True
  Password Configured: True

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📧 API ENDPOINT: POST /api/invitations/send
Email: member@church.com, Name: John Doe

=== SendInvitationAsync started ===
✅ No existing user or pending invitation found
✅ Invitation saved to database with ID: 1

=== Starting SendInvitationEmailAsync ===
>>> SendEmailAsync called
Creating SMTP client...
Attempting to send email via SMTP...
✅✅✅ EMAIL SENT SUCCESSFULLY to member@church.com ✅✅✅

✅✅ Invitation email sent successfully
✅ Invitation created successfully: ID=1
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

Then the recipient receives an email with:
- Professional HTML template
- "Accept Invitation" button
- Link to: `http://localhost:5173/accept-invitation?token=...`
- 7-day expiry notice
