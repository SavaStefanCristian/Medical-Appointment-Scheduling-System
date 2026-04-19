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


class TestDoctors:

    def test_get_doctors_is_public_no_auth_needed(self):
        # PASS: 200 returned without token — doctors list is intentionally public
        # FAIL: Patients cannot browse doctors before registering — UX broken
        response = requests.get(f"{BASE_URL}/Doctors")
        assert response.status_code == 200

    def test_get_doctors_with_token_returns_200(self):
        # PASS: 200 returned with valid token
        # FAIL: Authenticated users cannot see doctor list — booking impossible
        token = register_and_login()
        response = requests.get(f"{BASE_URL}/Doctors",
                                headers=auth_headers(token))
        assert response.status_code == 200

    def test_get_doctors_returns_list(self):
        # PASS: Response is a JSON array
        # FAIL: Response shape is wrong — frontend crashes parsing doctors
        token = register_and_login()
        response = requests.get(f"{BASE_URL}/Doctors",
                                headers=auth_headers(token))
        assert isinstance(response.json(), list)

    def test_get_doctors_response_has_expected_fields(self):
        # PASS: Each doctor has id, name, specialty fields
        # FAIL: Frontend crashes accessing missing fields
        token = register_and_login()
        response = requests.get(f"{BASE_URL}/Doctors",
                                headers=auth_headers(token))
        doctors = response.json()
        if len(doctors) > 0:
            doctor = doctors[0]
            assert "id" in doctor
            assert "name" in doctor
            assert "specialty" in doctor