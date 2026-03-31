# 📄 Church Ministry Rostering System  
## Technical Architecture & Hosting Strategy  
**Version 1.1**  
*Copy this entire document and paste into Word, Google Docs, or save as .txt/.md*

---

## 🎯 1. PURPOSE
This document outlines the technical stack, hosting architecture, and implementation plan for the **Church Ministry Rostering System**. The strategy prioritizes **zero monthly overhead**, leverages the development team's existing skill set (**C#, TypeScript, React**), and utilizes a **Progressive Web App (PWA)** approach to avoid mobile app store fees.

> ⚠️ **Update Note (v1.1):**  
> Backend API updated to **.NET 10** to match development environment.

---

## 🛠️ 2. RECOMMENDED TECHNOLOGY STACK

| Component | Technology | Justification |
|-----------|------------|---------------|
| **Backend API** | **.NET 10 Web API** (C#) | Leverages existing C# skills. Strong typing, robust security, excellent performance. |
| **Frontend (Web)** | **React + TypeScript** (Vite) | Leverages existing React/TS skills. Fast build times, modern ecosystem. |
| **Mobile Experience** | **Progressive Web App (PWA)** | **Zero Cost.** Uses same React codebase. No App Store fees ($99/yr Apple + $25/yr Google). Supports Push Notifications. |
| **Database** | **PostgreSQL** (via **Supabase**) | Free tier includes 500MB DB. Managed service (no server maintenance). Includes Auth & API tools. |
| **Authentication** | **Supabase Auth** | Secure, free tier supports 50k monthly active users. Saves weeks of security coding. |
| **Frontend Hosting** | **Vercel** or **Netlify** | Free tier includes SSL, CDN, and auto-deployment from GitHub. Optimized for React. |
| **Backend Hosting** | **Render** | Free tier supports Docker containers (.NET). Easy setup via GitHub integration. |
| **Push Notifications** | **Firebase Cloud Messaging (FCM)** | Free unlimited pushes. Compatible with Web PWA (Android & iOS 16.4+). |
| **Email Service** | **Brevo (Sendinblue)** | Free tier allows 300 emails/day. Sufficient for roster alerts and notifications. |
| **Domain Name** | **Namecheap / GoDaddy** | Required for SSL & Trust. (~$12/year). |

---

## 🏗️ 3. HOSTING ARCHITECTURE (ZERO COST SETUP)

### 3.1 High-Level Diagram
```plaintext
[User Devices] (Mobile/Web)
       │
       ▼
[Frontend: React PWA] ────────► [Vercel/Netlify] (Free)
       │                             │
       │ (HTTPS API Calls)           │
       ▼                             ▼
[Backend: .NET 10 API] ────────► [Render] (Free Docker Container)
       │                             │
       │ (DB Connection)             │ (Email via SMTP)
       ▼                             ▼
[Database: PostgreSQL] ───────► [Supabase] (Free Tier)
       │
       │ (Push Payload)
       ▼
[Push Service] ───────────────► [Firebase FCM] (Free)
```

### 3.2 Component Details

#### **A. Frontend (Admin Web + Member PWA)**
*   **Platform:** **Vercel**
*   **Configuration:** Connect GitHub repo. Auto-deploy on push.
*   **PWA Setup:** Configure `manifest.json` and Service Workers so members can "Add to Home Screen" without an App Store.
*   **Cost:** $0/month.

#### **B. Backend API (.NET 10)**
*   **Platform:** **Render**
*   **Configuration:** Dockerize the .NET 10 API (`Dockerfile`). Render builds and runs the container.
*   **Cost:** $0/month (Free Web Service).
*   **Note:** The service may "sleep" after 15 mins of inactivity. First request after sleep takes ~30 seconds to wake up.
*   **Compatibility:** Ensure Docker base images reference `.NET 10`.

#### **C. Database**
*   **Platform:** **Supabase**
*   **Configuration:** Create project, get PostgreSQL connection string.
*   **Cost:** $0/month (500MB Storage).
*   **Note:** 500MB is sufficient for text-based roster data for several years.

#### **D. Notifications**
*   **Push:** **Firebase** (Free). Backend sends HTTP request to Firebase → Firebase pushes to User Device.
*   **Email:** **Brevo** (Free). Backend sends SMTP email via Brevo for critical alerts or older iOS devices.

---

## 💰 4. COST BREAKDOWN (ESTIMATED)

| Item | Service | Cost | Frequency | Notes |
|------|---------|------|-----------|-------|
| **Domain Name** | Namecheap / GoDaddy | ~$12.00 | **Per Year** | **Unavoidable.** Required for SSL & Trust. |
| **Frontend Hosting** | Vercel | $0.00 | Monthly | Free Hobby Tier. |
| **Backend Hosting** | Render | $0.00 | Monthly | Free Web Service. |
| **Database** | Supabase | $0.00 | Monthly | Free Tier (500MB). |
| **Mobile App Stores** | **N/A (PWA)** | **$0.00** | **Yearly** | **Saving $124/yr** (Apple + Google Fees). |
| **Email** | Brevo | $0.00 | Monthly | Free Tier (300 emails/day). |
| **Push Notifications** | Firebase | $0.00 | Monthly | Unlimited. |
| **SSL Certificate** | Vercel/Render | $0.00 | Monthly | Included automatically. |
| **Total Monthly Cost** | | **$0.00** | | |
| **Total Yearly Cost** | | **~$12.00** | | (Domain renewal only) |

---

## ⚠️ 5. IMPORTANT CONSIDERATIONS & TRADE-OFFS

| Area | Consideration | Mitigation Strategy |
|------|---------------|---------------------|
| **.NET 10 Support** | Ensure hosting provider supports .NET 10 runtime. | Using **Docker** on Render ensures compatibility regardless of host OS, as long as the Microsoft base image exists. |
| **Backend Cold Starts** | Render free tier sleeps after 15 mins inactivity. First request takes ~30 secs. | Acceptable for admin tasks. Use **UptimeRobot** (free) to ping API every 5 mins if instant response is critical. |
| **iOS Push Notifications** | PWAs on iOS only receive pushes if: <br>1. Added to Home Screen<br>2. iOS 16.4+ | Instruct members to "Add to Home Screen" during onboarding. Use **Email** as fallback for older iOS versions. |
| **Database Backups** | Supabase free tier includes backups, but data ownership is key. | Export SQL dumps monthly to local storage as a safety precaution. |
| **App Store Distribution** | No native app icon in Apple/Google Store. | Educate users: "Visit link → Add to Home Screen". Creates same icon experience without fees. |
| **Email Limits** | Brevo free tier limits to 300 emails/day. | Sufficient for church roster. If exceeding, batch notifications or upgrade plan (~$25/mo). |
| **Security** | Free tiers share infrastructure. | Ensure HTTPS everywhere. Use Supabase Auth for secure password handling. Never store secrets in frontend code. |

---

## 🚀 6. IMPLEMENTATION PLAN (STEP-BY-STEP)

### **Phase 1: Foundation (Week 1)**
1.  **Domain:** Purchase domain (e.g., `churchroster.com`).
2.  **Database:** Set up project on **Supabase**. Run SQL schema scripts. Get Connection String.
3.  **Backend:**
    *   Initialize **.NET 10 Web API** solution.
    *   Install `Npgsql.EntityFrameworkCore.PostgreSQL`.
    *   Connect to Supabase DB.
    *   Implement JWT Auth (or integrate Supabase Auth SDK).
    *   Create `Dockerfile` for deployment (Ensure base image is `mcr.microsoft.com/dotnet/aspnet:10.0`).
4.  **Deploy Backend:** Push code to GitHub → Connect to **Render** → Deploy.

### **Phase 2: Frontend & PWA (Week 2)**
1.  **Initialize:** Create **React + TypeScript** app using **Vite**.
2.  **PWA Config:** Install `vite-plugin-pwa`. Configure `manifest.json` (icons, name, theme).
3.  **UI Development:** Build Admin Dashboard (Calendar, Assignment) and Member View (Task List).
4.  **Deploy Frontend:** Push code to GitHub → Connect to **Vercel** → Deploy.
5.  **Connect:** Point Frontend to Render Backend API URL.

### **Phase 3: Notifications (Week 3)**
1.  **Firebase:** Create Firebase project. Get VAPID keys.
2.  **Frontend:** Install `firebase` SDK. Implement `requestPermission()` and `getToken()`. Save token to Backend `Users` table.
3.  **Backend:** Install `FirebaseAdmin` SDK. Create service to send push messages when assignment status changes.
4.  **Email:** Set up **Brevo** account. Get SMTP credentials. Configure .NET `MailService`.

### **Phase 4: Testing & Launch (Week 4)**
1.  **Functional Test:** Verify assignment flow (Admin assigns → Member gets Push → Member Accepts → Admin sees update).
2.  **Mobile Test:** Test "Add to Home Screen" on Android & iOS. Verify Push Notifications work on both.
3.  **Security Check:** Ensure API endpoints are protected. Verify role-based access (Admin vs. Member).
4.  **Training:** Create a simple 1-page guide for members: *"How to Install the App on Your Phone"*.
5.  **Go Live:** Share domain link with church administration and members.

---

## 📎 7. APPENDIX: .NET 10 DOCKERFILE TEMPLATE

*Use this template for your Backend Deployment to ensure .NET 10 compatibility.*

```dockerfile
# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ChurchRoster.Api/ChurchRoster.Api.csproj", "ChurchRoster.Api/"]
RUN dotnet restore "ChurchRoster.Api/ChurchRoster.Api.csproj"
COPY . .
WORKDIR "/src/ChurchRoster.Api"
RUN dotnet build "ChurchRoster.Api.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "ChurchRoster.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChurchRoster.Api.dll"]
```

---

## 📥 8. HOW TO SAVE THIS DOCUMENT

**Option 1: Google Docs / Microsoft Word**
1.  Select all text above (Ctrl+A or Cmd+A)
2.  Copy (Ctrl+C or Cmd+C)
3.  Open Google Docs or Word
4.  Paste (Ctrl+V or Cmd+V)
5.  File → Download → PDF or .docx

**Option 2: Save as Text File**
1.  Select all text above
2.  Copy
3.  Open Notepad (Windows) or TextEdit (Mac)
4.  Paste
5.  File → Save As → `Technical_Architecture_Strategy_v1.1.txt`

**Option 3: Save as Markdown (for tech teams)**
1.  Copy all text
2.  Paste into a new file named `architecture_v1.1.md`
3.  Open with any code editor or Markdown viewer

---

> 🙏 **You're all set!**  
> This technical strategy (v1.1) is updated for **.NET 10**.  
> It ensures you can build a professional, scalable system using your **C# and React** skills while keeping monthly costs at **$0**.  
> **Next Step:** Begin Phase 1 (Database & Backend Setup).

*Document Version: 1.1 | Last Updated: March 2026 | Prepared for: Development Team*