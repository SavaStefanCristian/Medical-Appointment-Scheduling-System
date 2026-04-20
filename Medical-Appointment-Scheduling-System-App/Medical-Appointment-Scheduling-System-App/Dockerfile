FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080 

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Medical-Appointment-Scheduling-System-App.csproj", "./"]
RUN dotnet restore "./Medical-Appointment-Scheduling-System-App.csproj"

COPY . .
WORKDIR "/src/"
RUN dotnet build "Medical-Appointment-Scheduling-System-App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Medical-Appointment-Scheduling-System-App.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER app

ENTRYPOINT ["dotnet", "Medical-Appointment-Scheduling-System-App.dll"]