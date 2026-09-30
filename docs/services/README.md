# Python Services Documentation

This directory contains comprehensive guides for Python services (PaymentApi, VendorApi) in the distributed integration platform.

## Quick Start

### For Developers New to This Project

Start here to understand the Python service structure:

1. **[Python Service Architecture Guide](./PYTHON_SERVICE_ARCHITECTURE.md)** - Overview of file structure, purposes, and design decisions
2. **[Setuptools & Packaging Guide](./SETUPTOOLS_AND_PACKAGING.md)** - How setuptools works and Python packaging

### For Service-Specific Documentation

- [PaymentApi README](../../src/PaymentApi/README.md) - Payment service documentation
- [VendorApi README](../../src/VendorApi/README.md) - Vendor service documentation

---

## Document Guide

### Python Service Architecture Guide

**What you'll learn:**
- File structure and organization
- Purpose of each file (main.py, auth.py, config.py, pyproject.toml, Dockerfile, etc.)
- What's manually created vs auto-generated
- Tools and libraries used
- Why foundation code is duplicated (microservices pattern)
- When to extract shared code
- Development workflow
- Common tasks

**Key Sections:**
- File Structure & Purposes - reference table
- How Files Are Created - manual vs auto-generated
- Tools & Libraries Used - all Python packages and dependencies
- Setuptools & pyproject.toml - package management
- Microservices Architecture - why duplication is correct
- Development Workflow - local dev, code changes, CI/CD

---

### Setuptools & Packaging Guide

**What you'll learn:**
- What setuptools does and why you need it
- Complete setup workflow (step-by-step)
- How pyproject.toml works
- What .egg-info/ contains
- Dependency specifications (version constraints, extras, optional dependencies)
- Common setuptools tasks (install, uninstall, update)
- Development workflow with setuptools
- Troubleshooting common problems
- Comparing setuptools approaches (setup.py vs setup.cfg vs pyproject.toml)

**Key Sections:**
- Complete Setup Workflow - 4 steps to get running
- What .egg-info/ Contains - auto-generated metadata
- Dependency Specifications - version constraints
- Common setuptools Tasks - install, uninstall, build, upload
- Troubleshooting - solutions for common errors
- Quick Reference - command cheat sheet

---

## File Purposes at a Glance

| File | Purpose | Created By |
|------|---------|-----------|
| `main.py` | FastAPI application + HTTP routes | Developer |
| `__init__.py` | Package marker | Developer |
| `foundation/auth.py` | JWT validation + caller extraction | Developer |
| `foundation/config.py` | Load settings from environment | Developer |
| `tests/test_foundation.py` | Unit tests | Developer |
| `pyproject.toml` | Dependencies & project metadata | Developer |
| `Dockerfile` | Container build recipe | Developer |
| `README.md` | Service documentation | Developer |
| `.egg-info/` | Package metadata | AUTO (setuptools) |
| `__pycache__/` | Python bytecode | AUTO (Python) |
| `.pytest_cache/` | Test cache | AUTO (pytest) |

**All source files are manually written by developers. Only metadata files are auto-generated.**

---

## Why Foundation Code Is Duplicated

```
src/PaymentApi/payment_api/foundation/auth.py
src/VendorApi/vendor_api/foundation/auth.py
```

These files have identical code, but that's intentional:

**Benefits:**
- ✓ Deploy PaymentApi independently of VendorApi
- ✓ Test PaymentApi alone (0.25s vs 5s with shared code)
- ✓ Fix bug in PaymentApi without risking VendorApi
- ✓ Scale PaymentApi independently (5 replicas) from VendorApi (2 replicas)
- ✓ Evolve each service independently

**Cost:**
- ~100 lines of code duplicated (~200 KB)

**When to Extract:**
- Phase 2+: If 3+ services need same code
- Method: Create `dip-foundation` Python package
- Benefit: Versioned, still independent

---

## Tools & Libraries

### Web Framework
- **fastapi>=0.115** - Modern async web framework

### Authentication
- **PyJWT[crypto]>=2.10** - JWT token validation

### Configuration
- **pydantic-settings>=2.6** - Environment variable loading

### Testing
- **pytest>=8** - Unit testing

### Code Quality
- **ruff>=0.8** - Linting and formatting

### Package Management
- **setuptools>=68** - Python packaging

### Server
- **uvicorn** - ASGI server for FastAPI

### Container
- **docker** - Containerization

**Note:** No SDKs or code generation tools. All standard, open-source packages.

---

## Development Quick Start

### First-Time Setup

```bash
# Install PaymentApi in editable mode
cd src/PaymentApi
pip install -e ".[dev]"

# Or VendorApi
cd src/VendorApi
pip install -e ".[dev]"
```

### Daily Workflow

```bash
# Edit code
nano payment_api/main.py

# Test
pytest tests/ -v

# Check quality
ruff check . --fix

# Run locally
python -m uvicorn payment_api.main:app --reload
```

### Before Committing

```bash
# All tests pass
pytest tests/ -v

# No linting issues
ruff check .

# Code imports correctly
python -c "from payment_api.main import app; print('✓')"

# Commit
git add src/PaymentApi/
git commit -m "Add new endpoint"
```

---

## Common Questions

### "Why is auth.py the same in both services?"

**Answer:** Microservices architecture principle. Each service owns its code and can be deployed independently. Small duplication (~100 lines) is better than tight coupling.

See: [Why Foundation Code Is Duplicated](./PYTHON_SERVICE_ARCHITECTURE.md#microservices-architecture-why-duplication-is-correct)

### "How do I install setuptools?"

**Answer:** You already have it. It comes with Python 3.x. Just run `pip install -e .` and setuptools handles the rest.

See: [Setuptools & Packaging Guide](./SETUPTOOLS_AND_PACKAGING.md)

### "What's the difference between `pip install .` and `pip install -e .`?"

**Answer:**
- `pip install .` - Normal install (copy code to site-packages, changes don't take effect)
- `pip install -e .` - Editable install (symlink to source, changes take immediate effect)

Use `-e` for development, without `-e` for production.

See: [Editable Install (Development)](./SETUPTOOLS_AND_PACKAGING.md#editable-install-development)

### "Why do we have __init__.py files?"

**Answer:** They mark directories as Python packages. Without them, Python doesn't recognize `from payment_api import ...`.

See: [File Structure & Purposes](./PYTHON_SERVICE_ARCHITECTURE.md#file-structure--purposes)

### "What's .egg-info/ for?"

**Answer:** Auto-generated by setuptools. Contains metadata (dependencies, version, file list). Used by pip for install/uninstall. Never commit it.

See: [What .egg-info/ Contains](./SETUPTOOLS_AND_PACKAGING.md#what-egginfo-contains)

### "When should we extract shared code?"

**Answer:** Phase 2+, when 3+ services need identical logic. Create `dip-foundation` Python package instead of duplicating.

See: [When To Extract Foundation to Shared Package](./PYTHON_SERVICE_ARCHITECTURE.md#when-to-extract-foundation-to-shared-package)

---

## Related Documentation

- [Implementation Plan](../../.plans/ohs-api-implementation-plan.md) - Phase 0-3 roadmap (see DOC-001)
- [API Contract Baseline](../pre-implementation/api-contract-baseline.md) - HTTP API contracts
- [Cross-Cutting Standards](../pre-implementation/cross-cutting-standards.md) - Authentication & authorization
- [Code Guidelines](../code-guidelines-best-practices-and-patterns.md) - Coding standards

---

## File Structure of This Documentation

```
docs/services/
├── README.md                          # This file (index)
├── PYTHON_SERVICE_ARCHITECTURE.md     # File structure, setup, design decisions
└── SETUPTOOLS_AND_PACKAGING.md        # Packaging, dependencies, troubleshooting
```

---

## Quick Reference: Common Commands

```bash
# Setup (first time)
cd src/PaymentApi
pip install -e ".[dev]"

# Run tests
pytest tests/ -v

# Run app locally
python -m uvicorn payment_api.main:app --reload

# Check code quality
ruff check . --fix

# Test endpoint
curl http://localhost:8000/health/live

# Uninstall
pip uninstall payment-api
```

---

## Key Takeaways

1. **No code generation** - All source files are manually written
2. **Standard Python packages** - fastapi, PyJWT, pytest, ruff, etc.
3. **Setuptools handles packaging** - Read pyproject.toml, install dependencies, create metadata
4. **Duplication is intentional** - Each service is independent (microservices pattern)
5. **Extract shared code in Phase 2+** - When 3+ services need same logic
6. **Development is simple** - `pip install -e .`, edit code, `pytest tests/`, done

---

## Need Help?

- **File structure questions** → See [Python Service Architecture Guide](./PYTHON_SERVICE_ARCHITECTURE.md)
- **Installation issues** → See [Setuptools & Packaging Guide](./SETUPTOOLS_AND_PACKAGING.md#troubleshooting)
- **Service-specific questions** → See [PaymentApi README](../../src/PaymentApi/README.md) or [VendorApi README](../../src/VendorApi/README.md)
- **Architecture questions** → See [API Contract Baseline](../pre-implementation/api-contract-baseline.md)
