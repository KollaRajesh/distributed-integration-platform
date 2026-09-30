from pathlib import Path
import sys


ROOT = Path(__file__).resolve().parents[1]
required = (
    ROOT / "DistributedIntegrationPlatform.slnx",
    ROOT / ".github" / "workflows" / "ci.yml",
    ROOT / ".github" / "workflows" / "migration-validation.yml",
    ROOT / "python" / "pyproject.toml",
)

missing = [str(path.relative_to(ROOT)) for path in required if not path.is_file()]
if missing:
    print("CI foundation is missing: " + ", ".join(missing), file=sys.stderr)
    raise SystemExit(1)

print("CI foundation structure is valid")
