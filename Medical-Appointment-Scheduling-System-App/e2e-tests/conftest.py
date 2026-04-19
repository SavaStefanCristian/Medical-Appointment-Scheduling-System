import pytest
import requests
import uuid

BASE_URL = "http://localhost:8080/api"


def unique_email():
    return f"test_{uuid.uuid4().hex[:8]}@test.com"


@pytest.fixture(scope="session")
def base_url():
    return BASE_URL


@pytest.fixture
def patient_credentials(base_url):
    email = unique_email()
    password = "TestPass123!"
    requests.post(f"{base_url}/Auth/register", json={
        "email": email,
        "password": password,
        "name": "Test Patient",
        "phone": "0700000000"
    })
    return {"email": email, "password": password}


@pytest.fixture
def patient_token(base_url, patient_credentials):
    response = requests.post(f"{base_url}/Auth/login", json=patient_credentials)
    data = response.json()
    return data["token"], data.get("patientId")


@pytest.fixture
def admin_token(base_url):
    # Admin user must exist in DB — seeded manually or via first run
    response = requests.post(f"{base_url}/Auth/login", json={
        "email": "admin@medical.com",
        "password": "Admin123!"
    })
    if response.status_code == 200:
        return response.json()["token"]
    return None


def auth_headers(token):
    return {"Authorization": f"Bearer {token}"}
