# ADAScanCenter

Aplicación Windows para recibir escaneos por correo electrónico (IMAP) y guardarlos localmente como PDF o JPG.

## Características

- Monitoreo automático de correo IMAP (IDLE o polling)
- Procesamiento de adjuntos PDF, JPG, PNG
- Guardado directo como PDF o conversión a JPG con editor visual
- Editor de imágenes con recorte por área seleccionada
- Filtro por remitente configurable
- Icono en bandeja del sistema
- Instancia única

## Requisitos

- Windows 10/11 (64-bit)
- .NET 10 (incluido en el ejecutable)

## Compilación

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

O usar el script completo:

```powershell
.\build.ps1
```

## Documentación

- [Instalación y uso](INSTALACION.md)
- [Documentación wiki completa](WIKI.md)

## Licencia

[Definir según corresponda]
