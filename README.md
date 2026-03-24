# Medical Appointment Scheduling System API

Acest proiect reprezintă backend-ul pentru un sistem de programări medicale, construit cu **ASP.NET Core 8**. Oferă un API robust pentru gestionarea utilizatorilor (pacienți/doctori), a programărilor și include un sistem securizat de autentificare bazat pe JWT.

## 🏗️ Arhitectura Proiectului

Proiectul este structurat respectând principiile de Clean Code:

- **Entity Framework Core (EF Core):** Folosit ca ORM pentru interacțiunea cu baza de date PostgreSQL. Baza de date este generată și actualizată automat la pornirea aplicației folosind sistemul de Migrations.
- **DTOs (Data Transfer Objects):** Folosim DTO-uri pentru a decupla modelele bazei de date (Entities) de datele expuse către client, asigurând o comunicare sigură și curată a API-ului.

## ⚙️ Configurarea Bazei de Date (Connection String)

Aplicația folosește **PostgreSQL**. String-ul de conexiune se află în `appsettings.json`.
Parola bazei de date este protejată folosind un fișier de mediu. Înainte de a rula proiectul, trebuie să creezi un fișier `.env` în rădăcina proiectului cu următorul conținut:
`DB_PASSWORD=parola_ta_aici`

## 🚀 Setup Local

**Rulare prin Docker (Recomandat):**

1. Asigură-te că ai Docker Desktop instalat și pornit.
2. Deschide terminalul în folderul rădăcină (unde se află `docker-compose.yml`).
3. Rulează comanda: `docker compose up -d --build`
4. API-ul va fi disponibil la: `http://localhost:8080/swagger`

## 🔐 Testarea Endpoint-urilor (Swagger & JWT)

Interfața Swagger expune clar toate endpoint-urile disponibile (Users, Doctors, Patients, Appointments, Auth).

**Utilizarea token-ului JWT în Swagger:**

1. Apelează endpoint-ul de Login pentru a obține un token JWT valid.
2. Apasă pe butonul "Authorize" (lacătul) din Swagger UI.
3. Introdu token-ul în căsuță folosind formatul: `Bearer token_aici`
4. Apasă "Authorize" și testează endpoint-urile protejate.

---

## 🧪 Rularea Testelor Automate (Unit Testing)

Proiectul include o suită de teste unitare scrise cu **xUnit**, care validează logica de business a aplicației (în special fluxurile de Autentificare, Înregistrare și Autorizare pe roluri - cazuri Happy Path și Edge Cases). Baza de date este simulată folosind **SQLite In-Memory** pentru a permite testarea rapidă și izolată a tranzacțiilor, fără a afecta datele reale.

Pentru a rula testele, deschide un terminal în rădăcina proiectului (unde se află fișierul `.sln`) și execută comanda:

```bash
dotnet test
```
