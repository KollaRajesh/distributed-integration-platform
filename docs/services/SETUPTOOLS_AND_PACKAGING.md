# Setuptools & Python Packaging Guide

## What is Setuptools?

setuptools is a Python tool that packages your code so you can install it with `pip install`. It's the standard way to distribute Python applications.

### Core Job

setuptools reads `pyproject.toml` and:

1. **Finds your source code** - Scans directories for Python packages
2. **Checks dependencies** - Reads list of required packages
3. **Downloads packages** - Gets dependencies from PyPI (Python Package Index)
4. **Installs everything** - Puts code + dependencies where Python can find them
5. **Creates metadata** - Generates `.egg-info/` for pip (install/uninstall tracking)
6. **Registers with pip** - Makes `from payment_api import ...` work

---

## Complete Setup Workflow

### Step 1: Create pyproject.toml

```toml
[build-system]
requires = ["setuptools>=68"]
build-backend = "setuptools.build_meta"
# ↑ These lines tell pip to use setuptools for building

[project]
name = "payment-api"
version = "0.1.0"
description = "Payment processing service for distributed platform"
readme = "README.md"
requires-python = ">=3.12"

# These dependencies are installed when you do: pip install .
dependencies = [
    "fastapi>=0.115,<1",
    "PyJWT[crypto]>=2.10",
    "pydantic-settings>=2.6",
    "uvicorn",
]

# These are only installed for development: pip install ".[dev]"
[project.optional-dependencies]
dev = [
    "pytest>=8",
    "ruff>=0.8",
    "httpx",
]

[tool.ruff]
line-length = 100
target-version = "py312"

[tool.pytest.ini_options]
testpaths = ["tests"]
python_files = "test_*.py"
```

### Step 2: Organize Code

```
src/PaymentApi/
├── payment_api/              # ← This is your package
│   ├── __init__.py          # Makes it a package
│   ├── main.py              # Your code
│   └── foundation/          # Sub-package
│       ├── __init__.py
│       ├── auth.py
│       └── config.py
├── tests/                   # ← Tests directory
│   ├── __init__.py
│   └── test_foundation.py
├── pyproject.toml           # ← setuptools reads this
├── Dockerfile
└── README.md
```

**Important:** setuptools looks for packages (directories with `__init__.py`). Without `__init__.py`, directories are not packages.

### Step 3: Run pip install

```bash
cd src/PaymentApi
pip install -e ".[dev]"
```

#### What this command does:

| Part | Meaning |
|------|---------|
| `pip install` | Package installer |
| `-e` | Editable mode (code changes = immediate effect) |
| `"."` | Install from current directory |
| `"[dev]"` | Include optional dev dependencies |

#### Installation steps:

```
1. setuptools reads pyproject.toml
2. Sees: requires-python = ">=3.12"
   → Checks you have Python 3.12+
3. Sees: dependencies = ["fastapi>=0.115,<1", ...]
   → Downloads from PyPI:
      ✓ fastapi 0.115.0
      ✓ PyJWT 2.10.1
      ✓ pydantic-settings 2.6.0
      ✓ uvicorn
      ✓ All their dependencies
4. Sees: [project.optional-dependencies] dev = ["pytest>=8", ...]
   → Downloads:
      ✓ pytest 8.0.0
      ✓ ruff 0.8.0
      ✓ httpx
5. Installs everything to Python site-packages
6. Creates payment_api.egg-info/ directory with metadata
7. Registers "payment-api" with pip
   → Now: from payment_api import app works
```

### Step 4: Verify Installation

```bash
# Check if package is installed
pip show payment-api

# Output:
# Name: payment-api
# Version: 0.1.0
# Summary: Payment processing service...
# Location: C:\Users\...\lib\site-packages
# Requires: fastapi, PyJWT, pydantic-settings, uvicorn
# Required-by:

# List all installed packages
pip list | grep payment

# Try to import
python -c "from payment_api.main import app; print('✓ Installed correctly')"
```

---

## What .egg-info/ Contains

When you run `pip install -e .`, setuptools creates a directory like `payment_api.egg-info/`:

```
payment_api.egg-info/
├── METADATA                 # Package name, version, author, etc.
├── SOURCES.txt             # List of all files in package
├── requires.txt            # Runtime dependencies
│                           # fastapi>=0.115,<1
│                           # PyJWT[crypto]>=2.10
│                           # pydantic-settings>=2.6
│                           # uvicorn
├── top_level.txt           # Package name (payment_api)
└── dependency_links.txt    # Where to find dependencies (usually empty)
```

### Example SOURCES.txt

```
payment_api/__init__.py
payment_api/main.py
payment_api/foundation/__init__.py
payment_api/foundation/auth.py
payment_api/foundation/config.py
tests/__init__.py
tests/test_foundation.py
pyproject.toml
README.md
Dockerfile
```

### Purpose of .egg-info/

- **pip uses it to uninstall** - Knows which files to delete
- **pip uses it to upgrade** - Knows what version is installed
- **pip uses it for dependencies** - Knows what to install with this package
- **Developers ignore it** - It's generated automatically

### .gitignore Setup

Never commit .egg-info/:

```
# .gitignore
*.egg-info/
__pycache__/
.pytest_cache/
```

---

## Dependency Specifications

### Version Constraints

```toml
dependencies = [
    "fastapi",                 # Any version
    "fastapi>=0.115",         # 0.115 or higher
    "fastapi<1",              # Below 1.0
    "fastapi>=0.115,<1",      # 0.115 to 1.0 (recommended)
    "PyJWT>=2.10,<3",         # Major version lock
    "pydantic-settings==2.6",  # Exact version (not recommended)
]
```

**Best Practice:** Use `"package>=X.Y,<Z"` to allow patch updates but prevent breaking changes.

### Extras (Optional Dependencies)

```toml
[project.optional-dependencies]
dev = ["pytest", "ruff", "httpx"]
test = ["pytest", "pytest-cov"]
docs = ["sphinx", "sphinx-rtd-theme"]

# Usage:
# pip install ".[dev]"       ← dev + runtime deps
# pip install ".[test]"      ← test + runtime deps
# pip install ".[dev,test]"  ← dev + test + runtime deps
# pip install ".[all]"       ← everything (only if using all feature)
```

### Dependency with Extra Features

```toml
dependencies = [
    "PyJWT[crypto]>=2.10",  # Install PyJWT + cryptography for RS256 signing
    "requests[security]",   # Install requests + security extras
]
```

---

## Common setuptools Tasks

### Editable Install (Development)

```bash
# Install in editable mode (changes to code = immediate effect)
pip install -e ".[dev]"

# Now you can edit payment_api/main.py and changes are live
# No need to reinstall
```

### Normal Install (Production)

```bash
# Install from PyPI (for production)
pip install payment-api>=0.1.0,<1

# Or install from local directory (without editable)
pip install ./src/PaymentApi
```

### Build Distribution Package

```bash
# Create wheel (binary package)
pip install build
cd src/PaymentApi
python -m build

# Output:
# dist/payment_api-0.1.0-py3-none-any.whl
# dist/payment_api-0.1.0.tar.gz
```

### Upload to PyPI

```bash
# Only for public packages
pip install twine
twine upload dist/payment_api-0.1.0-py3-none-any.whl
```

### Uninstall Package

```bash
pip uninstall payment-api

# Uses .egg-info/ to know what to delete
```

### Update Dependencies

```bash
# Update one package
pip install --upgrade fastapi

# Update all packages
pip install --upgrade -r requirements.txt

# Generate requirements.txt from installed packages
pip freeze > requirements.txt
```

---

## Development Workflow with setuptools

### Initial Setup

```bash
# 1. Clone repository
git clone https://github.com/kollarajesh/distributed-integration-platform
cd distributed-integration-platform

# 2. Install PaymentApi in editable mode
cd src/PaymentApi
pip install -e ".[dev]"

# 3. Verify installation
pytest tests/
```

### Daily Development

```bash
# Edit code
nano payment_api/main.py

# Test immediately (no reinstall needed)
pytest tests/test_foundation.py -v

# Check code quality
ruff check . --fix

# Run locally
python -m uvicorn payment_api.main:app --reload
```

### Before Committing

```bash
# Run all tests
pytest tests/ -v

# Check linting
ruff check .

# Check imports are correct
python -c "from payment_api.main import app; print('✓')"

# Commit
git add src/PaymentApi/
git commit -m "Add new endpoint"
```

### CI/CD Execution

```bash
# GitHub Actions runs in order:
1. Setup Python 3.12
2. pip install -e "./src/PaymentApi[dev]"
3. pytest src/PaymentApi/tests/ -v
4. ruff check src/PaymentApi/
5. docker build -f src/PaymentApi/Dockerfile src/PaymentApi/
```

---

## Troubleshooting

### Problem: "ModuleNotFoundError: No module named 'payment_api'"

**Solution 1: Install in editable mode**
```bash
cd src/PaymentApi
pip install -e .
```

**Solution 2: Check __init__.py exists**
```bash
# Must exist:
ls payment_api/__init__.py
ls payment_api/foundation/__init__.py
ls tests/__init__.py
```

**Solution 3: Check current working directory**
```bash
# Must be inside service directory
cd src/PaymentApi

# Then:
python -m pytest tests/
# NOT: python -m pytest src/PaymentApi/tests/
```

### Problem: "Dependency X not found"

**Solution: Install dev dependencies**
```bash
pip install -e ".[dev]"
# Note the [dev] flag to get optional dependencies
```

### Problem: ".egg-info/ shows old dependencies"

**Solution: Reinstall package**
```bash
pip uninstall payment-api
pip install -e ".[dev]"
```

### Problem: Changes to code don't take effect

**Solution 1: Editable mode is required**
```bash
pip install -e .
# NOT: pip install .
```

**Solution 2: Restart Python process**
```bash
# If using long-running server, restart:
python -m uvicorn payment_api.main:app --reload
```

---

## Comparing Setuptools Approaches

### Approach 1: Traditional setup.py (Deprecated)

```python
# setup.py (OLD - don't use this)
from setuptools import setup

setup(
    name="payment-api",
    version="0.1.0",
    py_modules=["payment_api"],
    install_requires=["fastapi", "PyJWT"],
)
```

**Problems:**
- ✗ setup.py is Python code (security risk)
- ✗ Can execute arbitrary code during install
- ✗ Hard to read dependencies

### Approach 2: setup.cfg (Better, still used)

```ini
# setup.cfg (OK, but less popular now)
[metadata]
name = payment-api
version = 0.1.0

[options]
packages = find:
install_requires =
    fastapi
    PyJWT
```

### Approach 3: pyproject.toml (Modern Standard)

```toml
# pyproject.toml (BEST - use this)
[project]
name = "payment-api"
version = "0.1.0"
dependencies = ["fastapi", "PyJWT"]
```

**Benefits:**
- ✓ Declarative (not executable code)
- ✓ Easy to read
- ✓ Industry standard (Python 3.10+)
- ✓ Supports build backends (setuptools, poetry, hatch)

---

## Advanced: Custom Entry Points

For executables/CLIs:

```toml
[project.scripts]
payment-worker = "payment_api.worker:main"
```

Then:

```bash
pip install -e .
payment-worker  # Runs payment_api.worker.main()
```

---

## Quick Reference

| Task | Command |
|------|---------|
| **First-time setup** | `cd src/PaymentApi && pip install -e ".[dev]"` |
| **Run tests** | `pytest tests/` |
| **Run app** | `python -m uvicorn payment_api.main:app --reload` |
| **Check linting** | `ruff check .` |
| **Uninstall** | `pip uninstall payment-api` |
| **Check installation** | `pip show payment-api` |
| **List dependencies** | `pip show payment-api \| grep Requires` |
| **Upgrade dependency** | `pip install --upgrade fastapi` |

---

## See Also

- [Python Service Architecture Guide](./PYTHON_SERVICE_ARCHITECTURE.md)
- [PaymentApi README](../../src/PaymentApi/README.md)
- [VendorApi README](../../src/VendorApi/README.md)
- [Official setuptools docs](https://setuptools.pypa.io/)
- [pyproject.toml spec](https://packaging.python.org/en/latest/specifications/pyproject-toml/)
