# Guía de Configuración y Ejecución del Proyecto

Esta guía detalla los pasos necesarios para instalar, configurar y ejecutar la solución completa (Backend .NET + Frontend Web) en una computadora nueva.

## 1. Requisitos Previos

### Antes de comenzar, asegúrese de tener instaladas las siguientes herramientas:

*   **SDK de .NET 10.0**: Necesario para compilar y ejecutar el backend.
*   **Entity Framework (EF) Core**: Para el mapeo objeto-relacional y la gestión de la base de datos.
*   **SQL Server Express**: Para la persistencia de datos.
*   **Node.js**: Requerido para utilizar el servidor de desarrollo del frontend (`http-server`).
*   **Navegador Web**:
  
### Instalación de Herramientas Globales

Es obligatorio ejecutar estos comandos una sola vez para habilitar las funciones de base de datos y el servidor del frontend:

*   **Instalar EF Core CLI**: Permite gestionar las migraciones de la base de datos desde la terminal.
  ```
  dotnet tool install --global dotnet-ef
  ```
*   **Instalar http-server**: Servidor ligero para levantar el frontend.
  ```
  npm install -g http-server
  ```

## 2. Configuración Inicial

### Clonar y Preparar el Repositorio
1. Descargue o clone el código en una carpeta local.
2. Abra la terminal en la raíz del proyecto y restaure las dependencias:
  ```
  dotnet restore
  ```

## 3. Base de Datos

1. Verifique la cadena de conexión en "Backend/API/appsettings.json" y en "Backend/Infrastructure/DbContextFactory,
y asegúrese de que el servidor apunte a su instancia local de SQL Server (ej. "Server=.\\SQLEXPRESS" ).

2. Inicializar la base de datos:

Abra la consola en la carpeta del proyecto de infraestructura (Backend/Infrastructure) y ejecute:
  ```
  dotnet ef migrations add InitialCreate

  dotnet ef database update
  ```

## 4. Ejecución mediante Script (.bat)

Localice el archivo Startproject.bat en la raíz.

Ejecútelo con doble clic. El script realizará lo siguiente:
```
1.Iniciará el servidor de .NET en una ventana independiente.

2.Levantará el servidor del frontend.

3.Abrirá automáticamente el navegador en la dirección del proyecto.
```
## 5. Solución de Problemas Comunes

- Error CORS (ERR_FAILED): Asegúrese de estar accediendo vía http://localhost:5500 y no a través de una dirección IP , a menos que dicha IP esté explícitamente habilitada en la política de CORS en Program.cs.

- ERR_CONNECTION_REFUSED: Verifique que la ventana de la consola del Backend esté abierta y no muestre errores de compilación o de conexión a la base de datos.

- Puerto 5500 ocupado: Si recibe el error EADDRINUSE, cierre cualquier instancia de VS Code (Live Server) abierta o ejecute taskkill /F /IM node.exe en la terminal.

## 6. Tecnologías Utilizadas.

Backend: C#, .NET 10.0, Entity Framework Core, SQL Server.

Frontend: HTML, CSS, JavaScript.

