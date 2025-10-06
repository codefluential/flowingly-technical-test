# Windows 11 Testing Checklist

**Date**: 2025-10-06
**Purpose**: Verify README instructions work on Windows 11
**Tester**: User (running on Windows 11 natively)

---

## Pre-Test Setup

### Verify Windows Environment

- [ ] **Windows Version**: Run `winver` to confirm Windows 11
- [ ] **PowerShell Version**: Run `$PSVersionTable.PSVersion` in PowerShell
- [ ] **Git Bash Installed**: Check if Git for Windows installed (optional but recommended)

### Required Software Installation

Test the Prerequisites section instructions:

- [ ] **.NET 8 SDK**: Download and install from https://dotnet.microsoft.com/download
  - [ ] Run installer (should auto-configure PATH)
  - [ ] Verify: Open new PowerShell, run `dotnet --version`
  - [ ] Expected: `8.0.xxx` output

- [ ] **Node.js 18+**: Download and install from https://nodejs.org/
  - [ ] Run installer (should auto-configure PATH)
  - [ ] Verify: Open new PowerShell, run `node --version`
  - [ ] Expected: `v18.x.x` or `v20.x.x` output

- [ ] **Git**: Download and install from https://git-scm.com/download/win
  - [ ] Run installer
  - [ ] Verify: Open new PowerShell, run `git --version`
  - [ ] Expected: `git version 2.x.x` output

---

## Test 1: Quick Start in PowerShell

### Step 1: Clone Repository

```powershell
git clone <repository-url>
cd flowingly-technical-test
```

**Verification**:
- [ ] Repository cloned successfully
- [ ] No errors displayed
- [ ] Can `cd` into directory

---

### Step 2: Run Backend API

**Open PowerShell Terminal 1**:

```powershell
dotnet run --project src/Api
```

**Verification**:
- [ ] Command executes without errors
- [ ] See output: `Now listening on: http://localhost:5000`
- [ ] No PATH errors (no `dotnet: command not found`)
- [ ] Open http://localhost:5000/swagger in browser
- [ ] Swagger UI loads successfully
- [ ] See `/api/v1/parse` endpoint

**Leave running** (don't close terminal)

---

### Step 3: Run Frontend

**Open PowerShell Terminal 2**:

```powershell
cd client
npm install
npm run dev
```

**Verification**:
- [ ] `npm install` completes without errors
- [ ] `npm run dev` starts Vite server
- [ ] See output: `Local: http://localhost:5173/`
- [ ] Open http://localhost:5173 in browser
- [ ] UI loads with text input area
- [ ] No console errors in browser dev tools

**Leave running** (don't close terminal)

---

### Step 4: Test Expense Parsing

**Option 1 - Swagger UI**:
1. Open http://localhost:5000/swagger
2. Click POST `/api/v1/parse` → "Try it out"
3. Paste:
   ```json
   {
     "text": "Hi Yvaine, Please create an expense claim. <vendor>Mojo Coffee</vendor> <total>120.50</total> <cost_centre>DEV</cost_centre>",
     "taxRate": 0.15
   }
   ```
4. Click "Execute"

**Verification**:
- [ ] Request succeeds (HTTP 200)
- [ ] Response shows parsed expense data
- [ ] Vendor: "Mojo Coffee"
- [ ] Total: 120.50
- [ ] TotalExclTax: 104.78
- [ ] SalesTax: 15.72
- [ ] Correlation ID present

**Option 2 - Frontend UI**:
1. Open http://localhost:5173
2. Paste sample expense text in textarea
3. Click "Parse" button

**Verification**:
- [ ] Parse button works
- [ ] Response displays parsed data
- [ ] Correlation ID shows in result

---

## Test 2: Quick Start in Windows cmd.exe

Repeat Test 1 steps using Windows Command Prompt (cmd.exe) instead of PowerShell.

### Known Differences

In cmd.exe, chained commands use `&` not `&&`:
```cmd
cd client & npm install & npm run dev
```

**Verification**:
- [ ] All steps work identically
- [ ] `dotnet run` works
- [ ] `npm install` works
- [ ] `npm run dev` works
- [ ] Both servers start successfully

---

## Test 3: Quick Start in Git Bash

If Git Bash installed, repeat Test 1 using Git Bash terminal.

**Verification**:
- [ ] All Linux commands work identically
- [ ] `./` scripts work (if tested)
- [ ] Forward slashes work in paths
- [ ] Servers start successfully

---

## Test 4: Development Workflow Commands

Test the "Backend Commands" section:

```bash
# In project root
dotnet build
dotnet test
dotnet test --filter Category=Unit
dotnet test --filter Category=Contract
```

**Verification**:
- [ ] `dotnet build` succeeds (0 warnings, 0 errors)
- [ ] `dotnet test` runs all tests
- [ ] Unit tests: 116 passing
- [ ] Contract tests: 13 passing
- [ ] Total: 129 tests passing
- [ ] No test failures

---

## Test 5: Frontend Development Commands

Test the "Frontend Commands" section:

```bash
cd client
npm run build
npm run preview
```

**Verification**:
- [ ] `npm run build` creates production build
- [ ] Build output shows `dist/` directory created
- [ ] No TypeScript errors
- [ ] `npm run preview` starts preview server

---

## Test 6: README Accuracy

Verify README claims match actual behavior:

- [ ] **Prerequisites section**: Clear and accurate for Windows?
- [ ] **Platform notes**: Windows-specific info helpful?
- [ ] **Quick Start**: Steps work as documented?
- [ ] **Expected outputs**: Match what you see?
- [ ] **Verification steps**: URLs work as documented?
- [ ] **"Works on: Windows" notes**: Accurate?

---

## Test 7: Appendix Sections

Check if appendix sections are needed or helpful:

- [ ] **Appendix A (PATH Config)**: Did you need manual PATH setup? (Should be NO for Windows)
- [ ] **Appendix B (Git Bash)**: If Git Bash installed, info accurate?
- [ ] **Appendix C (Shell Syntax)**: PowerShell/cmd.exe differences helpful?
- [ ] **Appendix D (Scripts)**: Clear that scripts are Unix-only?

---

## Common Issues to Watch For

### Issue 1: PATH Not Configured
**Symptom**: `dotnet: command not found` or `npm: command not found`
**Expected**: Should NOT happen with official installers
**If it happens**: Document which installer was used

### Issue 2: Port Already in Use
**Symptom**: `Address already in use` error
**Solution**:
- Check if another app using port 5000 or 5173
- Close other instances
- Try again

### Issue 3: Node Modules Issues
**Symptom**: `npm install` errors, missing dependencies
**Solution**:
- Delete `node_modules/` and `package-lock.json`
- Run `npm install` again

### Issue 4: Firewall Warnings
**Symptom**: Windows Firewall blocks dotnet/node
**Solution**: Allow access when prompted

---

## Test Results Summary

### Overall Assessment

- [ ] **PASS**: All Quick Start steps work on Windows 11
- [ ] **PASS WITH NOTES**: Works but with minor issues (document below)
- [ ] **FAIL**: Critical issues prevent running app (document below)

### Environment Details

**Windows Version**: _________________

**PowerShell Version**: _________________

**Shells Tested**:
- [ ] PowerShell 7+
- [ ] PowerShell 5.1
- [ ] cmd.exe
- [ ] Git Bash

### Installation Methods

**.NET 8**:
- [ ] Official installer (recommended)
- [ ] Other: _________________

**Node.js**:
- [ ] Official installer (recommended)
- [ ] Other: _________________

### Issues Found

**Issue 1**: ________________________________________

**Issue 2**: ________________________________________

**Issue 3**: ________________________________________

### Recommended Changes

**README Changes Needed**: ________________________________________

**Appendix Additions**: ________________________________________

**Clarifications Needed**: ________________________________________

---

## Quick Reference - Test Commands

```powershell
# Prerequisites Check
dotnet --version
node --version
npm --version
git --version

# Quick Start
git clone <url>
cd flowingly-technical-test
dotnet run --project src/Api                 # Terminal 1
cd client; npm install; npm run dev           # Terminal 2 (PowerShell)

# Testing
dotnet build
dotnet test
dotnet test --filter Category=Unit
dotnet test --filter Category=Contract

# Frontend Build
cd client
npm run build
```

---

## Notes

- Test with **fresh Windows 11 installation** if possible (no dev tools pre-installed)
- Test as **non-admin user** (typical external reviewer scenario)
- Document any **Windows-specific errors** not covered in README
- Note any **confusing sections** that need clarification

---

**Testing Complete**: Date ___________ | Tester ___________
