import requests
import uuid
import pytest

BASE_URL = "http://localhost:8080/api"


def unique_email():
    return f"test_{uuid.uuid4().hex[:8]}@test.com"


class TestRegister:

    def test_register_valid_data_returns_201(self):
        # PASS: New patient registered, 201 returned with token
        # FAIL: Registration broken — new users cannot create accounts
        response = requests.post(f"{BASE_URL}/Auth/register", json={
            "email": unique_email(),
            "password": "TestPass123!",
            "name": "Test Patient",
            "phone": "0700000001"
        })
        assert response.status_code == 201

    def test_register_response_contains_token(self):
        # PASS: Token present in response after registration
        # FAIL: Auto-login after register broken — user must login separately
        response = requests.post(f"{BASE_URL}/Auth/register", json={
            "email": unique_email(),
            "password": "TestPass123!",
            "name": "Test Patient",
            "phone": "0700000002"
        })
        assert "token" in response.json()

    def test_register_duplicate_email_returns_400(self):
        # PASS: 400 returned for duplicate email
        # FAIL: Two accounts created with same email — data integrity broken
        email = unique_email()
        requests.post(f"{BASE_URL}/Auth/register", json={
            "email": email,
            "password": "TestPass123!",
            "name": "First User",
            "phone": "0700000003"
        })
        response = requests.post(f"{BASE_URL}/Auth/register", json={
            "email": email,
            "password": "TestPass123!",
            "name": "Second User",
            "phone": "0700000004"
        })
        assert response.status_code == 400

    def test_register_missing_email_returns_400(self):
        # PASS: 400 returned when email is missing
        # FAIL: Server crashes or creates user without email
        response = requests.post(f"{BASE_URL}/Auth/register", json={
            "password": "TestPass123!",
            "name": "No Email",
            "phone": "0700000005"
        })
        assert response.status_code == 400


class TestLogin:

    def test_login_valid_credentials_returns_200(self):
        # PASS: 200 returned with token for valid credentials
        # FAIL: Login broken — valid users cannot authenticate
        email = unique_email()
        requests.post(f"{BASE_URL}/Auth/register", json={
            "email": email,
            "password": "TestPass123!",
            "name": "Login Test",
            "phone": "0700000006"
        })
        response = requests.post(f"{BASE_URL}/Auth/login", json={
            "email": email,
            "password": "TestPass123!"
        })
        assert response.status_code == 200

    def test_login_response_contains_token(self):
        # PASS: Token present in login response
        # FAIL: Frontend cannot authenticate — token missing
        email = unique_email()
        requests.post(f"{BASE_URL}/Auth/register", json={
            "email": email,
            "password": "TestPass123!",
            "name": "Token Test",
            "phone": "0700000007"
        })
        response = requests.post(f"{BASE_URL}/Auth/login", json={
            "email": email,
            "password": "TestPass123!"
        })
        assert "token" in response.json()

    def test_login_response_contains_patient_id(self):
        # PASS: patientId in response so frontend knows who is logged in
        # FAIL: Frontend cannot book appointments — patientId missing
        email = unique_email()
        requests.post(f"{BASE_URL}/Auth/register", json={
            "email": email,
            "password": "TestPass123!",
            "name": "PatientId Test",
            "phone": "0700000008"
        })
        response = requests.post(f"{BASE_URL}/Auth/login", json={
            "email": email,
            "password": "TestPass123!"
        })
        data = response.json()
        assert "patientId" in data
        assert data["patientId"] is not None

    def test_login_wrong_password_returns_401(self):
        # PASS: 401 returned for wrong password
        # FAIL: Authentication bypass — wrong password accepted
        email = unique_email()
        requests.post(f"{BASE_URL}/Auth/register", json={
            "email": email,
            "password": "CorrectPass123!",
            "name": "Wrong Pass Test",
            "phone": "0700000009"
        })
        response = requests.post(f"{BASE_URL}/Auth/login", json={
            "email": email,
            "password": "WrongPassword"
        })
        assert response.status_code == 401

    def test_login_nonexistent_email_returns_401(self):
        # PASS: 401 returned for non-existent email
        # FAIL: Server leaks info or crashes for unknown emails
        response = requests.post(f"{BASE_URL}/Auth/login", json={
            "email": "nobody_xyz_123@test.com",
            "password": "AnyPassword"
        })
        assert response.status_code == 401

    def test_login_empty_body_returns_400(self):
        # PASS: 400 returned for empty request body
        # FAIL: Server crashes or returns 500 for missing fields
        response = requests.post(f"{BASE_URL}/Auth/login", json={})
        assert response.status_code == 400
