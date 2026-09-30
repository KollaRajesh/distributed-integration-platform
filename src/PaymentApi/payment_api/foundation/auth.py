from dataclasses import dataclass
from typing import Annotated

import jwt
from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from jwt import InvalidTokenError

from payment_api.foundation.config import ServiceSettings

bearer_scheme = HTTPBearer(auto_error=False)

@dataclass(frozen=True)
class Caller:
    subject: str
    tenant_id: str
    roles: frozenset[str]

async def get_settings() -> ServiceSettings:
    return ServiceSettings()

def require_caller(
    credentials: Annotated[HTTPAuthorizationCredentials | None, Depends(bearer_scheme)],
    settings: Annotated[ServiceSettings, Depends(get_settings)],
) -> Caller:
    if credentials is None:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED, detail="Authentication required"
        )

    if not settings.auth_public_key:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="JWT verification keys are not configured",
        )

    try:
        claims = jwt.decode(
            credentials.credentials,
            settings.auth_public_key,
            algorithms=["RS256"],
            issuer=settings.auth_authority,
            audience=settings.auth_audience,
        )
    except InvalidTokenError as error:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid access token",
        ) from error

    subject = claims.get("sub")
    tenant_id = claims.get(settings.auth_tenant_claim)
    if not isinstance(subject, str) or not isinstance(tenant_id, str):
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN, detail="Required claims are missing"
        )

    roles = claims.get("roles", [])
    return Caller(subject=subject, tenant_id=tenant_id, roles=frozenset(roles))
