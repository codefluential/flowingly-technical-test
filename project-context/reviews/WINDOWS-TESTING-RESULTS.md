# Windows 11 Testing Results

**Date**: 2025-10-06
**Environment**: Windows 11
**Tester**: User (Adarsh)

---

## Issues Found & Resolved

### Issue 1: .NET SDK Version Mismatch ✅ RESOLVED

**Error**:
```
NETSDK1045: The current .NET SDK does not support targeting .NET 8.0.
Either target .NET 7.0 or lower, or use a version of the .NET SDK that supports .NET 8.0.
```

**Environment**:
- Windows had .NET SDK 7.0.201 installed
- Project targets .NET 8.0

**Resolution**:
- User installed .NET 8 SDK from https://dotnet.microsoft.com/download
- After installation, `dotnet run` succeeded

**Status**: ✅ **RESOLVED** - Prerequisites documentation correct, user just needed to upgrade

---

### Issue 2: Frontend/Backend Port Mismatch ✅ RESOLVED

**Error**:
```
net::ERR_CONNECTION_REFUSED
Failed to fetch: http://localhost:5000/api/v1/parse
```

**Root Cause**:
- **Frontend** (.env): Configured for `http://localhost:5000/api/v1`
- **Backend** (launchSettings.json): Running on `http://localhost:5161`
- Port mismatch caused connection refused

**Resolution**:
- Updated `src/Api/Properties/launchSettings.json`
- Changed `http` profile: 5161 → 5000
- Changed `https` profile: 5161 → 5000, 7101 → 7000
- Committed fix: `72a7c0f`

**Impact**:
- Frontend can now successfully call backend
- Matches documented port in README
- Consistent with all examples and CORS config

**Status**: ✅ **RESOLVED** - Backend now runs on port 5000

---

## Successful Configuration

### Final Working Setup

**Backend**:
```powershell
PS C:\temp\flowingly\flowingly-technical-test> dotnet run --project src/Api
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Frontend**:
```powershell
PS C:\temp\flowingly\flowingly-technical-test\client> npm run dev
  VITE v7.1.9  ready in 245 ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
```

**API Connectivity**:
- ✅ Frontend successfully calls `http://localhost:5000/api/v1/parse`
- ✅ CORS allows origin `http://localhost:5173`
- ✅ Swagger accessible at `http://localhost:5000/swagger`

---

## Testing Status

### Prerequisites Installation ✅ PASS

**Windows 11 Experience**:
1. **.NET 8 SDK**: Downloaded installer, ran, auto-configured PATH ✅
2. **Node.js 18+**: Downloaded installer, ran, auto-configured PATH ✅
3. **Git**: Already installed with Git Bash ✅

**Observations**:
- ✅ Installers auto-configure PATH (no manual setup needed)
- ✅ README prerequisites accurate for Windows
- ✅ No `~/.bashrc` confusion (Windows notes helpful)

---

### Quick Start Steps ✅ PASS

**PowerShell Testing**:

**Step 1**: Clone repository
```powershell
git clone <url>
cd flowingly-technical-test
```
**Result**: ✅ Works

**Step 2**: Run backend
```powershell
dotnet run --project src/Api
```
**Result**: ✅ Works after .NET 8 SDK installed
**Output**: `Now listening on: http://localhost:5000` ✅

**Step 3**: Run frontend (new terminal)
```powershell
cd client
npm install
npm run dev
```
**Result**: ✅ Works
**Output**: `Local: http://localhost:5173/` ✅

**Step 4**: Test application
- Swagger: http://localhost:5000/swagger ✅
- Frontend UI: http://localhost:5173 ✅
- API connectivity: ✅ Working after port fix

---

### Sample Email Testing ✅ PASS

**Tested via Swagger UI**:

**Sample 1** (XML Island):
```json
{
  "text": "Hi Yvaine, Please create an expense claim for the below. Relevant details are:\n<expense><cost_centre>DEV002</cost_centre><total>1024.01</total><payment_method>personal card</payment_method></expense>",
  "taxRate": 0.15
}
```
**Result**: ✅ HTTP 200
- Vendor: null
- Total: 1024.01
- TotalExclTax: 890.44
- SalesTax: 133.57
- CostCentre: "DEV002"
- PaymentMethod: "personal card"

**Sample 2** (Inline Tags):
```json
{
  "text": "Hi Yvaine, <vendor>Mojo Coffee</vendor> <total>120.50</total> <payment_method>personal card</payment_method>",
  "taxRate": 0.15
}
```
**Result**: ✅ HTTP 200
- Vendor: "Mojo Coffee"
- Total: 120.50
- TotalExclTax: 104.78
- SalesTax: 15.72

**Sample 3** (Error - Missing Total):
```json
{
  "text": "Hi Yvaine, <vendor>Starbucks</vendor> <payment_method>personal card</payment_method>",
  "taxRate": 0.15
}
```
**Result**: ✅ HTTP 400
- ErrorCode: "MISSING_TOTAL"
- Message: "<total> tag is required for expense processing"
- Correlation ID present

---

### Development Commands ✅ PASS

**PowerShell Testing**:

```powershell
# Build
dotnet build
# Result: ✅ Succeeded, 0 warnings, 0 errors

# Test
dotnet test
# Result: ✅ 129 tests passed (116 unit + 13 contract)

# Frontend build
cd client
npm run build
# Result: ✅ Production build succeeded (198KB bundle)
```

---

## README Accuracy Assessment

### Quick Start Section ✅ ACCURATE

**Expectations vs Reality**:
- ✅ Prerequisites: Accurate - installers auto-configure PATH
- ✅ Clone instructions: Work identically
- ✅ Backend command: Works after .NET 8 install
- ✅ Frontend command: Works identically
- ✅ Expected outputs: Match actual outputs
- ✅ Verification URLs: All accessible

**Windows-Specific Notes**:
- ✅ "Installers automatically configure PATH" - TRUE
- ✅ "No manual setup needed" - TRUE
- ✅ "Works on: Windows, Linux, macOS" - TRUE

---

### Appendix Sections ✅ HELPFUL

**Appendix A (Manual PATH Configuration)**:
- Not needed on Windows (as documented) ✅
- Correctly labeled as "rare, installers handle this"

**Appendix B (Git Bash on Windows)**:
- User has Git Bash installed
- All Linux commands work identically in Git Bash ✅

**Appendix C (Shell Syntax)**:
- PowerShell 7+ supports `&&` - confirmed ✅
- Helpful for cmd.exe users

**Appendix D (Development Scripts)**:
- Scripts work in Git Bash ✅
- Correctly documented as Unix/Git Bash/WSL

---

## Issues Requiring Attention

### Critical Issues ✅ ALL RESOLVED
1. ~~Port mismatch (5161 vs 5000)~~ → Fixed in commit 72a7c0f
2. ~~.NET SDK version~~ → User upgraded to .NET 8

### Documentation Improvements

**No critical changes needed**, but could enhance:

1. **Add .NET 8 version check** to Prerequisites:
   ```markdown
   Verify .NET 8 SDK installed:
   ```powershell
   dotnet --version
   # Should output: 8.0.xxx
   ```
   If < 8.0, download from https://dotnet.microsoft.com/download
   ```

2. **Add troubleshooting note** for NETSDK1045 error:
   ```markdown
   **If you see** `NETSDK1045: The current .NET SDK does not support targeting .NET 8.0`:
   - You have .NET 7 or earlier installed
   - Download .NET 8 SDK from link above
   - Restart terminal after installation
   ```

---

## Performance Observations

**Windows 11 Performance**:
- **Backend startup**: ~2 seconds
- **Frontend startup**: ~3 seconds with npm install, ~1 second with hot reload
- **API response time**: < 50ms (similar to Linux)
- **Test suite**: ~5 seconds (129 tests)

**No performance degradation on Windows** compared to Linux/WSL2.

---

## Cross-Platform Verification

### Commands Tested

| Command | PowerShell | cmd.exe | Git Bash | Status |
|---------|-----------|---------|----------|--------|
| `git clone` | ✅ | ✅ | ✅ | Works |
| `cd project` | ✅ | ✅ | ✅ | Works |
| `dotnet build` | ✅ | ✅ | ✅ | Works |
| `dotnet test` | ✅ | ✅ | ✅ | Works |
| `npm install` | ✅ | ✅ | ✅ | Works |
| `npm run dev` | ✅ | ✅ | ✅ | Works |

### Shell-Specific Syntax

**Chained Commands**:
- PowerShell 7+: `cd client && npm run dev` ✅ Works
- PowerShell 5.1: Use `;` instead: `cd client; npm run dev`
- cmd.exe: Use single `&`: `cd client & npm run dev`
- Git Bash: `&&` works ✅

---

## Final Assessment

### Overall Grade: ✅ **PASS** (After Port Fix)

**Summary**:
- ✅ Application runs successfully on Windows 11
- ✅ All 3 sample emails from test brief work
- ✅ 129 tests pass
- ✅ Frontend/backend communication working
- ✅ README instructions accurate
- ✅ Prerequisites section helpful
- ✅ Cross-platform appendices useful

**Recommendations**:
1. ✅ Port fix committed (72a7c0f) - **DEPLOYED**
2. 📋 Consider adding .NET version check to Prerequisites
3. 📋 Consider adding NETSDK1045 troubleshooting note

**Status**: ✅ **PRODUCTION-READY FOR WINDOWS USERS**

---

## Testing Checklist Summary

- [x] Windows 11 environment verified
- [x] Prerequisites installed (.NET 8, Node.js, Git)
- [x] Backend runs on Windows
- [x] Frontend runs on Windows
- [x] API connectivity working
- [x] All 3 sample emails tested
- [x] 129 tests passing
- [x] Swagger UI accessible
- [x] Frontend UI functional
- [x] Cross-platform commands verified
- [x] README accuracy confirmed
- [x] Port mismatch resolved
- [x] Documentation helpful for Windows users

---

**Tested By**: User (Adarsh)
**Date**: 2025-10-06
**Environment**: Windows 11 (native), PowerShell 7+
**Result**: ✅ **PASS** - Ready for external reviewers on Windows
