# Etapa 1: Baza pentru rularea aplicației (foarte ușoară, doar Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080 

# Etapa 2: Construirea aplicației (folosim SDK-ul complet)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiem fișierul proiectului și dăm restore la pachetele NuGet
COPY ["Medical-Appointment-Scheduling-System-App.csproj", "./"]
RUN dotnet restore "./Medical-Appointment-Scheduling-System-App.csproj"

# Copiem restul codului sursă
COPY . .
WORKDIR "/src/"
RUN dotnet build "Medical-Appointment-Scheduling-System-App.csproj" -c Release -o /app/build

# Etapa 3: Publicarea aplicației (generarea fișierelor finale)
FROM build AS publish
RUN dotnet publish "Medical-Appointment-Scheduling-System-App.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 4: Imaginea finală care va rula efectiv
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Punctul de pornire al aplicației
ENTRYPOINT ["dotnet", "Medical-Appointment-Scheduling-System-App.dll"]