# Kytame - Sistema de Puntuación de Taekwondo en Tiempo Real

Kytame es una aplicación web diseñada para gestionar el marcador de combates de Taekwondo, permitiendo el control de puntos, penalizaciones, rondas y temporizador de forma colaborativa y en tiempo real gracias a SignalR.

## Características

- Control de puntos y penalizaciones para dos equipos.
- Manejo de rondas (rounds) y temporizador configurable.
- Sincronización de la información del marcador en tiempo real entre varios dispositivos o navegadores.
- Interfaz de control y visualización separadas.
- Generación de códigos QR para facilitar el acceso rápido a los paneles desde dispositivos móviles.
- Límite de conexiones y grupos para mayor organización y control.
- Desarrollado con ASP.NET Core, Blazor WebAssembly y Azure SignalR.

## Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/luckberonne/Kytame.git
   ```

2. **Configuración de dependencias**
   - Requiere .NET 8.0 SDK o superior.
   - Requiere una instancia de Azure SignalR Service (ver archivo `Kytame/Properties/ServiceDependencies/Kytame - Web Deploy/signalr1.arm.json` para despliegue).
   - Configura la cadena de conexión de Azure SignalR como variable de entorno o en tu archivo `appsettings.json` (no se incluye ninguna clave privada en el repositorio).

3. **Ejecutar la aplicación**
   ```bash
   dotnet run --project Kytame/Kytame.csproj
   ```
   El frontend se encuentra en el proyecto `Kytame.Client`.

## Uso

- Accede a la ruta principal para configurar un nuevo combate y generar un grupo de marcador.
- Usa el panel de control para sumar/restar puntos y penalizaciones, controlar el temporizador y las rondas.
- Comparte el código QR generado para que otros dispositivos puedan visualizar el marcador en tiempo real.

## Arquitectura

- **Backend:** ASP.NET Core + SignalR para la gestión de la lógica y comunicación en tiempo real.
- **Frontend:** Blazor WebAssembly, páginas interactivas para control y visualización.
- **Sincronización:** SignalR permite que cualquier cambio en el marcador se refleje instantáneamente para todos los usuarios conectados al mismo grupo.

## Seguridad

- La comunicación se realiza a través de canales seguros usando Azure SignalR.

## Contribuciones

¡Las contribuciones son bienvenidas! Abre un issue o un pull request para sugerencias, correcciones o mejoras.

## Licencia

Proyecto privado. Contacta al autor para más detalles sobre su uso o distribución.

---

Desarrollado por [luckberonne](https://github.com/luckberonne)
