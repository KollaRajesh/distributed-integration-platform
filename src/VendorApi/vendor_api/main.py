from typing import Annotated

from fastapi import Depends, FastAPI

from vendor_api.foundation.auth import Caller, require_caller
from vendor_api.foundation.config import load_settings

app = FastAPI(title="Vendor API", version="v1")
settings = load_settings("vendor-api")

@app.get("/health/live")
def liveness() -> dict[str, str]:
    return {"status": "ok"}

@app.get("/v1/vendors/foundation")
def foundation(caller: Annotated[Caller, Depends(require_caller)]) -> dict[str, str]:
    return {"service": settings.service_name, "tenantId": caller.tenant_id}
