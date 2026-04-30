# Guía de Configuración y Ejecución del Proyecto

Esta guía detalla los pasos necesarios para instalar, configurar y ejecutar la solución completa (Backend .NET + Frontend Web) en una computadora nueva.

## 1. Requisitos Previos

Antes de comenzar, asegúrese de tener instaladas las siguientes herramientas:

*   **SDK de .NET 10.0**: Necesario para compilar y ejecutar el backend.
*   **Visual Studio 2026**: Con la carga de trabajo "Desarrollo de ASP.NET y web" instalada.
*   **SQL Server Express: Para la persistencia de datos (usuarios, eventos, tickets).
*   **Node.js**: Requerido para utilizar el servidor de desarrollo del frontend (`http-server`).
*   **Navegador Web**: Chrome o Edge (recomendado).

## 2. Configuración Inicial

### Clonar y Preparar el Repositorio
1. Descargue o clone el código en una carpeta local.
2. Abra la terminal en la raíz del proyecto y restaure las dependencias:
```
dotnet restore
```

3. Base de Datos
Verifique la cadena de conexión en "Backend/API/appsettings.json".

Asegúrese de que el servidor apunte a su instancia local de SQL Server (ej. "Server=.\\SQLEXPRESS" ).

Inicializar la base de datos:

- Abra la consola en "Backend/Infrastructure" y ejecute:
```
dotnet ef migrations add InitialCreate

dotnet ef database update
```

4. Ejecución mediante Script (.bat)

Localice el archivo Startproject.bat en la raíz.

Ejecútelo con doble clic. El script realizará lo siguiente:
```
1.Iniciará el servidor de .NET en una ventana independiente.

2.Levantará el servidor del frontend.

3.Abrirá automáticamente el navegador en la dirección del proyecto.
```
4. Solución de Problemas Comunes
Error CORS (ERR_FAILED): Asegúrese de estar accediendo vía http://localhost:5500 y no a través de una dirección IP , a menos que dicha IP esté explícitamente habilitada en la política de CORS en Program.cs.

ERR_CONNECTION_REFUSED: Verifique que la ventana de la consola del Backend esté abierta y no muestre errores de compilación o de conexión a la base de datos.

Puerto 5500 ocupado: Si recibe el error EADDRINUSE, cierre cualquier instancia de VS Code (Live Server) abierta o ejecute taskkill /F /IM node.exe en la terminal.

5. Tecnologías Utilizadas
Backend: C#, .NET 10.0, Entity Framework Core, SQL Server.
Frontend: HTML, CSS, JavaScript.
