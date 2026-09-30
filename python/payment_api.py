from typing import Annotated

from fastapi import Depends, FastAPI

from foundation.auth import Caller, require_caller
from foundation.config import load_settings

app = FastAPI(title="Payment API", version="v1")
settings = load_settings("payment-api")


@app.get("/health/live")
def liveness() -> dict[str, str]:
    return {"status": "ok"}


@app.get("/v1/payments/foundation")
def foundation(caller: Annotated[Caller, Depends(require_caller)]) -> dict[str, str]:
    return {"service": settings.service_name, "tenantId": caller.tenant_id}
