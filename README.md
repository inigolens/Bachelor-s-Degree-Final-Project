# Proyecto Fin de Grado: Generación Procedural de Entornos en Videojuegos

## Introducción

El crecimiento exponencial de la industria de los videojuegos ha impulsado la necesidad de crear mundos virtuales vastos y detallados de manera eficiente. Este proyecto explora y aplica técnicas avanzadas de generación procedural para el desarrollo de entornos jugables, culminando en un prototipo que permite la exploración de sistemas solares y superficies planetarias generados de manera automática.

El enfoque principal es combinar tecnologías emergentes y algoritmos de generación procedural para optimizar el proceso creativo y ofrecer experiencias únicas e inmersivas a los jugadores.

## Características Principales

- **Sistemas solares generados proceduralmente:** Creación de sistemas solares con planetas únicos, cada uno con biomas y terrenos detallados.
- **Exploración espacial:** El jugador puede controlar una nave espacial para viajar entre planetas.
- **Exploración planetaria:** Los jugadores pueden aterrizar en planetas y explorar sus superficies, incluyendo cuevas y biomas diversos.
- **Técnicas avanzadas de generación:** Uso de algoritmos como Wave Function Collapse, Marching Cubes y mapas de ruido para crear entornos visualmente atractivos.
- **Experiencia relajante:** Diseño enfocado en la exploración sin elementos competitivos ni amenazas.

---

## Objetivos del Proyecto

El proyecto tiene como objetivo investigar y aplicar técnicas de generación procedural para desarrollar un prototipo de videojuego que cumpla con los siguientes propósitos:

1. **Investigación:** Estudiar las principales técnicas de generación procedural, incluyendo mapas de ruido, voxels, biomas y métodos de generación de terrenos y flora.
2. **Implementación de prototipo:** Diseñar un juego que integre estas técnicas, permitiendo al jugador explorar sistemas solares y planetas generados proceduralmente.
3. **Enfoque relajante:** Crear una experiencia de juego no competitiva, centrada en la exploración libre y la inmersión.

### Sub-objetivos técnicos

- Generar planetas con mapas de ruido para superficies realistas.
- Crear biomas diferenciados utilizando mapas de ruido y técnicas de transición suave.
- Implementar controles intuitivos para la navegación espacial y terrestre.
- Diseñar un sistema procedural robusto que garantice rendimiento estable y tiempos de carga mínimos.
- Incorporar un jetpack y un modo escáner para facilitar la exploración de terrenos complejos.

---

## Tecnologías Utilizadas

### Lenguajes y Motores:
- **Unity3D:** Motor de desarrollo principal para implementar las mecánicas del juego y los entornos 3D.
- **C#:** Lenguaje principal de programación para scripts y lógica del juego.
- **HLSL (High-Level Shading Language):** Para el desarrollo de ComputeShaders y optimización de tareas gráficas.

### Herramientas y Frameworks:
- **Blender:** Diseño de modelos 3D, como naves espaciales y personajes.
- **ShaderGraph:** Creación de shaders personalizados para efectos visuales avanzados.
- **GitHub:** Control de versiones y gestión del código fuente del proyecto.

### Algoritmos de Generación Procedural:
- **Mapas de ruido (Perlin, Simplex, Celular):** Creación de terrenos, texturas y biomas naturales.
- **Wave Function Collapse (WFC):** Generación coherente de estructuras complejas como niveles y mapas.
- **Marching Cubes:** Representación de superficies tridimensionales en entornos de voxels.
- **Octrees:** Optimización de cálculos en espacios 3D.

---

## Estructura del Proyecto

El proyecto está dividido en las siguientes carpetas y archivos:

- **Assets/**: Contiene los modelos, texturas, scripts y otros recursos.
- **Docs/**: Documentación técnica y manual de usuario.
- **Prototypes/**: Prototipos iniciales para probar las técnicas de generación procedural.
- **Scenes/**: Escenarios principales del juego (Sistema Solar, Exploración Planetaria, Menú de Inicio).

---

## Instalación

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/usuario/proyecto-generacion-procedural.git
   ```
2. **Abrir en Unity:**
   - Versión recomendada: Unity 2021.3 o superior.
   - Importar las dependencias necesarias desde el Unity Package Manager.

3. **Ejecutar el proyecto:**
   - Abre la escena inicial (`MainMenu.unity`).
   - Ejecuta el juego en el editor o exporta el proyecto para la plataforma deseada.

---

## Instrucciones de Uso

1. **Menú Principal:**
   - Generar un nuevo sistema solar o cargar uno existente.

2. **Exploración Espacial:**
   - Controla la nave espacial con los comandos del teclado.
   - Usa el modo "Turbo" para viajar rápidamente, con cuidado de evitar colisiones.

3. **Exploración Planetaria:**
   - Aterriza en un planeta y explora su superficie.
   - Activa el modo escáner para analizar materiales y descubrir recursos ocultos.

4. **Cambio de Escenarios:**
   - Transita entre el sistema solar y la exploración planetaria con una mecánica fluida y sin interrupciones.

---

## Desarrollo del Prototipo

El desarrollo se llevó a cabo utilizando metodologías ágiles basadas en Scrum, divididas en los siguientes sprints:

1. **Investigación de Tecnologías:** Exploración de algoritmos y técnicas procedurales.
2. **Prototipos Iniciales:** Creación de prototipos para mapas de ruido, voxels y WFC.
3. **Implementación del Sistema Solar:** Generación procedural de planetas y órbitas.
4. **Exploración Planetaria:** Desarrollo de biomas y mecánicas de exploración terrestre.
5. **Integración y Pruebas:** Validación de rendimiento y experiencia de usuario.

---

## Resultados

- **Rendimiento:** El prototipo mantiene un framerate estable incluso en entornos complejos.
- **Variedad Infinita:** Generación de sistemas solares y planetas virtualmente ilimitados.
- **Experiencia Inmersiva:** Diseño visual y mecánicas intuitivas para una exploración relajante.

---

## Contribuciones Futuras

Aunque se lograron los objetivos principales, el proyecto deja abierta la posibilidad de ampliar las funcionalidades:

1. **Soporte para Realidad Virtual:** Extensión del prototipo para dispositivos VR.
2. **Multijugador:** Incorporación de interacciones en línea.
3. **Generación Procedural de Fauna:** Desarrollo de algoritmos para crear criaturas únicas.


## Contacto

**Autor:** Iñigo Lens  
**Email:** inigolensblasco@gmail.ocm
**Universidad:** Universidad de Deusto  

