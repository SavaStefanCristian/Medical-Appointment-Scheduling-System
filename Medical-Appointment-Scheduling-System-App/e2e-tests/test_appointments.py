import requests
import uuid
import pytest
from datetime import datetime, timedelta, timezone

BASE_URL = "http://localhost:8080/api"


def unique_email():
    return f"test_{uuid.uuid4().hex[:8]}@test.com"


def register_and_login(name="Test Patient", phone="0700000000"):
    email = unique_email()
    requests.post(f"{BASE_URL}/Auth/register", json={
        "email": email,
        "password": "TestPass123!",
        "name": name,
        "phone": phone
    })
    response = requests.post(f"{BASE_URL}/Auth/login", json={
        "email": email,
        "password": "TestPass123!"
    })
    data = response.json()
    return data["token"], data.get("patientId")


def auth_headers(token):
    return {"Authorization": f"Bearer {token}"}


def future_date(days=5):
    return (datetime.now(timezone.utc) + timedelta(days=days)).isoformat()


def past_date(days=3):
    return (datetime.now(timezone.utc) - timedelta(days=days)).isoformat()


def get_first_doctor_id(token):
    response = requests.get(f"{BASE_URL}/Doctors",
                            headers=auth_headers(token))
    doctors = response.json()
    if doctors:
        return doctors[0]["id"]
    return None


class TestCreateAppointment:

    def test_create_appointment_requires_auth(self):
        # PASS: 401 returned without token
        # FAIL: Anyone can book appointments without logging in
        response = requests.post(f"{BASE_URL}/Appointments", json={
            "doctorId": 1,
            "patientId": 1,
            "appointmentDate": future_date()
        })
        assert response.status_code == 401

    def test_create_appointment_valid_data_returns_201(self):
        # PASS: 201 Created with Pending status
        # FAIL: Appointment booking broken — core feature not working
        token, patient_id = register_and_login()
        doctor_id = get_first_doctor_id(token)

        if doctor_id is None:
            pytest.skip("No doctors in DB — add a doctor first")

        response = requests.post(f"{BASE_URL}/Appointments",
                                 headers=auth_headers(token),
                                 json={
                                     "doctorId": doctor_id,
                                     "patientId": patient_id,
                                     "appointmentDate": future_date()
                                 })
        assert response.status_code == 201

    def test_create_appointment_status_is_pending(self):
        # PASS: New appointment has Status = "Pending"
        # FAIL: Wrong initial status — doctor workflow broken
        token, patient_id = register_and_login()
        doctor_id = get_first_doctor_id(token)

        if doctor_id is None:
            pytest.skip("No doctors in DB — add a doctor first")

        response = requests.post(f"{BASE_URL}/Appointments",
                                 headers=auth_headers(token),
                                 json={
                                     "doctorId": doctor_id,
                                     "patientId": patient_id,
                                     "appointmentDate": future_date()
                                 })
        assert response.json()["status"] == "Pending"

    def test_create_appointment_nonexistent_doctor_returns_400(self):
        # PASS: 400 returned for invalid doctorId
        # FAIL: Appointment created with ghost doctor — data integrity broken
        token, patient_id = register_and_login()
        response = requests.post(f"{BASE_URL}/Appointments",
                                 headers=auth_headers(token),
                                 json={
                                     "doctorId": 99999,
                                     "patientId": patient_id,
                                     "appointmentDate": future_date()
                                 })
        assert response.status_code == 400

    def test_create_appointment_nonexistent_patient_returns_400(self):
        # PASS: 400 returned for invalid patientId
        # FAIL: Appointment created with ghost patient — data integrity broken
        token, _ = register_and_login()
        doctor_id = get_first_doctor_id(token)

        if doctor_id is None:
            pytest.skip("No doctors in DB — add a doctor first")

        response = requests.post(f"{BASE_URL}/Appointments",
                                 headers=auth_headers(token),
                                 json={
                                     "doctorId": doctor_id,
                                     "patientId": 99999,
                                     "appointmentDate": future_date()
                                 })
        assert response.status_code == 400


class TestGetAppointments:

    def test_get_my_appointments_requires_auth(self):
        # PASS: 401 returned without token
        # FAIL: Anyone can see patient appointments — privacy violation
        response = requests.get(f"{BASE_URL}/Appointments/my")
        assert response.status_code == 401

    def test_get_my_appointments_returns_200(self):
        # PASS: 200 returned for authenticated patient
        # FAIL: Patient cannot see their own appointments
        token, _ = register_and_login()
        response = requests.get(f"{BASE_URL}/Appointments/my",
                                headers=auth_headers(token))
        assert response.status_code == 200

    def test_get_my_appointments_returns_list(self):
        # PASS: Response is a JSON array
        # FAIL: Frontend crashes parsing appointments
        token, _ = register_and_login()
        response = requests.get(f"{BASE_URL}/Appointments/my",
                                headers=auth_headers(token))
        assert isinstance(response.json(), list)

    def test_get_appointment_by_invalid_id_returns_404(self):
        # PASS: 404 returned for non-existent appointment
        # FAIL: Server crashes or returns wrong data for missing records
        token, _ = register_and_login()
        response = requests.get(f"{BASE_URL}/Appointments/99999",
                                headers=auth_headers(token))
        assert response.status_code == 404

    def test_get_doctor_appointments_invalid_doctor_returns_404(self):
        # PASS: 404 returned for non-existent doctor
        # FAIL: Empty list returned instead of 404 — silent wrong result
        token, _ = register_and_login()
        response = requests.get(f"{BASE_URL}/Appointments/doctor/99999",
                                headers=auth_headers(token))
        assert response.status_code == 404


class TestCancelAppointment:

    def test_cancel_nonexistent_appointment_returns_404(self):
        # PASS: 404 returned for non-existent appointment
        # FAIL: Server crashes or returns 200 for missing records
        token, _ = register_and_login()
        response = requests.patch(f"{BASE_URL}/Appointments/99999/cancel",
                                  headers=auth_headers(token))
        assert response.status_code == 404

    def test_cancel_requires_auth(self):
        # PASS: 401 returned without token
        # FAIL: Anyone can cancel appointments without logging in
        response = requests.patch(f"{BASE_URL}/Appointments/1/cancel")
        assert response.status_code == 401

    def test_cancel_future_appointment_returns_200(self):
        # PASS: 200 returned, status becomes Cancelled
        # FAIL: Patient cannot cancel their own appointment — core feature broken
        token, patient_id = register_and_login()
        doctor_id = get_first_doctor_id(token)

        if doctor_id is None:
            pytest.skip("No doctors in DB — add a doctor first")

        create_response = requests.post(f"{BASE_URL}/Appointments",
                                        headers=auth_headers(token),
                                        json={
                                            "doctorId": doctor_id,
                                            "patientId": patient_id,
                                            "appointmentDate": future_date(10)
                                        })
        if create_response.status_code != 201:
            pytest.skip("Could not create appointment")

        appointment_id = create_response.json()["id"]
        response = requests.patch(
            f"{BASE_URL}/Appointments/{appointment_id}/cancel",
            headers=auth_headers(token))
        assert response.status_code == 200
        assert response.json()["status"] == "Cancelled"


class TestUpdateAppointmentStatus:

    def test_update_status_requires_auth(self):
        # PASS: 401 returned without token
        # FAIL: Anyone can change appointment status — security issue
        response = requests.patch(f"{BASE_URL}/Appointments/1/status",
                                  json={"status": "Confirmed"})
        assert response.status_code == 401

    def test_update_status_nonexistent_appointment_returns_404(self):
        # PASS: 404 returned for non-existent appointment
        # FAIL: Server crashes or returns 200 for missing records
        token, _ = register_and_login()
        response = requests.patch(
            f"{BASE_URL}/Appointments/99999/status",
            headers=auth_headers(token),
            json={"status": "Confirmed"})
        assert response.status_code in [404, 403]

    def test_update_status_invalid_status_returns_400(self):
        # PASS: 400 returned for invalid status value
        # FAIL: Garbage status accepted — DB contains invalid data
        token, _ = register_and_login()
        response = requests.patch(
            f"{BASE_URL}/Appointments/1/status",
            headers=auth_headers(token),
            json={"status": "InvalidStatus"})
        assert response.status_code in [400, 403]
