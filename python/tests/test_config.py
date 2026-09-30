from foundation.config import load_settings


def test_load_settings_uses_service_name() -> None:
    settings = load_settings("payment-api")

    assert settings.service_name == "payment-api"
    assert settings.auth_audience == "ohs-api"
