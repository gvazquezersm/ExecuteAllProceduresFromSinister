# ExecuteAllProceduresFromSinister — Azure Function

Azure Function HTTP Trigger (C#, .NET Core 3.1) que recibe correos electrónicos de aseguradoras y los procesa automáticamente para crear o actualizar expedientes de siniestros en el sistema de gestión ERSM/Elevia.

---

## Arquitectura general

```
Logic App de Azure
       │
       ▼
ExecuteAllProceduresFromSinister  (Azure Function HTTP Trigger)
       │
       ├── Recibe: DataMailModel (JSON con From, Subject, Body, Attachments...)
       │
       ├── 1. Limpia prefijos del asunto (RE:, FW:, RV:, FWD:)
       │
       ├── 2. Detecta el patrón del correo  ──────► patterns.json
       │         (3 niveles de búsqueda)             (fuente única de verdad)
       │
       ├── 3. Extrae la referencia del siniestro
       │         (mediante Helpers)
       │
       ├── 4. Llama a los procedimientos almacenados
       │         (SQL Server / Elevia)
       │
       └── Devuelve: resultado de procesamiento
```

---

## Flujo de detección de patrones (3 niveles)

La función aplica los niveles en orden. En cuanto uno coincide, se detiene.

### Nivel 1 — Email específico
Compara el remitente completo (`From`) con la lista `specificEmails` de `patterns.json`.

```
From: cts.autosnoreste@allianz.es
  └─► Coincide con specificEmails → aplica sus casos
```

### Nivel 2 — Dominio
Extrae el dominio del remitente y lo compara con `domains`. Si el dominio tiene `canonicalDomain`, ambos se consideran equivalentes.

```
From: mediador@allianz.es
  └─► Dominio: allianz.es → coincide en domains → aplica sus casos
```

### Nivel 3 — Patrón genérico de asunto
Si ningún email ni dominio coincide, se comparan los `genericSubjectCases` contra el asunto. Sirve como red de seguridad.

```
Subject: "VST2026000001 texto"
  └─► Ningún dominio conocido → regex ^vst\d{10} coincide → extrae referencia
```

---

## patterns.json — Fuente única de verdad

Ubicación: `ExecuteAllProceduresFromSinister/patterns.json`

Debe estar marcado como `CopyToOutputDirectory: PreserveNewest` en el `.csproj`.

### Estructura

```json
{
  "version": "1.0",
  "specificEmails": [
    {
      "email": "noreply@aseguradora.es",
      "description": "Descripción del caso",
      "cases": [ { ...caso... } ]
    }
  ],
  "domains": [
    {
      "domain": "allianz.es",
      "canonicalDomain": null,
      "description": "Allianz Seguros",
      "cases": [ { ...caso... } ]
    }
  ],
  "genericSubjectCases": [
    { ...caso... }
  ]
}
```

### Campos de un caso

| Campo | Tipo | Descripción |
|---|---|---|
| `keyword` | string | Texto fijo que debe aparecer en el asunto |
| `helper` | string | Nombre del método que extrae la referencia |
| `regex` | string \| null | Expresión regular para validar/capturar la referencia |
| `containMail` | string \| null | Subcadena que debe estar en el remitente (filtro adicional) |
| `isGenericTask` | bool | Si `true`, crea tarea genérica sin referencia de siniestro |
| `onlyLoad` | bool | Si `true`, solo carga datos, no abre expediente nuevo |

### Edición de patterns.json

Usar la herramienta **Pattern Manager** (Streamlit) en `scripts/pattern_manager/`. No editar el JSON a mano salvo casos muy simples.

---

## Helpers — métodos de extracción de referencia

Implementados en `Common/Helpers.cs`. Cada uno aplica una estrategia distinta para extraer la referencia del asunto.

| Helper | Estrategia | Ejemplo |
|---|---|---|
| `GetFirstElementSplitFromSubject` | Primer token tras eliminar el keyword | `"Siniestro 12345678 datos"` → `"12345678"` |
| `GetRefSinisterFromSubjectBetweenDash` | Texto entre el 1º y 2º guión | `"Tramitación - 12345678 - nombre"` → `"12345678"` |
| `GetRefFromLastDashSubject` | Texto antes del último guión | `"Expediente 12345678 - adjunto"` → `"12345678"` |
| `GetSinisterClaudatorsFromSubject` | Texto entre `[]` o `()` | `"Aviso [12345678]"` → `"12345678"` |
| `GetSubjectTrimmed` | El asunto completo recortado | `"12345678"` → `"12345678"` |
| `GetLastTokenFromMatch` | Último token del fragmento capturado por regex | Refs al final de cadena compleja |
| `GetRefFromSubjectReplaced` | Elimina el keyword y devuelve el resto | `"Ref: 12345678/A"` → `"12345678/A"` |
| `GetRefFromDeletingElementsAndJoin` | Elimina fragmentos y une el resultado | Referencias compuestas |
| `GetRefFromInitStringToCaseString` | Texto desde el inicio hasta el keyword | Refs que preceden al tipo de correo |

---

## Patrones VST / VS

Patrones genéricos para referencias ERSM enviadas directamente en el asunto.

### Formato esperado
- `VST` o `vst` (indistintamente) al **inicio del asunto**, sin ningún carácter previo
- Inmediatamente pegado, sin espacios, la **referencia de 10 dígitos** ERSM
- Formato de referencia: `AAAA` (año) + `NNNNNN` (secuencial) → ej: `2026000001`
- Tras los 10 dígitos puede haber cualquier texto adicional

### Ejemplos válidos
```
VST2026000001
vst2026000001 Siniestro comunidad
VST2025000123 - Gestión nueva
```

### Configuración en patterns.json
```json
{ "keyword": "vst", "helper": "GetRefFromSubjectReplaced", "regex": "^vst\\d{10}", "isGenericTask": true }
{ "keyword": "vs",  "helper": "GetRefFromSubjectReplaced", "regex": "^vs\\d{10}",  "isGenericTask": false, "onlyLoad": true }
```

> Los prefijos `RE:`, `FW:`, `RV:`, `FWD:` se eliminan automáticamente del asunto antes del matching.

---

## PatternLoader — carga de patterns.json

`Common/PatternLoader.cs` implementa un singleton thread-safe que carga `patterns.json` una sola vez y lo mantiene en caché.

### Resolución de ruta

El runtime de Azure Functions coloca los ficheros de contenido en un nivel distinto al DLL:

```
bin\Debug\netcoreapp3.1\          ← patterns.json (CopyToOutputDirectory)
bin\Debug\netcoreapp3.1\bin\      ← DLL del proyecto
```

Por eso PatternLoader sube un nivel si no encuentra el fichero:

```csharp
var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var path = Path.Combine(assemblyDir, "patterns.json");
if (!File.Exists(path))
    path = Path.Combine(Path.GetDirectoryName(assemblyDir), "patterns.json");
```

> No usar `AppContext.BaseDirectory` — apunta al directorio de las herramientas CLI de Azure Functions, no al proyecto.

### Métodos públicos

| Método | Descripción |
|---|---|
| `GetSpecificEmailPatterns()` | Patrones de emails específicos |
| `GetDomainPatterns()` | Patrones de dominio |
| `GetGenericSubjectPatterns()` | Patrones genéricos de asunto (VST, VS, sin. vseg:) |
| `InvalidateCache()` | Fuerza recarga del fichero en la próxima llamada |

---

## Resultados posibles

| Resultado | Significado |
|---|---|
| `Success` | Correo detectado y siniestro procesado correctamente |
| `Pattern not found` | Ningún patrón coincidió con el remitente ni el asunto |
| `Not processed: sinister not found` | Patrón detectado, referencia extraída, pero el siniestro no existe en BD |
| `Exception case` | Correo detectado como caso de excepción (lógica especial en C#) |

> **SINISTER_NOT_FOUND es normal**: el patrón funcionó correctamente pero la referencia no corresponde a ningún expediente activo. No indica un fallo del sistema de patrones.

---

## Casos de excepción (hardcoded en C#)

Algunos correos requieren lógica especial que no puede expresarse con keyword + helper. Estos casos permanecen en `Helpers.cs` como `DataExceptionMailModel`:

- Lógica que depende del cuerpo del correo, no solo del asunto
- Condiciones múltiples (AND/OR entre campos)
- Múltiples remitentes posibles bajo una misma lógica de negocio

---

## Variables de entorno

| Variable | Descripción |
|---|---|
| `EnableDebugLogs` | `"true"` activa logs detallados. Solo usar en local. En producción no existe → sin logs DEBUG |
| `EnableAiExtraction` | `"true"` activa extracción de referencia por IA (Groq) como último recurso |

---

## Seguridad al publicar — qué cambia y qué no

### Es seguro publicar esta versión porque:

- Todos los patrones existentes están en `patterns.json` — el comportamiento es idéntico al hardcode anterior
- La limpieza de `RE:/FW:/RV:/FWD:` es inocua para correos que ya no llevan ese prefijo
- VST/VS ahora detectan más casos que antes (mejora, nunca rotura)
- `PatternLoader` cachea el JSON una vez — mismo rendimiento que el hardcode

### Lo que NO cambia en producción (Azure):

- La Logic App no se ve afectada en absoluto
- La Azure Function no sabe nada de Graph API ni de movimiento de correos
- Los logs DEBUG (`EnableDebugLogs`) siguen sin emitirse en producción

### Lo que es exclusivamente local:

- El movimiento de correos a la carpeta `Procesados` solo ocurre cuando se ejecuta `ProcessOldEmails.ps1 -MoveSuccess` desde el PC local. La Azure Function nunca mueve correos.

---

## Cómo añadir un nuevo patrón

1. Identifica si es un email específico o un dominio genérico.
2. Abre **Pattern Manager** (Streamlit) → pestaña correspondiente.
3. Añade el caso con el keyword y elige el helper según cómo se extrae la referencia.
4. Guarda — `patterns.json` se actualiza al instante.
5. Reinicia la Azure Function para que `PatternLoader` recargue el fichero.

Para correos no detectados, usar la pestaña **Casos pendientes** del Pattern Manager, que captura automáticamente los `PATTERN_MISSING` durante el reprocesado.

---

## Estructura del proyecto

```
ExecuteAllProceduresFromSinister/
├── ExecuteAllProceduresFromSinister.cs   # Función principal (HTTP Trigger)
├── patterns.json                         # Fuente única de verdad de patrones
├── Business/
│   └── Models/
│       ├── DataMailModel.cs
│       ├── DataActionMailModel.cs
│       ├── DataDomainMailModel.cs
│       ├── DataReferenceModel.cs
│       └── PatternConfigModel.cs         # Modelos de deserialización del JSON
├── Common/
│   ├── Helpers.cs                        # Métodos de extracción de referencia
│   ├── PatternLoader.cs                  # Carga thread-safe de patterns.json
│   └── SubjectCasesConstants.cs          # Constantes legacy (referencia)
└── Services/
    └── GroqClientService.cs              # Cliente IA para extracción como fallback
```
