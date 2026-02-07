# ADAScanCenter - Documentación Completa

**Versión:** 1.0.0  
**Última actualización:** Febrero 2026

### Descarga

- **[ADAScanCenter_Setup.exe v1.0](https://nube.kiracloud.es/s/GRmNWqWMx5ze6Kx)** — Instalador para Windows (64-bit)

---

## Índice

1. [Introducción](#1-introducción)
2. [Guía de Usuario](#2-guía-de-usuario)
3. [Guía de Administrador](#3-guía-de-administrador)
4. [Documentación Técnica](#4-documentación-técnica)
5. [Estructura del Proyecto](#5-estructura-del-proyecto)
6. [Flujos y Diagramas](#6-flujos-y-diagramas)
7. [Requerimientos y Dependencias](#7-requerimientos-y-dependencias)
8. [Guía para Desarrolladores](#8-guía-para-desarrolladores)

---

## 1. Introducción

### 1.1 Descripción

**ADAScanCenter** es una aplicación Windows que monitorea una cuenta de correo IMAP para detectar escaneos enviados por email (PDF, JPG, PNG). Permite guardar los documentos directamente como PDF o procesarlos mediante un editor visual para convertirlos a imágenes JPG con recorte.

### 1.2 Características principales

- Monitoreo automático de correo IMAP (IDLE o polling)
- Procesamiento de adjuntos PDF, JPG, JPEG, PNG
- Guardado directo como PDF o conversión a JPG con editor visual
- Editor de imágenes con recorte por área seleccionada
- Filtro por remitente configurable
- Icono en bandeja del sistema
- Instancia única (reutiliza la existente al abrir de nuevo)
- Opción "Revisar más tarde" para diferir escaneos

### 1.3 Plataforma

- **Sistema operativo:** Windows 10/11 (64-bit)
- **Framework:** .NET 10
- **Tipo:** Aplicación de escritorio (Windows Forms)

---

## 2. Guía de Usuario

### 2.1 Instalación

1. Ejecutar `ADAScanCenter_Setup.exe`
2. Seguir el asistente de instalación
3. Opciones recomendadas:
   - **Iniciar automáticamente con Windows:** Marcado
   - **Crear icono en el escritorio:** Opcional
4. Ubicación por defecto: `C:\Program Files\ADAScanCenter\`

### 2.2 Primer inicio

1. Al ejecutar por primera vez, se solicita **establecer contraseña de administrador**
2. Esta contraseña protege la configuración completa
3. Guardar la contraseña en lugar seguro

### 2.3 Panel de Control (ventana principal)

Al abrir el acceso directo o usar "Comprobar Escaneo":

- **Abrir Carpeta de Escaneos:** Abre la carpeta donde se guardan los PDF/JPG
- **Comprobar Escaneo:** Fuerza una búsqueda inmediata de correos pendientes. También muestra de nuevo los escaneos marcados como "Revisar más tarde"
- **⚙️ (engranaje):** Abre "Mis credenciales" para actualizar email y contraseña (sin contraseña admin)

### 2.4 Icono en la bandeja del sistema

- **Escáner azul:** Conectado y funcionando
- **Icono de error rojo:** Desconectado (usar "Comprobar Ahora" para reconectar)

**Doble clic:** Abre la carpeta de escaneos

**Clic derecho:**
- **Administración:** Configuración completa (requiere contraseña admin)
- **Comprobar Ahora:** Buscar correos inmediatamente
- **Abrir Carpeta:** Abrir carpeta de escaneos
- **Salir:** Cerrar la aplicación

### 2.5 Cuando llega un escaneo

Aparece la ventana **"Nuevo Escaneo Detectado"** con opciones:

| Botón               | Acción                                                                 |
|---------------------|-----------------------------------------------------------------------|
| **Guardar PDF**     | Guarda el archivo original en la carpeta configurada                  |
| **Guardar JPG**     | Abre el editor visual para convertir páginas a imágenes              |
| **Revisar más tarde** | No muestra el popup hasta que uses "Comprobar Escaneo/Ahora"        |
| **Cancelar**        | Cierra la ventana; el scan volverá a aparecer en la siguiente comprobación |

### 2.6 Editor Visual (JPG)

Al elegir "Guardar JPG":

- **Panel lateral:** Miniaturas de cada página del PDF
- **Área central:** Vista de la página actual
- **Recorte:** Arrastrar el mouse para seleccionar un área rectangular

**Botones:**
- **Guardar TODO (JPG):** Guarda todas las páginas como imágenes separadas
- **Guardar Página Actual:** Solo la página visible
- **Guardar RECORTADO:** Solo el área seleccionada

Si cierra el editor sin guardar, el correo permanece en la bandeja y puede recuperarse con "Comprobar Escaneo".

### 2.7 Varios escaneos a la vez

Si hay varios correos con adjuntos no leídos, aparece **"Varios escaneos detectados"** para elegir cuál procesar primero.

### 2.8 Mis credenciales

Desde el Panel de Control (botón ⚙️):

- Actualizar **email** y **contraseña** de correo
- Probar conexión
- No requiere contraseña de administrador
- Los datos del servidor IMAP (servidor, puerto, etc.) solo se configuran en Administración

### 2.9 Rutas de archivos del usuario

| Tipo              | Ubicación                                                   |
|-------------------|-------------------------------------------------------------|
| **Escaneos guardados** | Carpeta configurada en Administración (por defecto: Documentos) |
| **Archivos temporales** | `%Temp%` (ej. `C:\Users\[Usuario]\AppData\Local\Temp\`)      |

---

## 3. Guía de Administrador

### 3.1 Acceso a Administración

1. Clic derecho en el icono de la bandeja
2. **Administración**
3. Introducir contraseña de administrador

### 3.2 Configuración IMAP

| Campo                  | Descripción                                                |
|------------------------|------------------------------------------------------------|
| **Servidor IMAP**      | Dirección del servidor (ej. `imap.zoho.eu`)               |
| **Puerto**             | Generalmente 993 (SSL)                                    |
| **Usar SSL/TLS**       | Recomendado activado                                       |
| **Email Usuario**      | Cuenta de correo que recibe los escaneos                  |
| **Contraseña**         | Contraseña del correo (se guarda encriptada con DPAPI)   |
| **Remitente (Filtro)** | Email del escáner; solo procesa correos de este remitente |

**Probar Conexión:** Verifica que servidor y credenciales sean correctos.

### 3.3 Opciones de monitoreo

| Campo                    | Descripción                                                                 |
|--------------------------|-----------------------------------------------------------------------------|
| **Usar IMAP IDLE**       | Activado: usa IDLE (menos carga). Desactivado: polling periódico          |
| **Comprobar soporte**    | Verifica si el servidor soporta IMAP IDLE                                  |
| **Timeout IDLE (seg)**   | 60 recomendado en Windows 11; 30–120 permitidos                            |
| **Intervalo polling (seg)** | Solo si IDLE está desactivado; ej. 30 segundos                         |

### 3.4 Ruta destino

- Carpeta donde se guardan los PDF y JPG
- Por defecto: `Documentos`
- Puede ser ruta local o de red

### 3.5 Ubicación de archivos del sistema

| Tipo         | Ruta                                                                 |
|--------------|----------------------------------------------------------------------|
| **Configuración** | `%AppData%\ADAScanCenter\config.json`                           |
| **Logs**     | `%AppData%\ADAScanCenter\Logs\ADAScanCenter_YYYYMMDD.log`           |

Ejemplo completo: `C:\Users\[Usuario]\AppData\Roaming\ADAScanCenter\`

### 3.6 Solución de problemas

**No se conecta:**
- Verificar credenciales y servidor
- Usar "Probar Conexión"
- Revisar logs en `%AppData%\ADAScanCenter\Logs\`

**No detecta correos:**
- Comprobar "Remitente (Filtro)"
- Usar "Comprobar Ahora"
- Revisar logs

**Conexión perdida:**
- El icono pasa a error
- Usar "Comprobar Ahora" para reconectar

**Contraseña admin olvidada:**
1. Cerrar la aplicación
2. Eliminar `%AppData%\ADAScanCenter\config.json`
3. Reiniciar; se pedirá configurar de nuevo

### 3.7 Instalación silenciosa

```cmd
ADAScanCenter_Setup.exe /SILENT
```

---

## 4. Documentación Técnica

### 4.1 Arquitectura general

```
┌─────────────────────────────────────────────────────────────┐
│                      Program.cs (entry)                      │
│  - Single instance / Show panel trigger                      │
│  - Pdfium native DLL resolver                                │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    AppTrayContext                             │
│  - Icono bandeja, menú contextual                             │
│  - Orquestación de escaneos (FileReceived, MultipleScans)    │
│  - Deferred scans (Revisar más tarde)                         │
└─────────────────────────────────────────────────────────────┘
          │              │              │
          ▼              ▼              ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ ConfigService│ │ ImapService  │ │FileProcessor  │
│ LogService   │ │              │ │              │
└──────────────┘ └──────────────┘ └──────────────┘
```

### 4.2 Servicios

| Servicio        | Responsabilidad                                           |
|-----------------|-----------------------------------------------------------|
| **ConfigService** | Carga/guardado de config.json, encriptación de contraseñas |
| **ImapService**   | Conexión IMAP, MonitorLoop, IDLE/polling, eventos        |
| **FileProcessor** | Mover/renombrar PDF a carpeta destino                     |
| **LogService**    | Logs en AppData\ADAScanCenter\Logs\                      |

### 4.3 Modelos de datos

**AppConfig** (`Models/AppConfig.cs`):

```
ImapServer, ImapPort, UseSsl, EmailUser, EmailPasswordEncrypted,
SenderEmailFilter, OutputDirectory, AdminPasswordHash,
PollingIntervalSeconds, UseImapIdle, IdleTimeoutSeconds
```

**ScanReceivedEventArgs** (en ImapService):

```
TempFilePath, OriginalFileName, MessageUid (UniqueId),
Subject, Date
```

### 4.4 Eventos IMAP

- **FileReceived:** Un solo escaneo detectado
- **MultipleScansAvailable:** Varios escaneos; el usuario elige uno
- **ConnectionStatusChanged(bool):** Conectado/desconectado

### 4.5 Seguridad

- Contraseñas encriptadas con **DPAPI** (`DataProtectionScope.CurrentUser`)
- Contraseña admin: hash SHA256
- Los logs no almacenan credenciales

---

## 5. Estructura del Proyecto

```
ADAScanCenter/
├── Program.cs                 # Punto de entrada, single-instance, Pdfium
├── AppTrayContext.cs          # Contexto de bandeja, flujo de escaneos
├── IconHelper.cs              # Icono de escáner para bandeja
├── app.ico                    # Icono de aplicación
├── app.manifest               # Manifest de Windows
├── channels4_profile.jpg      # Logo ADA (panel)
├── Models/
│   └── AppConfig.cs           # Modelo de configuración
├── Services/
│   ├── ConfigService.cs       # Configuración (JSON, encriptación)
│   ├── FileProcessor.cs       # Procesamiento de archivos
│   ├── ImapService.cs         # IMAP, IDLE, monitoreo
│   └── LogService.cs          # Logging
├── UI/
│   ├── ConfigForm.cs          # Administración (IMAP completo)
│   ├── MainForm.cs            # Panel de usuario
│   ├── NewScanForm.cs         # Modal "Nuevo escaneo detectado"
│   ├── ScanSelectForm.cs      # Selección cuando hay varios
│   ├── ImageEditorForm.cs     # Editor PDF → JPG
│   ├── UserCredentialsForm.cs # Mis credenciales (usuario)
│   ├── PasswordPrompt.cs      # Diálogo contraseña admin
│   └── SetPasswordForm.cs     # Establecer contraseña inicial
├── installer/
│   └── setup.iss              # Script Inno Setup
├── build.ps1                  # Script de compilación
├── generate-icon.ps1          # Generación de icono
└── COMPILAR.bat               # Compilación rápida
```

### 5.1 Archivos clave por funcionalidad

| Funcionalidad           | Archivo(s)                                |
|-------------------------|-------------------------------------------|
| Inicio, single-instance | Program.cs                                |
| Bandeja, menú           | AppTrayContext.cs                         |
| IMAP, IDLE, reconexión  | Services/ImapService.cs                   |
| Configuración           | Services/ConfigService.cs, Models/AppConfig.cs |
| Administración          | UI/ConfigForm.cs                          |
| Credenciales usuario    | UI/UserCredentialsForm.cs                 |
| Procesamiento escaneo   | AppTrayContext.ProcessSingleScan          |
| Editor PDF/JPG           | UI/ImageEditorForm.cs                     |
| Revisar más tarde       | AppTrayContext._deferredScanUids          |

---

## 6. Flujos y Diagramas

### 6.1 Flujo de inicio

```
Main(args)
  ├─ ConfigurePdfiumNativePath()
  ├─ ¿--service?
  │   ├─ Sí → AppTrayContext (modo background)
  │   └─ No → ¿Ya hay instancia?
  │       ├─ Sí → Crear trigger → Salir
  │       └─ No → AppTrayContext + MainForm
  └─ Run(context)
```

### 6.2 Flujo MonitorLoop (IMAP)

```
MonitorLoop
  └─ while (!cancelled)
       ├─ Config válida?
       │   └─ No → Delay 5s, continuar
       ├─ Connect → Authenticate → Open Inbox
       ├─ ConnectionStatusChanged(true)
       └─ while (conectado)
            ├─ Search (NotSeen [+ filtro remitente])
            ├─ Por cada uid: descargar adjuntos PDF/JPG/PNG
            ├─ Si 1 → FileReceived
            ├─ Si >1 → MultipleScansAvailable
            ├─ NoOp (verificar conexión)
            ├─ IDLE(timeout) o Polling(Delay + NoOp)
            └─ Si desconexión → break, finally, reintentar
```

### 6.3 Flujo de escaneo recibido

```
FileReceived / MultipleScansAvailable
  ├─ Filtrar _deferredScanUids (Revisar más tarde)
  ├─ Si vacío → return
  ├─ NewScanForm / ScanSelectForm + NewScanForm
  └─ Según elección:
       ├─ Guardar PDF → FileProcessor.ProcessFileAsync → DeleteMessage
       ├─ Guardar JPG → ImageEditorForm → Si SavedAny → DeleteMessage
       │                 Si no guardó → MarkAsUnreadAsync
       ├─ Revisar más tarde → _deferredScanUids.Add(uid)
       └─ Cancelar → (no hacer nada; volverá en siguiente check)
```

### 6.4 Flujo Comprobar Escaneo / Comprobar Ahora

```
ForceCheckNow / OnCheckNowClick
  ├─ _deferredScanUids.Clear()
  └─ ImapService.ForceCheckNow()
       ├─ Stop() (cancelar MonitorLoop)
       ├─ Sleep(500)
       └─ Nuevo MonitorLoop
```

### 6.5 Single-instance y Show Panel

```
Usuario abre acceso directo
  ├─ ¿Proceso ya existe?
  │   ├─ Sí → File.WriteAllText(TEMP\ADAScanCenter_ShowPanel.trigger)
  │   │        → Salir
  │   └─ No → Crear MainForm + AppTrayContext
  └─ Timer (500ms) en AppTrayContext
       └─ Si existe trigger → Borrar + Mostrar MainForm
```

---

## 7. Requerimientos y Dependencias

### 7.1 Requerimientos de sistema

- Windows 10/11 (64-bit)
- .NET 10 (incluido en el ejecutable self-contained)

### 7.2 Paquetes NuGet

| Paquete                                   | Versión    | Uso                         |
|-------------------------------------------|------------|-----------------------------|
| MailKit                                   | 4.14.1     | Cliente IMAP                 |
| Newtonsoft.Json                           | 13.0.4     | Serialización config         |
| PdfiumViewer                               | 2.13.0     | Visualización/conversión PDF |
| PdfiumViewer.Native.x86_64.v8-xfa         | 2018.4.8.256 | DLL nativa PDF (x64)      |
| PdfiumViewer.Native.x86.v8-xfa            | 2018.4.8.256 | DLL nativa PDF (x86)      |
| System.Security.Cryptography.ProtectedData | 10.0.2   | DPAPI para contraseñas       |

### 7.3 Herramientas de compilación

- **SDK:** .NET 10
- **Instalador:** Inno Setup 6.x (opcional)

### 7.4 Rutas de DLL nativas (Pdfium)

Con `PublishSingleFile`, las DLL nativas deben estar en:

- `{exe_dir}\x64\pdfium.dll`
- `{exe_dir}\x86\pdfium.dll`

`Program.ConfigurePdfiumNativePath()` registra un `DllImportResolver` para que PdfiumViewer las encuentre.

---

## 8. Guía para Desarrolladores

### 8.1 Compilación

**Desde línea de comandos:**

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishReadyToRun=true
```

**Script completo (incluye instalador):**

```powershell
.\build.ps1
```

**Archivo de salida:** `bin\Release\net10.0-windows\win-x64\publish\ADAScanCenter.exe`

### 8.2 Estructura de config.json

```json
{
  "ImapServer": "imap.example.com",
  "ImapPort": 993,
  "UseSsl": true,
  "EmailUser": "usuario@ejemplo.com",
  "EmailPasswordEncrypted": "<base64 DPAPI>",
  "SenderEmailFilter": "scanner@empresa.com",
  "OutputDirectory": "C:\\Users\\...\\Documents",
  "AdminPasswordHash": "<sha256>",
  "PollingIntervalSeconds": 30,
  "UseImapIdle": true,
  "IdleTimeoutSeconds": 60
}
```

### 8.3 Puntos de extensión

- **Nuevos formatos de adjunto:** En `ImapService.MonitorLoop`, ampliar la condición `ext == ".pdf" || ext == ".jpg" ...`
- **Nueva acción al guardar:** En `AppTrayContext.ProcessSingleScan`, añadir ramas según `form.UserChoice`
- **Cambio de UI:** Forms en `UI/`; colores y estilos definidos en cada formulario

### 8.4 Consideraciones importantes

1. **Threading:** Los eventos IMAP se disparan desde hilos de fondo; uso de `Invoke`/`BeginInvoke` en handlers de UI.
2. **Archivos temporales:** Nombres únicos con GUID para evitar bloqueos cuando el mismo correo se procesa varias veces.
3. **Reconexión:** MonitorLoop reintenta cada 5 s tras excepción; NoOp antes de IDLE para chequeo de conexión.
4. **Revisar más tarde:** `_deferredScanUids` se limpia al llamar a `ForceCheckNow`.
5. **Single-instance:** Detección por nombre de proceso; trigger en `%TEMP%` para mostrar panel.

### 8.5 Logs

- Rutas: `%AppData%\ADAScanCenter\Logs\ADAScanCenter_YYYYMMDD.log`
- Niveles: INFO, WARN, ERROR
- Prefijos: `[IMAP]`, `[Tray]` para filtrar por componente

### 8.6 Testing manual

1. Configurar servidor IMAP de prueba
2. Enviar correo con adjunto PDF al buzón configurado
3. Verificar: popup, Guardar PDF, Guardar JPG, Revisar más tarde, Cancelar
4. Probar "Comprobar Escaneo" para recuperar diferidos
5. Simular desconexión (Wi-Fi) y comprobar reconexión

---

## Resumen de cambios recientes (historial)

- **Revisar más tarde:** Los escaneos diferidos no muestran popup hasta "Comprobar"
- **Cancelar/cerrar:** El escaneo vuelve a aparecer en la siguiente comprobación
- **Archivos temporales únicos:** GUID en nombre para evitar "file in use"
- **IDLE mejorado:** Timeout 60s, NoOp pre-IDLE, más fiable en Windows 11
- **Reconexión:** NoOp antes de IDLE; manejo de IOException por archivo bloqueado
- **Dos configuraciones:** Administración (completa) vs Mis credenciales (solo email/contraseña)

---
