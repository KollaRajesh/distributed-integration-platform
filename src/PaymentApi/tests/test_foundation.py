"""Payment API foundation tests."""

from fastapi.testclient import TestClient

from payment_api.main import app

client = TestClient(app)

def test_health_endpoint_returns_ok() -> None:
    response = client.get("/health/live")
    assert response.status_code == 200
    assert response.json() == {"status": "ok"}

def test_foundation_endpoint_requires_auth() -> None:
    response = client.get("/v1/payments/foundation")
    assert response.status_code == 401
