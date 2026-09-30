from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class ServiceSettings(BaseSettings):
    model_config = SettingsConfigDict(env_nested_delimiter="__", extra="ignore")

    service_name: str = Field(default="service", min_length=1)
    auth_authority: str = "http://localhost:8080/realms/ohs"
    auth_audience: str = "ohs-api"
    auth_public_key: str | None = None
    auth_tenant_claim: str = "tenant_id"


def load_settings(service_name: str) -> ServiceSettings:
    return ServiceSettings(service_name=service_name)
