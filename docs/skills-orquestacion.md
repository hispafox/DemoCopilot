# Orquestación de Skills — DemoCopilot

Este documento describe el conjunto de skills disponibles en el proyecto, su propósito, las dependencias entre ellos y cómo se coordinan para construir la aplicación de forma incremental.

---

## 0. Agentes vs. Skills: la arquitectura completa

Los **skills** son las herramientas. Los **agentes** son quienes las usan.

```mermaid
flowchart TD
    U[Usuario] --> O[Orquestador]
    O --> P[Planificador]
    O --> D[Desarrollador]
    O --> V[Verificador]
    
    P --> CODE[Codigo]
    P --> PLAN[plan.md]
    
    D --> PLAN
    D --> SKILLS[Skills]
    SKILLS --> SK1[diseno-analisis]
    SKILLS --> SK2[modelo]
    SKILLS --> SK3[dto]
    SKILLS --> SK4[base-de-datos]
    SKILLS --> SK5[logica-negocio]
    SKILLS --> SK6[validaciones]
    SKILLS --> SK7[servicio]
    SKILLS --> SK8[controlador]
    SKILLS --> SK9[frontend-react]
    
    D --> CODE
    
    V --> CODE
    V --> O
    O --> GIT[Git]
    
    style O fill:#1f6feb,color:#fff
    style P fill:#238636,color:#fff
    style D fill:#238636,color:#fff
    style V fill:#238636,color:#fff
    style SKILLS fill:#fdf4ff,stroke:#a21caf,stroke-width:2px
```

**Flujo completo:**

1. **Usuario** invoca `@orquestador-democopilot <feature>`
2. **Orquestador** coordina el ciclo completo:
   - Llama a **Planificador** → genera `docs/plan-<slug>.md`
   - Llama a **Desarrollador** → lee el plan e **invoca los skills necesarios en orden**
   - Llama a **Verificador** → comprueba que compila y cumple criterios (bucle hasta 3 iter.)
   - Hace **commit + push** cuando el verificador da APROBADO

**El desarrollador es quien ejecuta los skills** según la tabla de skills del plan (sección 10 del plan). No hay un "orquestador de skills" separado — el agente desarrollador-democopilot lee el plan y va invocando cada skill que corresponda.

---

## 1. Catálogo de skills

| Skill | Carpeta generada | Responsabilidad |
|---|---|---|
| `nueva-feature` | _todas_ | **Implementación completa end-to-end.** Detecta qué capas afecta la feature y orquesta la invocación de todos los skills necesarios en orden. |
| `diseño-analisis` | `docs/` | Documento de análisis y diseño — fuente de verdad de todo lo demás |
| `modelo` | `Models/` | Entidades de dominio (clases C#) |
| `dto` | `Dtos/` | Contratos de entrada y salida de la API |
| `base-de-datos` | `Data/` | AppDbContext, Fluent API, migraciones, seeder |
| `logica-negocio` | `LogicaNegocio/` | Reglas de negocio + acceso a `DbContext` |
| `validaciones` | `Dtos/` + `LogicaNegocio/` | Anotaciones de validación y reglas de dominio |
| `servicio` | `Services/` | Orquestación: mapeo DTO ↔ entidad, delegación a lógica |
| `controlador` | `Controllers/` | Capa HTTP: recibe peticiones, llama al servicio, devuelve respuesta |
| `ui-ux-pro-max` | — | **VALORAR ANTES DE FRONTEND.** Catálogo de patrones UI/UX, heurísticas de usabilidad, accesibilidad y mejores prácticas para validar diseño antes de escribir código React |
| `frontend-react` | `frontend/` | Frontend React + Vite + TypeScript: tipos, servicios fetch, páginas y componentes |
| `actualizar-documentacion` | `docs/` | **Sincronizar docs con código.** Mantiene actualizada la documentación técnica cuando cambian agentes, skills o arquitectura. Corrige diagramas Mermaid problemáticos |
| `commit-message` | — | Genera el mensaje de commit siguiendo convenciones del proyecto |

---

## 2. Flujo de ejecución — orden obligatorio

El **agente desarrollador-democopilot** lee el plan generado por el planificador y ejecuta los skills en el orden especificado en la sección 10 del plan. Para implementaciones completas, el skill `nueva-feature` puede invocar todos los pasos de una vez.

Orden estándar de ejecución:

```mermaid
flowchart TD
    DEV([desarrollador-democopilot])

    DA[diseño-analisis]
    M[modelo]
    DTO[dto]
    BD[base-de-datos]
    LN[logica-negocio]
    VA[validaciones]
    SV[servicio]
    CT[controlador]
    UX[ui-ux-pro-max]
    FR[frontend-react]
    CM[commit-message]

    DEV --> DA
    DA --> M
    M --> DTO
    DTO --> BD
    BD --> LN
    LN --> VA
    VA --> SV
    SV --> CT
    CT --> UX
    UX --> FR
    FR --> CM

    style DEV fill:#238636,color:#fff,stroke:#0b3d91,stroke-width:2px
    style DA fill:#dbeafe,stroke:#3b82f6
    style M  fill:#dcfce7,stroke:#16a34a
    style DTO fill:#fef9c3,stroke:#ca8a04
    style BD fill:#e0f2fe,stroke:#0284c7
    style LN fill:#fce7f3,stroke:#db2777
    style VA fill:#fef3c7,stroke:#d97706
    style SV fill:#ede9fe,stroke:#7c3aed
    style CT fill:#ffedd5,stroke:#ea580c
    style UX fill:#fef3c7,stroke:#f59e0b
    style FR fill:#fdf4ff,stroke:#7c3aed
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
    DB["Data/AppDbContext.cs"]
    LN["LogicaNegocio/"]
    SV["Services/"]
    CT["Controllers/"]
    PR["Program.cs"]
    AP["appsettings.json"]

    AD -->|"secciones 4 y 5"| MO
    AD -->|"secciones 4 y 5"| DT
    AD -->|"sección 4"| DB
    AD -->|"sección 5"| LN
    AD -->|"sección 5"| SV
    AD -->|"sección 5"| CT

    MO -->|"tipos de entidad"| DB
    MO -->|"tipos de entidad"| LN
    MO -->|"tipos de entidad"| SV
    DT -->|"campos a validar"| DT
    DT -->|"firmas de métodos"| SV
    DT -->|"parámetros de acción"| CT
    DB -->|"AppDbContext inyectado"| LN
    LN -->|"validación de existencia"| LN
    LN -->|"interfaz I*Logica"| SV
    SV -->|"interfaz I*Service"| CT
    DT -->|"tipos TypeScript"| FR["frontend/src/types/"]
    CT -->|"endpoints consumidos"| FR

    DB -->|"AddDbContext"| PR
    DB -->|"ConnectionStrings"| AP
    LN -->|"AddScoped I*Logica"| PR
    SV -->|"AddScoped I*Service"| PR
```

---

## 4. Arquitectura de capas generada

Una vez ejecutados todos los skills, la aplicación queda estructurada en capas con responsabilidades claramente separadas:

```mermaid
flowchart LR
    subgraph HTTP["Capa HTTP"]
        C["Controller"]
    end

    subgraph DTO_IN["DTOs entrada"]
        DI["Crear*Dto Actualizar*Dto"]
    end

    subgraph DTO_OUT["DTOs salida"]
        DO["*Dto"]
    end

    subgraph SVC["Capa Orquestación"]
        SI["I*Service"]
        SS["*Service"]
        SI -.implementa.- SS
    end

    subgraph BL["Lógica Negocio"]
        LI["I*Logica"]
        LS["*Logica"]
        LI -.implementa.- LS
    end

    subgraph DATA["Acceso Datos"]
        DB["AppDbContext"]
    end

    subgraph DOM["Dominio"]
        MO["Models"]
    end

    C -->|"recibe"| DI
    C -->|"llama"| SI
    SI -->|"devuelve"| DO
    C -->|"responde"| DO

    SS -->|"mapea"| MO
    SS -->|"delega"| LI
    LS -->|"usa"| MO
    LS -->|"accede"| DB
```

---

## 5. Flujo de una petición en runtime

Cómo viajan los datos desde el cliente HTTP hasta la base de datos y de vuelta, incluyendo las validaciones:

```mermaid
sequenceDiagram
    actor Cliente
    participant C as Controller
    participant S as Service
    participant L as LogicaNegocio
    participant DB as AppDbContext

    Cliente->>C: POST /api/tareas
    Note over C: Valida ModelState
    alt ModelState inválido
        C-->>Cliente: 400 Bad Request
    else ModelState válido
        C->>S: CrearAsync(dto)
        Note over S: Mapea DTO a entidad
        S->>L: CrearAsync(entidad)
        Note over L: Valida reglas de negocio
        alt Regla violada
            L-->>S: throws Exception
            S-->>C: excepción propagada
            C-->>Cliente: 400 Bad Request
        else Todo OK
            L->>DB: Add + SaveChanges
            DB-->>L: entidad con Id
            L-->>S: entidad creada
            Note over S: Mapea entidad a DTO
            S-->>C: DTO
            C-->>Cliente: 201 Created
        end
    end
```

---

## 6. Gestión de `Program.cs`

Cada skill que genera clases registrables actualiza `Program.cs` con los registros de inyección de dependencias. El resultado final queda así:

```mermaid
flowchart TD
    PR["Program.cs"]

    BD_REG["AddDbContext SQLite"]
    LN_REG["AddScoped Logica"]
    SV_REG["AddScoped Service"]
    CT_REG["AddControllers"]
    MAP["MapControllers"]

    PR --> BD_REG
    PR --> LN_REG
    PR --> SV_REG
    PR --> CT_REG
    PR --> MAP

    BD_REG -->|"registrado por"| SK_BD["skill base-de-datos"]
    LN_REG -->|"registrado por"| SK_LN["skill logica-negocio"]
    SV_REG -->|"registrado por"| SK_SV["skill servicio"]
    CT_REG -->|"verificado por"| SK_CT["skill controlador"]
    MAP    -->|"verificado por"| SK_CT

    style SK_BD fill:#e0f2fe,stroke:#0284c7
    style SK_LN fill:#fce7f3,stroke:#db2777
    style SK_SV fill:#ede9fe,stroke:#7c3aed
    style SK_CT fill:#ffedd5,stroke:#ea580c
```

---

## 7. Cuándo usar cada skill

### Desde los agentes

Lo habitual es invocar **`@orquestador-democopilot <feature>`** y dejar que el ciclo completo se encargue:

1. El **planificador** genera el plan en `docs/plan-<slug>.md`
2. El **desarrollador** lee el plan y ejecuta los skills necesarios en orden
3. El **verificador** comprueba que todo compila y cumple criterios
4. El **orquestador** hace commit + push

### Ejecución manual de skills

Puedes invocar skills individuales cuando necesites actuar sobre una única capa de forma aislada (p. ej. ajustar solo un DTO sin tocar nada más), pero es menos común.

```mermaid
flowchart TD
    START([Nueva feature]) --> Q0{Afecta varias capas?}
    Q0 -->|Sí| AG["Invocar @orquestador-democopilot"]
    Q0 -->|No| Q1{Existe analisis-diseño?}
    AG --> END2([Listo])
    style AG fill:#1f6feb,color:#fff,stroke:#0b3d91,stroke-width:2px

    Q1 -->|No| DA["diseño-analisis"]
    Q1 -->|Sí| Q2{Existen modelos?}
    DA --> Q2

    Q2 -->|No| MO["modelo"]
    Q2 -->|Sí| Q3{Existen DTOs?}
    MO --> Q3

    Q3 -->|No| DTO["dto"]
    Q3 -->|Sí| Q4{Existe AppDbContext?}
    DTO --> Q4

    Q4 -->|No| BD["base-de-datos"]
    Q4 -->|Sí| Q5{Existe lógica negocio?}
    BD --> Q5

    Q5 -->|No| LN["logica-negocio"]
    Q5 -->|Sí| Q6{Tiene validaciones?}
    LN --> Q6

    Q6 -->|No| VA["validaciones"]
    Q6 -->|Sí| Q7{Existen servicios?}
    VA --> Q7

    Q7 -->|No| SV["servicio"]
    Q7 -->|Sí| Q8{Existen controladores?}
    SV --> Q8

    Q8 -->|No| CT["controlador"]
    Q8 -->|Sí| CM["commit-message"]
    CT --> CM

    CM --> END([Listo para commit])

    style DA  fill:#dbeafe,stroke:#3b82f6
    style MO  fill:#dcfce7,stroke:#16a34a
    style DTO fill:#fef9c3,stroke:#ca8a04
    style BD  fill:#e0f2fe,stroke:#0284c7
    style LN  fill:#fce7f3,stroke:#db2777
    style VA  fill:#fef3c7,stroke:#d97706
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
| Contexto de datos | — | `AppDbContext` | `AppDbContext` |

