# Medical Appointment Scheduling System

> Platformă web pentru programări medicale — pacienții își pot programa consultații la doctori, iar doctorii își pot gestiona programările.

## 👥 Echipa și Roluri

| Nume Student | Rol Principal | GitHub |
|---|---|---|
| Sava Stefan Cristian | DevOps / Team Lead | @SavaStefanCristian |
| Rotar Robert | Frontend Developer | @RotarRobert183 |
| Moroianu Andrei | Backend Developer | @MoroianuAndrei |
| Straton Andre | QA Engineer | @StratonAndre |

## 🏗️ Arhitectură și Tehnologii

| Layer | Tehnologie | Port |
|---|---|---|
| Frontend | Angular 19 | 4200 |
| Backend | ASP.NET Core 8 | 8080 |
| Database | PostgreSQL 15 | 5432 |

- **Containerizare:** Docker + Docker Compose
- **CI/CD:** GitHub Actions (Build → Lint → Test → Deploy)
- **Cloud:** [LINK_DEPLOY_AICI]
- **Management:** [GitHub Projects Board](https://github.com/users/SavaStefanCristian/projects/4)

## 📋 Funcționalități

- Înregistrare și autentificare pacienți/doctori (JWT)
- Pacienții pot crea, vizualiza și anula programări
- Doctorii își pot gestiona programările și statusul
- Listă și căutare doctori
- Interfață responsive Angular

## 🚀 Setup Local

**Cerințe:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalat și pornit.

```bash
git clone https://github.com/SavaStefanCristian/Medical-Appointment-Scheduling-System
cd Medical-Appointment-Scheduling-System
docker compose up -d --build
```

Accesează:
- **Frontend:** http://localhost:4200
- **API Swagger:** http://localhost:8080/swagger

## 🏗️ Arhitectura Proiectului

Proiectul este structurat respectând principiile de Clean Code:

- **Entity Framework Core (EF Core):** Folosit ca ORM pentru interacțiunea cu baza de date PostgreSQL. Baza de date este generată și actualizată automat la pornirea aplicației folosind sistemul de Migrations.
- **DTOs (Data Transfer Objects):** Folosim DTO-uri pentru a decupla modelele bazei de date (Entities) de datele expuse către client, asigurând o comunicare sigură și curată a API-ului.
- **JWT Authentication:** Sistem securizat de autentificare cu roluri (Patient / Doctor).

## ⚙️ Configurarea Bazei de Date (Connection String)

Aplicația folosește **PostgreSQL**. String-ul de conexiune se află în `appsettings.json`. Parola bazei de date este protejată folosind un fișier de mediu. Înainte de a rula proiectul, trebuie să creezi un fișier `.env` în rădăcina proiectului cu următorul conținut:DB_PASSWORD=parola_ta_aici

## 🔐 Testarea Endpoint-urilor (Swagger & JWT)

Interfața Swagger expune clar toate endpoint-urile disponibile (Users, Doctors, Patients, Appointments, Auth).

**Utilizarea token-ului JWT în Swagger:**
1. Apelează endpoint-ul de Login pentru a obține un token JWT valid.
2. Apasă pe butonul "Authorize" (lacătul) din Swagger UI.
3. Introdu token-ul în căsuță folosind formatul: `Bearer token_aici`
4. Apasă "Authorize" și testează endpoint-urile protejate.

## 🧪 Rularea Testelor Automate (Unit Testing)

Proiectul include o suită de teste unitare scrise cu **xUnit**, care validează logica de business a aplicației (în special fluxurile de Autentificare, Înregistrare și Autorizare pe roluri - cazuri Happy Path și Edge Cases). Baza de date este simulată folosind **SQLite In-Memory** pentru a permite testarea rapidă și izolată a tranzacțiilor, fără a afecta datele reale.

Pentru a rula testele, deschide un terminal în rădăcina proiectului (unde se află fișierul `.sln`) și execută comanda:

```bash
dotnet test
```

## 🐳 Infrastructură Docker

Aplicația pornește complet containerizat cu o singură comandă:

```bash
docker compose up -d --build
```

Servicii:
- `backend` — ASP.NET Core API (port 8080)
- `frontend` — Angular via Nginx (port 4200)
- `db` — PostgreSQL cu named volume pentru persistența datelor

## 📊 Management Proiect

- 28 issues gestionate cu Labels, Assignees, Estimates și Gantt (Roadmap)
- Branch strategy: `feat/` și `fix/` → PR → Code Review → merge `main`
- Branch `main` protejat (no direct push)

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/e7b75f5f-a1ef-4e9e-86bb-445291192e4c" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/e012c810-3a79-499e-8885-2879e23d5fb9" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/8ad69f0e-3992-4007-9a63-6068c1019471" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/240de457-830c-4679-80f0-8f3f0a8c9226" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/17bb4755-525c-4384-aeed-c7383d4719eb" />



