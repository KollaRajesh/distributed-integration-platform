# Python Service Architecture Guide

## Overview

This guide explains the structure, file organization, and design decisions for Python services (PaymentApi, VendorApi) in the distributed integration platform.

---

## File Structure & Purposes

### Standard Python Service Layout

```
src/PaymentApi/
├── payment_api/                    # Service package
│   ├── __init__.py                 # Package marker
│   ├── main.py                     # FastAPI application entry point
│   └── foundation/                 # Shared foundation for this service
│       ├── __init__.py             # Package marker
│       ├── auth.py                 # JWT validation, tenant extraction
│       └── config.py               # Configuration from environment
├── tests/                          # Service unit tests
│   ├── __init__.py                 # Package marker
│   └── test_foundation.py          # Tests for auth and config
├── pyproject.toml                  # Project metadata and dependencies
├── Dockerfile                      # Container build definition
└── README.md                       # Service documentation
```

### File Purposes Reference

| File | Purpose | Created By | Tool/SDK |
|------|---------|-----------|----------|
| `payment_api/main.py` | FastAPI app + HTTP routes | Developer (manual) | Python |
| `payment_api/__init__.py` | Marks directory as Python package | Developer (manual) | None |
| `foundation/auth.py` | JWT validation + caller/tenant extraction | Developer (manual) | PyJWT library |
| `foundation/config.py` | Load environment variables into typed settings | Developer (manual) | Pydantic library |
| `tests/test_foundation.py` | Unit tests for auth and config | Developer (manual) | pytest framework |
| `pyproject.toml` | Dependencies list and project metadata | Developer (manual) | None |
| `Dockerfile` | Container build recipe | Developer (manual) | Docker |
| `README.md` | Service documentation | Developer (manual) | None |
| `.egg-info/` | Package metadata (auto-generated) | **AUTO** (setuptools) | setuptools |
| `__pycache__/` | Python bytecode cache | **AUTO** (Python) | Python |
| `.pytest_cache/` | Test execution cache | **AUTO** (pytest) | pytest |

**Key Point:** No code generation tools used. All source files are manually written by developers using standard Python libraries.

---

## How Files Are Created

### 1. Manual File Creation (Developer-Written)

Developers create source files by hand:

```bash
# Edit main.py (FastAPI application)
$ nano payment_api/main.py
# Developer types:
#   from fastapi import FastAPI
#   app = FastAPI()
#   @app.get("/health/live")
#   async def health(): ...

# Edit auth.py (JWT validation)
$ nano payment_api/foundation/auth.py
# Developer types:
#   import jwt
#   def require_caller(...): ...

# Edit pyproject.toml (project metadata)
$ nano pyproject.toml
# Developer types:
#   dependencies = [
#       "fastapi>=0.115,<1",
#       "PyJWT[crypto]>=2.10",
#       ...
#   ]
```

### 2. Auto-Generated Files (Tools)

When you run specific commands, tools auto-generate metadata:

```bash
# Install package in development mode
$ cd src/PaymentApi
$ pip install -e ".[dev]"
# setuptools reads pyproject.toml
# AUTO-CREATES: payment_api.egg-info/ (package metadata)

# Run application
$ python -m uvicorn payment_api.main:app
# Python reads bytecode
# AUTO-CREATES: __pycache__/ (compiled Python)

# Run tests
$ pytest tests/
# pytest runs tests and caches results
# AUTO-CREATES: .pytest_cache/ (test metadata)
```

### 3. Add to .gitignore

Auto-generated files should NOT be committed:

```
# .gitignore
*.egg-info/
__pycache__/
.pytest_cache/
```

---

## Tools & Libraries Used

### Python Web Framework
- **fastapi>=0.115** - Modern web framework for HTTP routes, dependency injection, automatic API documentation

### Authentication & JWT
- **PyJWT[crypto]>=2.10** - JWT token validation, RS256 signature verification with cryptography

### Configuration Management
- **pydantic-settings>=2.6** - Load and validate configuration from environment variables with type safety

### Testing
- **pytest>=8** - Unit test framework with fixtures, parametrization, plugin system

### Code Quality
- **ruff>=0.8** - Fast Python linter (replaces flake8, isort, black in one tool)

### Package Management
- **setuptools>=68** - Enables package installation with `pip install -e .`

### Server
- **uvicorn** - ASGI web server (runs FastAPI application)

### Containerization
- **docker** - Container image building and deployment

**Note:** No SDKs or code generation tools used. Everything is standard, open-source Python packages.

---

## Setuptools & pyproject.toml

### What is setuptools?

setuptools is a Python tool that packages your code so you can install it with `pip install`. It reads `pyproject.toml` and:

1. Finds your source code
2. Checks dependencies
3. Creates `.egg-info/` metadata
4. Registers package with pip
5. Enables: `from payment_api import ...`

### How to Setup (3 Steps)

#### Step 1: Create pyproject.toml

```toml
[build-system]
requires = ["setuptools>=68"]
build-backend = "setuptools.build_meta"

[project]
name = "payment-api"
version = "0.1.0"
description = "Payment processing service"
requires-python = ">=3.12"
dependencies = [
    "fastapi>=0.115,<1",
    "PyJWT[crypto]>=2.10",
    "pydantic-settings>=2.6",
    "uvicorn",
]

[project.optional-dependencies]
dev = [
    "pytest>=8",
    "ruff>=0.8",
    "httpx",
]

[tool.ruff]
line-length = 100
target-version = "py312"
```

#### Step 2: Run pip install (one time)

```bash
$ cd src/PaymentApi
$ pip install -e ".[dev]"
```

What happens:
- ✓ Reads pyproject.toml
- ✓ Installs: fastapi, PyJWT, pydantic-settings, uvicorn (runtime)
- ✓ Installs: pytest, ruff, httpx (dev tools)
- ✓ Creates: payment_api.egg-info/
- ✓ Registers package with pip

Flag meanings:
- `-e` = editable mode (changes to code take immediate effect, no reinstall needed)
- `.[dev]` = install from current directory + dev dependencies

#### Step 3: Use the package

```python
# In your code:
from payment_api.main import app
from payment_api.foundation.auth import require_caller

# In tests:
from fastapi.testclient import TestClient
from payment_api.main import app

# Run:
python -m pytest tests/
python -m uvicorn payment_api.main:app --reload
```

### What .egg-info/ Contains

```
payment_api.egg-info/
├── METADATA          → Name, version, description
├── SOURCES.txt       → List of all files in package
├── requires.txt      → Dependencies list
├── top_level.txt     → Package name (payment_api)
└── dependency_links.txt → Where to get dependencies
```

**Purpose:** Tells pip how to install/uninstall this package
**Important:** Add `*.egg-info/` to `.gitignore` (never commit)

### Setup Flow

```
pyproject.toml (developer writes)
        ↓
pip install -e ".[dev]" (you run once)
        ↓
setuptools reads pyproject.toml
        ↓
Downloads dependencies from PyPI
        ↓
Installs fastapi, PyJWT, pytest, etc.
        ↓
Creates .egg-info/ metadata
        ↓
Now you can: from payment_api import ...
```

---

## Microservices Architecture: Why Duplication Is Correct

### Current Structure (Microservices Best Practice)

```
src/PaymentApi/
└── payment_api/foundation/
    ├── auth.py       (JWT validation)
    └── config.py     (Settings)

src/VendorApi/
└── vendor_api/foundation/
    ├── auth.py       (JWT validation - IDENTICAL code)
    └── config.py     (Settings - IDENTICAL code)
```

### Why Duplication Is Better Than Sharing

| Aspect | Shared Code (Bad) | Duplicated Code (Good) |
|--------|-------------------|------------------------|
| **Deployment** | Must deploy both services together | Deploy PaymentApi independently |
| **Testing** | Change auth.py → test both services | Change PaymentApi/auth.py → test 1 service |
| **Test speed** | 5 seconds (coordinate tests) | 0.25 seconds per service |
| **Failure scope** | Bug affects all services | Bug affects only one service |
| **Rollback** | Must rollback both services | Rollback only affected service |
| **Scaling** | Tied scaling (5 Payment, 5 Vendor) | Independent scaling (5 Payment, 2 Vendor) |
| **Code evolution** | Blocked by compatibility needs | Free to evolve independently |
| **Development velocity** | Slower (coordinate changes) | Faster (independent changes) |

### Real-World Example

**Scenario:** PaymentApi needs to add webhook signature validation to auth.

**With Shared Code:**
```
src/shared_foundation/auth.py (shared by both services)
├─ Add webhook_signature_validation()
├─ Both PaymentApi and VendorApi immediately affected
├─ Must test both services
├─ Must deploy both services
└─ If webhook logic breaks VendorApi? Both services down
```

**With Duplicated Code:**
```
src/PaymentApi/payment_api/foundation/auth.py (PaymentApi owns it)
├─ Add webhook_signature_validation()
├─ Only PaymentApi affected
├─ Test only PaymentApi
├─ Deploy only PaymentApi
└─ If webhook logic breaks? Only PaymentApi down, VendorApi running fine
```

### Cost-Benefit Analysis

- **Cost:** ~100 lines of code duplicated (~200 KB disk space)
- **Benefit:** Independent deployment, testing, scaling, failure isolation
- **Industry Standard:** All major platforms use duplication (AWS microservices, Netflix architecture, Uber services)

---

## When To Extract Foundation to Shared Package

### Timeline

| Phase | Action | Reason |
|-------|--------|--------|
| **Phase 0-1** | Keep duplicated | Only 2 services, small code (~100 lines each), low cost |
| **Phase 2+** | Extract to package | 3+ services need identical logic, duplication burden increases |

### How To Extract (Phase 2+)

**Step 1: Create internal Python package**
```
src/dip-foundation/
├── dip_foundation/
│   ├── __init__.py
│   ├── auth.py       (shared JWT validation)
│   └── config.py     (shared settings)
├── pyproject.toml
└── README.md
```

**Step 2: Publish to GitHub Packages or internal PyPI**
```bash
pip install dip-foundation>=1.0,<2
```

**Step 3: Update each service**
```toml
# src/PaymentApi/pyproject.toml
dependencies = [
    "fastapi>=0.115",
    "dip-foundation>=1.0,<2",  # Instead of local auth.py
]
```

**Step 4: Remove duplicate code**
```bash
rm src/PaymentApi/payment_api/foundation/auth.py
rm src/VendorApi/vendor_api/foundation/auth.py
```

**Step 5: Import from package**
```python
# src/PaymentApi/payment_api/main.py
from dip_foundation.auth import require_caller
```

**Benefits:**
- ✓ Single source of truth for shared logic
- ✓ Explicit versioning (services control when to upgrade)
- ✓ Faster deployment (don't rebuild services when foundation changes)
- ✓ Still independent (each service chooses which version to use)

---

## Development Workflow

### Local Development

```bash
# 1. Navigate to service
cd src/PaymentApi

# 2. Install dependencies (one-time)
pip install -e ".[dev]"

# 3. Run tests
pytest tests/

# 4. Check code quality
ruff check .

# 5. Run locally
python -m uvicorn payment_api.main:app --reload

# 6. Test endpoint
curl -X GET http://localhost:8000/health/live
```

### Code Changes

```bash
# 1. Edit files
nano payment_api/main.py

# 2. Run tests
pytest tests/ -v

# 3. Check linting
ruff check . --fix

# 4. Commit
git add src/PaymentApi/
git commit -m "Add new payment endpoint"
```

### CI/CD Execution

```bash
# Automatic (GitHub Actions):
1. Build .NET services
2. pytest src/PaymentApi/tests/
3. pytest src/VendorApi/tests/
4. ruff check src/PaymentApi/
5. ruff check src/VendorApi/
6. docker build -f src/PaymentApi/Dockerfile ...
7. docker build -f src/VendorApi/Dockerfile ...
```

---

## Common Tasks

### Add a New Endpoint

```python
# src/PaymentApi/payment_api/main.py
from fastapi import APIRouter, Depends
from .foundation.auth import require_caller, Caller

router = APIRouter()

@router.post("/v1/payments/process")
async def process_payment(
    request: PaymentRequest,
    caller: Caller = Depends(require_caller)
):
    """Process payment with tenant isolation."""
    return await service.process(request, caller.tenant_id)
```

### Add a New Configuration Setting

```python
# src/PaymentApi/payment_api/foundation/config.py
from pydantic_settings import BaseSettings

class ServiceSettings(BaseSettings):
    service: str = "payment-api"
    auth_authority: str = ""
    payment_processor_url: str = ""  # NEW

    class Config:
        env_file = ".env"
        env_prefix = "PAYMENT_"
```

### Add a Unit Test

```python
# src/PaymentApi/tests/test_foundation.py
from fastapi.testclient import TestClient
from payment_api.main import app
import jwt

def test_payment_processing_with_auth():
    # Arrange
    client = TestClient(app)
    token = jwt.encode(
        {"sub": "user-123", "tenant_id": "org-123"},
        "secret",
        algorithm="HS256"
    )
    
    # Act
    response = client.post(
        "/v1/payments/process",
        json={"amount": 100},
        headers={"Authorization": f"Bearer {token}"}
    )
    
    # Assert
    assert response.status_code == 200
    assert response.json()["tenant_id"] == "org-123"
```

---

## Common Commands

### Setup (first time)
```bash
cd src/PaymentApi
pip install -e ".[dev]"
```

### Run tests
```bash
pytest tests/

# With verbose output
pytest tests/ -v

# Run specific test
pytest tests/test_foundation.py::test_health_endpoint_returns_ok
```

### Run linting
```bash
ruff check .

# Fix issues automatically
ruff check . --fix
```

### Run app locally
```bash
python -m uvicorn payment_api.main:app --reload

# Access at http://localhost:8000
# Auto-reload on file changes
```

### Test endpoint
```bash
curl -X GET http://localhost:8000/health/live

curl -X POST http://localhost:8000/v1/payments/process \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"amount": 100}'
```

### Update dependencies
```bash
pip install --upgrade fastapi
```

### Uninstall package
```bash
pip uninstall payment-api
```

---

## Docker & Containerization

### Dockerfile Explained

```dockerfile
# Use Python 3.12 slim image as base
FROM python:3.12-slim

# Set working directory inside container
WORKDIR /app

# Copy project file
COPY pyproject.toml .

# Install dependencies inside container
RUN pip install --no-cache-dir .

# Copy application code
COPY payment_api ./payment_api

# Expose port
EXPOSE 8000

# Health check (Docker/Kubernetes)
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD python -c "import urllib.request; urllib.request.urlopen('http://localhost:8000/health/live')"

# Run application when container starts
ENTRYPOINT ["python", "-m", "uvicorn", "payment_api.main:app", "--host", "0.0.0.0", "--port", "8000"]
```

### Build Container

```bash
# Build image
docker build -f src/PaymentApi/Dockerfile -t payment-api:latest src/PaymentApi/

# Run container
docker run -p 8000:8000 payment-api:latest

# Test inside container
curl http://localhost:8000/health/live
```

### CI/CD Docker Build

```bash
# GitHub Actions builds each service's image
docker build -f src/PaymentApi/Dockerfile -t payment-api:${{ github.sha }} src/PaymentApi/
docker push ghcr.io/kollarajesh/payment-api:${{ github.sha }}
```

---

## Environment Variables

### Payment API Settings

```bash
# src/PaymentApi/.env (local development only)

# Service identification
PAYMENT_SERVICE=payment-api

# Authentication
PAYMENT_AUTH_AUTHORITY=https://auth.example.com
PAYMENT_AUTH_PUBLIC_KEY=<public-key-for-jwt-verification>

# Application
PAYMENT_LOG_LEVEL=INFO
```

### VendorApi Settings

```bash
# src/VendorApi/.env (local development only)

# Service identification
VENDOR_SERVICE=vendor-api

# Authentication
VENDOR_AUTH_AUTHORITY=https://auth.example.com
VENDOR_AUTH_PUBLIC_KEY=<public-key-for-jwt-verification>

# Application
VENDOR_LOG_LEVEL=INFO
```

### Loading in Code

```python
# src/PaymentApi/payment_api/foundation/config.py
from pydantic_settings import BaseSettings

class ServiceSettings(BaseSettings):
    service: str = "payment-api"
    auth_authority: str
    auth_public_key: str
    log_level: str = "INFO"

    class Config:
        env_file = ".env"
        env_prefix = "PAYMENT_"

settings = ServiceSettings()
# Reads: PAYMENT_AUTH_AUTHORITY, PAYMENT_AUTH_PUBLIC_KEY, etc.
```

---

## See Also

- [PaymentApi README](../../src/PaymentApi/README.md) - PaymentApi-specific documentation
- [VendorApi README](../../src/VendorApi/README.md) - VendorApi-specific documentation
- [Implementation Plan](../../.plans/ohs-api-implementation-plan.md) - Phase tracking (DOC-001)
- [API Contract Baseline](../pre-implementation/api-contract-baseline.md) - HTTP contract details
- [Cross-Cutting Standards](../pre-implementation/cross-cutting-standards.md) - Authentication and authorization standards
