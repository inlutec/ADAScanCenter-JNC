# ADAScanCenter - Instalación y Uso

## 📦 Instalador Generado

El instalador se encuentra en:
```
installer\installer\ADAScanCenter_Setup.exe
```

## 🚀 Instalación

1. **Ejecutar el instalador**: Doble clic en `ADAScanCenter_Setup.exe`
2. **Seguir el asistente**: El instalador te guiará paso a paso
3. **Opciones importantes**:
   - ✅ **"Iniciar automáticamente con Windows"** - Se recomienda marcar esta opción
   - ✅ **"Crear icono en el escritorio"** - Opcional pero recomendado

4. **Ubicación por defecto**: `C:\Program Files\ADAScanCenter\`

## ⚙️ Configuración Inicial

Al iniciar la aplicación por primera vez:

1. Se te pedirá establecer una **contraseña de administrador**
2. Esta contraseña protegerá el acceso a la configuración
3. **¡Guárdala en un lugar seguro!**

## 🔧 Configuración del Servicio

1. **Clic derecho** en el icono de la bandeja del sistema (escáner azul)
2. Seleccionar **"Administración"**
3. Ingresar la contraseña de administrador
4. Configurar:
   - **Servidor IMAP**: Dirección del servidor de correo
   - **Puerto**: Generalmente 993 para SSL
   - **Email Usuario**: Tu dirección de correo
   - **Contraseña**: Contraseña del correo (se guarda encriptada)
   - **Remitente (Filtro)**: Email del escáner (solo procesa correos de este remitente)
   - **Ruta Destino**: Carpeta donde se guardarán los escaneos
   - **Intervalo (seg)**: Frecuencia de comprobación de correos (por defecto 30 segundos)

5. **Probar Conexión**: Usa el botón para verificar que los datos son correctos
6. **Guardar**: Los cambios se aplican inmediatamente

### Mis credenciales (Panel de usuario)

Si cambias tu contraseña de correo, puedes actualizarla desde el **Panel de Control** sin necesidad de la contraseña de administrador:
- Clic en **⚙️ Mis credenciales**
- Actualiza email y/o contraseña
- **Probar Conexión** para verificar
- **Guardar**

Los datos del servidor IMAP (servidor, puerto, SSL, etc.) solo se configuran en Administración.

## 📁 Ubicación de Archivos

### Archivos de Configuración
```
C:\Users\[TuUsuario]\AppData\Roaming\ADAScanCenter\config.json
```

### Logs del Sistema
```
C:\Users\[TuUsuario]\AppData\Roaming\ADAScanCenter\Logs\
```

Los logs se generan diariamente con el formato: `ADAScanCenter_YYYYMMDD.log`

## 📋 Uso Diario

1. **Monitoreo Automático**: La aplicación funciona en segundo plano
2. **Icono en Bandeja**: 
   - 🟢 **Escáner azul** = Conectado y funcionando
   - 🔴 **Icono de error** = Problema de conexión

3. **Cuando llega un escaneo**:
   - Aparece una ventana emergente: **"Nuevo escaneo detectado"**
   - Opciones:
     - **Guardar como PDF**: Guarda el archivo original
     - **Guardar como JPG**: Abre el editor visual

4. **Editor Visual (JPG)**:
   - Navega entre páginas con los botones laterales
   - **Arrastra el mouse** sobre la imagen para seleccionar un área
   - Opciones:
     - **Guardar TODO (JPG)**: Todas las páginas como imágenes separadas
     - **Guardar Página Actual**: Solo la página visible
     - **Guardar RECORTADO**: Solo el área seleccionada

## 🔍 Menú Contextual

**Clic derecho** en el icono de la bandeja:

- **Administración**: Configuración completa del servidor IMAP (requiere contraseña)
- **Comprobar Ahora**: Forzar búsqueda inmediata de correos
- **Abrir Carpeta**: Abrir la carpeta de escaneos guardados
- **Salir**: Cerrar la aplicación

## 🛠️ Solución de Problemas

### La aplicación no se conecta al servidor IMAP

1. Verificar credenciales en Administración o Mis credenciales
2. Usar el botón **"Probar Conexión"**
3. Revisar los logs en `C:\Users\[TuUsuario]\AppData\Roaming\ADAScanCenter\Logs\`
4. Verificar que el puerto 993 no esté bloqueado por firewall

### No detecta correos nuevos

1. Verificar el **Remitente (Filtro)** en Administración
2. Asegurarse de que el email del escáner coincide exactamente
3. Usar **"Comprobar Ahora"** para forzar búsqueda
4. Revisar logs para ver si hay errores

### Olvidé la contraseña de administrador

1. Cerrar la aplicación completamente
2. Eliminar el archivo: `C:\Users\[TuUsuario]\AppData\Roaming\ADAScanCenter\config.json`
3. Reiniciar la aplicación
4. Se te pedirá configurar todo de nuevo

## 🔄 Actualización

Para actualizar a una nueva versión:

1. Ejecutar el nuevo instalador
2. Se detectará la versión anterior y se actualizará automáticamente
3. La configuración se mantiene intacta

## 🗑️ Desinstalación

1. **Panel de Control** → **Programas y características**
2. Buscar **ADAScanCenter**
3. Clic en **Desinstalar**
4. La aplicación se cerrará automáticamente y se eliminará

**Nota**: Los archivos de configuración en `AppData` no se eliminan automáticamente.

## 📊 Características del Instalador

- ✅ Instalación silenciosa disponible: `ADAScanCenter_Setup.exe /SILENT`
- ✅ Inicio automático con Windows (opcional)
- ✅ Detección y cierre automático de versiones anteriores
- ✅ Soporte para Windows 10 y 11 (64-bit)
- ✅ No requiere .NET Framework (incluido en el ejecutable)

## 🔐 Seguridad

- Las contraseñas se almacenan encriptadas usando DPAPI de Windows
- Solo el usuario actual puede desencriptar las contraseñas
- Los logs NO contienen información sensible

## 📞 Soporte

Para reportar problemas:
1. Revisar los logs en `C:\Users\[TuUsuario]\AppData\Roaming\ADAScanCenter\Logs\`
2. Incluir el archivo de log más reciente al reportar el problema
