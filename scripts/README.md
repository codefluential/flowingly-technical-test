# Development Scripts

Essential build, test, and development automation scripts.

## Available Scripts

### Build & Test
- **`build.sh`** — Build the .NET solution and frontend
- **`test.sh`** — Run all tests (backend unit/integration/contract + E2E)
- **`clean.sh`** — Clean build artifacts

### Development Servers
- **`dev.sh`** — Start both backend API and frontend dev server (Linux/Mac)
- **`dev.ps1`** — Start both backend API and frontend dev server (Windows)

### Utilities
- **`run-script.js`** — Cross-platform script runner (detects OS and runs appropriate .sh/.ps1)

## Usage

```bash
# Build entire project
./scripts/build.sh

# Run all tests
./scripts/test.sh

# Start development servers
./scripts/dev.sh  # Linux/Mac
# OR
powershell ./scripts/dev.ps1  # Windows

# Clean build artifacts
./scripts/clean.sh

# Cross-platform (uses run-script.js)
npm run dev    # Runs dev.sh on Linux/Mac, dev.ps1 on Windows
npm run build  # Runs build.sh
npm run test   # Runs test.sh
```

## Prerequisites

- .NET 8 SDK (8.0.414+)
- Node.js 18+
- Playwright browsers installed (`npx playwright install`)

## Script Details

### dev.sh / dev.ps1
Starts both backend API (port 5000) and frontend dev server (port 5173) concurrently.

**Backend**: http://localhost:5000 (Swagger at /swagger)
**Frontend**: http://localhost:5173

### build.sh
Builds the entire solution:
1. Backend (.NET solution)
2. Frontend (client/ directory with Vite)

### test.sh
Runs comprehensive test suite:
1. Backend unit tests (xUnit)
2. Backend contract tests (API integration)
3. Frontend E2E tests (Playwright)

**Total**: 195 tests (118 backend + 77 E2E)

### clean.sh
Cleans build artifacts:
- .NET bin/ and obj/ directories
- Frontend dist/ and node_modules/.cache/

## Notes

- Backend API runs on http://localhost:5000
- Frontend runs on http://localhost:5173
- Swagger documentation at http://localhost:5000/swagger
- All scripts are cross-platform compatible via `run-script.js`
