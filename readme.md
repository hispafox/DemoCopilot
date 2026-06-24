El objetivo de este proyecto es crear una aplicacion de lista de tareas.


Pasos a seguir para crear la aplicacion:

Analisis / Arquitectura
-----------------------
1-Diseño/Analisis completo de la aplicacion


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
1-Añadir validaciones
2-Comprobar la existencia de un identificador (de tarea por ejemplo)

Conectar con base de datos
--------------------------
1-Crear contexto de base de datos (dbcontext)
2-Crear entidades a partir del modelo
2-Consultas ( filtros )
3-Crear datos de ejemplo


