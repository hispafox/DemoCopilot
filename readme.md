El objetivo de este proyecto es crear una aplicacion de lista de tareas.


Pasos a seguir para crear la aplicacion:

Analisis / Arquitectura
-----------------------
1-Diseño/Analisis completo de la aplicacion  → skill `diseño-analisis`


Elementos basicos
-----------------
1-Crear modelo         → skill `modelo`
2-Crear el controlador → skill `controlador`
3-crear los dtos       → skill `dto`
4-Crear los servicios  → skill `servicio`
5-Inyectar servicios   → incluido en el skill `servicio` (Paso 5)
6-Crear logica negocio → skill `logica-negocio`

Comprobar que los datos sean correctos
--------------------------------------
1-Añadir validaciones                              → skill `validaciones`
2-Comprobar la existencia de un identificador      → incluido en el skill `validaciones` (Paso 3)

Conectar con base de datos
--------------------------
1-Crear contexto de base de datos (dbcontext)  → skill `base-de-datos`
2-Crear entidades a partir del modelo          → incluido en el skill `base-de-datos` (Paso 7 — migraciones)
3-Consultas ( filtros )                        → incluido en el skill `logica-negocio`
4-Crear datos de ejemplo                       → incluido en el skill `base-de-datos` (Paso 6 — seeder)


Tests y Cobertura
----------------
1-Ejecutar tests                               → `dotnet test`
2-Generar informe HTML de cobertura en local   → `.\generar-informe-cobertura.ps1`
   - Ejecuta los tests con cobertura
   - Genera informe HTML en `coverage-report/index.html`
   - Abre el informe automáticamente en el navegador
3-Ver cobertura de CI                          → Descargar artefacto `backend-test-results` desde GitHub Actions


