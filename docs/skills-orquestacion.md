# Orquestación de Skills — DemoCopilot

Este documento describe el conjunto de skills disponibles en el proyecto, su propósito, las dependencias entre ellos y cómo se coordinan para construir la aplicación de forma incremental.

---

## 1. Catálogo de skills

| Skill | Carpeta generada | Responsabilidad |
|---|---|---|
| `diseño-analisis` | `docs/` | Documento de análisis y diseño — fuente de verdad de todo lo demás |
| `modelo` | `Models/` | Entidades de dominio (clases C#) |
| `dto` | `Dtos/` | Contratos de entrada y salida de la API |
| `logica-negocio` | `LogicaNegocio/` | Reglas de negocio + acceso a `DbContext` |
| `servicio` | `Services/` | Orquestación: mapeo DTO ↔ entidad, delegación a lógica |
| `controlador` | `Controllers/` | Capa HTTP: recibe peticiones, llama al servicio, devuelve respuesta |
| `commit-message` | — | Genera el mensaje de commit siguiendo convenciones del proyecto |

---

## 2. Flujo de ejecución — orden obligatorio

Los skills tienen dependencias estrictas. El diagrama muestra el orden en que deben ejecutarse y qué artefacto produce cada uno:

```mermaid
flowchart TD
    A([Inicio]) --> DA

    DA["🔍 diseño-analisis\ndocs/analisis-diseño.md"]
    M["📦 modelo\nModels/*.cs"]
    DTO["📄 dto\nDtos/*Dto.cs"]
    LN["⚙️ logica-negocio\nLogicaNegocio/I*Logica.cs\nLogicaNegocio/*Logica.cs"]
    SV["🔀 servicio\nServices/I*Service.cs\nServices/*Service.cs"]
    CT["🌐 controlador\nControllers/*Controller.cs"]
    CM["✅ commit-message\nMensaje de commit"]

    DA --> M
    M --> DTO
    DTO --> LN
    LN --> SV
    SV --> CT
    CT --> CM

    style DA fill:#dbeafe,stroke:#3b82f6
    style M  fill:#dcfce7,stroke:#16a34a
    style DTO fill:#fef9c3,stroke:#ca8a04
    style LN fill:#fce7f3,stroke:#db2777
    style SV fill:#ede9fe,stroke:#7c3aed
    style CT fill:#ffedd5,stroke:#ea580c
    style CM fill:#f1f5f9,stroke:#64748b
```

---

## 3. Dependencias entre skills

Cada skill lee los artefactos de los skills anteriores como fuente de verdad. Nunca infiere ni inventa — si el prerequisito no existe, detiene la ejecución.

```mermaid
graph LR
    AD["docs/analisis-diseño.md"]
    MO["Models/"]
    DT["Dtos/"]
    LN["LogicaNegocio/"]
    SV["Services/"]
    CT["Controllers/"]
    PR["Program.cs"]

    AD -->|"lee secciones 4 y 5"| MO
    AD -->|"lee secciones 4 y 5"| DT
    AD -->|"lee sección 5"| LN
    AD -->|"lee sección 5"| SV
    AD -->|"lee sección 5"| CT

    MO -->|"tipos de entidad"| LN
    MO -->|"tipos de entidad"| SV
    DT -->|"firmas de métodos"| SV
    DT -->|"parámetros de acción"| CT
    LN -->|"interfaz I*Logica"| SV
    SV -->|"interfaz I*Service"| CT

    LN -->|"AddScoped"| PR
    SV -->|"AddScoped"| PR
```

---

## 4. Arquitectura de capas generada

Una vez ejecutados todos los skills, la aplicación queda estructurada en capas con responsabilidades claramente separadas:

```mermaid
flowchart LR
    subgraph HTTP["Capa HTTP"]
        C["Controller\n[ApiController]"]
    end

    subgraph DTO_IN["DTOs de entrada"]
        DI["Crear*Dto\nActualizar*Dto"]
    end

    subgraph DTO_OUT["DTOs de salida"]
        DO["*Dto"]
    end

    subgraph SVC["Capa de Orquestación"]
        SI["I*Service"]
        SS["*Service"]
        SI -.implementa.- SS
    end

    subgraph BL["Lógica de Negocio"]
        LI["I*Logica"]
        LS["*Logica"]
        LI -.implementa.- LS
    end

    subgraph DATA["Acceso a Datos"]
        DB["AppDbContext\n(EF Core + SQLite)"]
    end

    subgraph DOM["Dominio"]
        MO["Models/\n*.cs"]
    end

    C -->|"recibe"| DI
    C -->|"llama"| SI
    SI -->|"devuelve"| DO
    C -->|"responde"| DO

    SS -->|"mapea DTO → entidad"| MO
    SS -->|"delega"| LI
    LS -->|"usa"| MO
    LS -->|"accede"| DB
```

---

## 5. Flujo de una petición en runtime

Cómo viajan los datos desde el cliente HTTP hasta la base de datos y de vuelta:

```mermaid
sequenceDiagram
    actor Cliente
    participant C as Controller
    participant S as Service
    participant L as LogicaNegocio
    participant DB as AppDbContext

    Cliente->>C: POST /api/tareas\n{ "titulo": "..." }
    Note over C: Valida ModelState (automático con [ApiController])
    C->>S: CrearAsync(CrearTareaDto)
    Note over S: Mapea DTO → entidad TodoItem
    S->>L: CrearAsync(TodoItem)
    Note over L: Aplica reglas de negocio
    L->>DB: Add(todoItem) + SaveChangesAsync()
    DB-->>L: todoItem con Id asignado
    L-->>S: TodoItem creado
    Note over S: Mapea entidad → TareaDto
    S-->>C: TareaDto
    C-->>Cliente: 201 Created\n{ "id": 1, "titulo": "...", ... }
```

---

## 6. Gestión de `Program.cs`

Cada skill que genera clases registrables actualiza `Program.cs` con los registros de inyección de dependencias. El resultado final queda así:

```mermaid
flowchart TD
    PR["Program.cs"]

    LN_REG["builder.Services\n.AddScoped&lt;I*Logica, *Logica&gt;()"]
    SV_REG["builder.Services\n.AddScoped&lt;I*Service, *Service&gt;()"]
    CT_REG["builder.Services\n.AddControllers()"]
    MAP["app.MapControllers()"]

    PR --> LN_REG
    PR --> SV_REG
    PR --> CT_REG
    PR --> MAP

    LN_REG -->|"registrado por"| SK_LN["skill logica-negocio"]
    SV_REG -->|"registrado por"| SK_SV["skill servicio"]
    CT_REG -->|"verificado por"| SK_CT["skill controlador"]
    MAP    -->|"verificado por"| SK_CT

    style SK_LN fill:#fce7f3,stroke:#db2777
    style SK_SV fill:#ede9fe,stroke:#7c3aed
    style SK_CT fill:#ffedd5,stroke:#ea580c
```

---

## 7. Cuándo usar cada skill

```mermaid
flowchart TD
    START([Nueva feature / nuevo recurso]) --> Q1{¿Existe\ndocs/analisis-diseño.md?}

    Q1 -->|No| DA["▶ Ejecutar\ndiseño-analisis"]
    Q1 -->|Sí| Q2{¿Existen\nlos modelos?}

    DA --> Q2

    Q2 -->|No| MO["▶ Ejecutar\nmodelo"]
    Q2 -->|Sí| Q3{¿Existen\nlos DTOs?}

    MO --> Q3

    Q3 -->|No| DTO["▶ Ejecutar\ndto"]
    Q3 -->|Sí| Q4{¿Existe\nlógica de negocio?}

    DTO --> Q4

    Q4 -->|No| LN["▶ Ejecutar\nlogica-negocio"]
    Q4 -->|Sí| Q5{¿Existen\nlos servicios?}

    LN --> Q5

    Q5 -->|No| SV["▶ Ejecutar\nservicio"]
    Q5 -->|Sí| Q6{¿Existen\nlos controladores?}

    SV --> Q6

    Q6 -->|No| CT["▶ Ejecutar\ncontrolador"]
    Q6 -->|Sí| CM["▶ Ejecutar\ncommit-message"]

    CT --> CM
    CM --> END([Listo para commit])

    style DA  fill:#dbeafe,stroke:#3b82f6
    style MO  fill:#dcfce7,stroke:#16a34a
    style DTO fill:#fef9c3,stroke:#ca8a04
    style LN  fill:#fce7f3,stroke:#db2777
    style SV  fill:#ede9fe,stroke:#7c3aed
    style CT  fill:#ffedd5,stroke:#ea580c
    style CM  fill:#f1f5f9,stroke:#64748b
```

---

## 8. Convenciones de nomenclatura por capa

| Capa | Interfaz | Implementación | Ejemplo |
|---|---|---|---|
| Lógica de negocio | `I<Recurso>Logica` | `<Recurso>Logica` | `ITareaLogica` / `TareaLogica` |
| Servicio | `I<Recurso>Service` | `<Recurso>Service` | `ITareaService` / `TareaService` |
| Controlador | — | `<Recurso>Controller` | `TareasController` |
| DTO entrada crear | — | `Crear<Recurso>Dto` | `CrearTareaDto` |
| DTO entrada actualizar | — | `Actualizar<Recurso>Dto` | `ActualizarTareaDto` |
| DTO salida | — | `<Recurso>Dto` | `TareaDto` |
| Entidad de dominio | — | `<Recurso>` | `TodoItem` |
