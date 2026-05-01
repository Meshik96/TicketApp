@echo off
title Lanzador Fullstack - Backend y Frontend
echo ==========================================
echo Preparando el entorno...
echo ==========================================

:: 1. Levantar el Backend
echo [1/2] Iniciando Backend .NET...
start "Backend API" cmd /k "dotnet run --project Backend/API/API.csproj --launch-profile https"

:: Esperar a que el backend esté listo
timeout /t 5 /nobreak > nul

:: 2. Levantar el Frontend
echo [2/2] Iniciando Frontend...

start "Frontend" cmd /k "cd Frontend && http-server -p 5500"
timeout /t 1 /nobreak > nul
start "" "http://localhost:5500/events.html"

echo ==========================================
echo Todo listo. Revisa las ventanas de comandos por si hay errores.
echo ==========================================
pause