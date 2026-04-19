import requests
import uuid

BASE_URL = "http://localhost:8080/api"


def unique_email():
    return f"test_{uuid.uuid4().hex[:8]}@test.com"


def register_and_login():
    email = unique_email()
    requests.post(f"{BASE_URL}/Auth/register", json={
        "email": email,
        "password": "TestPass123!",
        "name": "Test Patient",
        "phone": "0700000000"
    })
    response = requests.post(f"{BASE_URL}/Auth/login", json={
        "email": email,
        "password": "TestPass123!"
    })
    return response.json()["token"]


def auth_headers(token):
    return {"Authorization": f"Bearer {token}"}


class TestPatients:

    def test_get_patients_is_public_no_auth_needed(self):
        # PASS: 200 returned without token — endpoint is intentionally public
        # FAIL: Endpoint broken — returns error without token
        # NOTE: For a production app this should require [Authorize],
        #       but for this academic project it is intentionally public
        response = requests.get(f"{BASE_URL}/Patients")
        assert response.status_code == 200

    def test_get_patients_with_token_returns_200(self):
        # PASS: 200 returned with valid token
        # FAIL: Authenticated users cannot see patient list
        token = register_and_login()
        response = requests.get(f"{BASE_URL}/Patients",
                                headers=auth_headers(token))
        assert response.status_code == 200

    def test_get_patients_returns_list(self):
        # PASS: Response is a JSON array
        # FAIL: Response shape wrong — frontend crashes
        token = register_and_login()
        response = requests.get(f"{BASE_URL}/Patients",
                                headers=auth_headers(token))
        assert isinstance(response.json(), list)

    def test_get_patients_list_not_empty_after_register(self):
        # PASS: At least 1 patient exists after registration
        # FAIL: Register does not create patient profile
        token = register_and_login()
        response = requests.get(f"{BASE_URL}/Patients",
                                headers=auth_headers(token))
        assert len(response.json()) >= 1