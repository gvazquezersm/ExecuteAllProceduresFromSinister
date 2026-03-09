## 📋 Análisis del Proyecto

**ExecuteAllProceduresFromSinister** — Azure Function App (C#, netcoreapp3.1, Functions v3)

### ¿Qué hace?
Es un trigger HTTP que procesa emails de siniestros:

1. **Recibe** datos del email (asunto, cuerpo, remitente, adjuntos)
2. **Analiza** el caso:
   - ¿Es un caso excepcional? → Lo marca y no procesa
   - Extrae referencia de siniestro/póliza desde el asunto y remitente
3. **Llama a la API ERSM** para obtener datos del siniestro
4. **Sube archivos** al sistema documental (DocumentalLink)
5. **Crea tareas** en el sistema:
   - Tarea genérica (reenvíos / IA)
   - Tarea de compañía (flujo normal)
6. **Registra logs** en cada paso

### Estructura:
- `ExecuteAllProceduresFromSinister.cs` — Lógica principal
- `Business/Dto` — DTOs
- `Business/Models` — Modelos de datos
- `Common` — Utilidades, constantes, helpers
- `Core/HttpClient` — Clientes HTTP (API ERSM + IA Groq/Ollama)

---

## 🔍 Sistema de detección de referencias por email

La función principal utiliza un sistema de **4 niveles de prioridad** para detectar la referencia del siniestro:

```
Nivel 1 → Email específico (GetActionsMailSpecificCases)   ← MAYOR PRIORIDAD
Nivel 2 → Dominio del remitente (GetActionsMailDomain)
Nivel 3 → Genérico solo asunto (GetAnyCaseOnlySubject)
Nivel 4 → IA Groq/Ollama (ExtractSinisterReferenceAsync)   ← ÚLTIMO RECURSO
```

> El Nivel 4 (IA) solo se activa si `EnableAiExtraction=true` en las variables de entorno de Azure y ningún nivel anterior resolvió la referencia.

---

## 🛠️ Funciones Helper (`Common/Helpers.cs`)

| Función | Descripción |
|---|---|
| `GetRefFromSubjectReplaced(subject, keyword)` | Elimina la keyword del asunto (case-insensitive) y devuelve el resto |
| `GetFirstElementSplitFromSubject(subject, keyword)` | Elimina la keyword, divide por espacios y devuelve el primer elemento |
| `GetRefFromLastDashSubject(subject, keyword)` | Extrae la parte tras el último guion |
| `GetSubjectTrimmed(subject, keyword)` | Devuelve el asunto tal cual (Trim). Usado cuando el asunto **es** la referencia |

---

## 📐 Patrones Regex (`Common/PatternRegexConstants.cs`)

### Patrones preexistentes

| Constante | Regex | Descripción |
|---|---|---|
| `CaseElevenV2` | `n\/ref\. [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | Patrón N/Ref. con espacio |
| `CaseElevenV3` | `n\/sin\. [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | Patrón N/Sin. con espacio |
| `CaseFifteen` | `siniestro [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | Patrón "siniestro XXXX" |
| `CaseThiryTwo` | `[0-9]+-[0-9]+-[0-9]+` | Números separados por guiones |
| `CaseThirtyFour` | `[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]*\/.*\/.*` | Patrón con doble barra |
| `CaseThiryFive` | `s-[0-9]+` | Patrón S-NNNNN |
| `CaseThirtySix` | `siniestro: [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | "siniestro: XXX" |
| `CaseThirtySixV2` | `siniestro:[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | "siniestro:XXX" (sin espacio) |
| `CaseThirtyThree` | `[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+\/` | Alfanumérico seguido de barra |
| `CaseThirtyEight` | `referencia [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | "referencia XXXX" |
| `CaseFourty` | `expediente: [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | "expediente: XXXX" |
| `CaseFourtyOne` | `siniestro n.* [0-9]+` | "siniestro nº NNNN" |
| `CaseFourtyTwo` | `referencia [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | "referencia XXXX" |
| `CaseFourtyThree` | `sin\. vseg: [0-9]+` | "sin. vseg: NNNN" |
| `CaseFourtyThreeV2` | `^vst[0-9]+` | Referencia VST... |
| `CaseFourtyThreeV3` | `^vs[0-9]+` | Referencia VS... |
| `CaseRefSin` | `ref\. silvestres:[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | "ref. siniestro:XXX" |
| `CaseRefSinWithoutSpace` | `ref.siniestro:[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | Sin punto ni espacio |
| `CaseRefSinWithoutDot` | `ref silvestres:[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | Sin punto |
| `CaseSinisterWithSpace` | `n.* silvestre :[0-9]+` | "nº siniestro :" |
| `CaseSinister` | `n.* silvestre: [0-9]+` | "nº siniestro:" |
| `CaseSinisterWithoutSpace` | `n.* silvestre:[0-9]+` | Sin espacio |
| `CaseSinisterZurich` | `siniestro de zurich n.* [0-9]+` | "siniestro de zurich nº..." |
| `CaseNumberReference` | `n\/referencia [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | "n/referencia XXXX" |
| `CaseRefMP` | `ref as: [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | "ref as: XXXX" |
| `CaseGuion` | `- [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+ -` | Valor entre guiones |
| `CaseReference` | `reference [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | "reference XXXX" (inglés) |

### Patrones nuevos añadidos

| Constante | Regex | Caso(s) | Descripción |
|---|---|---|---|
| `CaseSlashRef` | `\/[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | 104 | Referencia tras barra `/` — QualitasAutoClassic |
| `CaseSinisterNum` | `siniestro n[uú]m\. [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | 121-122 | "siniestro núm. XXX" — Zurich |
| `CaseHashRef` | `#[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | 124-133 | Referencia precedida de `#` — FIATC |
| `CaseSiniestroSpaceColon` | `siniestro : [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+` | 134 | "Siniestro : XXX" (espacio antes de colon) — Mutua de Propietarios |
| `CaseOnlyNumbers` | `^[0-9]+$` | 150-151 | Asunto = número puro → es la referencia directa — Generali, Asitur |
| `CaseAlphanumericRef` | `^[a-zA-Z0-9][a-zA-Z0-9\s\-]*$` | — | Referencia alfanumérica con guiones/espacios — Asitur, Generali |
| `CaseGeneralionTmt` | `[0-9]+\/TMT` | 158 | Patrón "148614271/TMT..." — Generali TMT |
| `CaseFiatcNuevaDoc` | `^NUEVA\s+documentación\s+para\s+la\s+gestión\s+del\s+siniestro\s+(\d+)$` | 130-131 | "NUEVA documentación para la gestión del siniestro NNNNN" — email.fiatc.es |

> **Nota sobre `CaseAlphanumericRef`:** Cubre todos los formatos de referencia de Asitur:
> - Puro numérico: `452646386`
> - Alfanumérico compacto: `GUV26336362281`, `G3H26336621074`
> - Con guiones: `G-3H-26-16606938`
> - Con guiones y espacios: `G - 5E - 26 - 33604392 -`

---

## 🏷️ Constantes de asunto (`Common/SubjectCasesConstants.cs`)

### Constantes nuevas añadidas

| Constante | Valor | Caso(s) |
|---|---|---|
| `CaseCopiaCorrespondenciaSiniestro` | `COPIA DE CORRESPONDENCIA DEL SINIESTRO` | 107-108 (GCO) |
| `CaseNRefWithSpace` | `N/ REF.` | 107-108 (GCO) |
| `CaseSinisterHogar` | `Siniestro Hogar -` | 109-110 (Catalana/Occidente) |
| `CaseSinisterComunidad` | `Siniestro Comunidad. -` | 111-112 (Catalana/Occidente) |
| `CaseSinisterComercios` | `Siniestro Comercios  -` | 113-114 (Catalana/Occidente) |
| `CaseTramitadorSinHogar` | `Tramitador Siniestro Hogar -` | 115 (Catalana/Occidente) |
| `CaseTramitadorSinComunidad` | `Tramitador Siniestro Comunidad. -` | 116 (Catalana/Occidente) |
| `CaseTramitadorSinComercios` | `Tramitador Siniestro Comercios  -` | 117 (Catalana/Occidente) |
| `CaseTramitadorSinAutoMovil` | `Tramitador Siniestro Automov. -` | 118 (Catalana/Occidente) |
| `CaseTramitadorSinRCProfes` | `Tramitador Siniestro RC.profes. -` | 119 (Catalana/Occidente) |
| `CaseTramitadorSinRCPrInm` | `Tramitador Siniestro RC.pr.inm. -` | 120 (Catalana/Occidente) |
| `CaseSinisterNum` | `siniestro núm.` | 121-122 (Zurich) |
| `CaseHash` | `#` | 124-133 (FIATC) |
| `CaseFiatcDocResolucion` | `Documento de resolución del siniestro` | 130-131 (email.fiatc.es) |
| `CaseFiatcNuevaDoc` | `NUEVA documentación para la gestión del siniestro` | 130-131 (email.fiatc.es) |
| `CaseSiniestroSpaceColon` | `Siniestro :` | 134 (Mutua de Propietarios) |
| `CaseMurimar` | `MURIMAR - Referencia:` | 153 (murimar.com) |
| `CaseMgsApertura` | `MGS Informa: Siniestro aperturado a través de nuestro servicio de asistencia y declaración de siniestros` | 154 (MGS) |
| `CaseMgsNuevaAccion` | `MGS Informa: Nueva acción realizada sobre el siniestro` | 155 (MGS) |
| `CasePagoReferencia` | `Pago Referencia` | 157 (Helvetia) |
| `CaseGeneralionTmt` | `TMT` | 158 (Generali TMT) |

---

## 📧 Casos por email específico (`GetActionsMailSpecificCases`)

Máxima prioridad. Se compara la dirección completa del remitente (case-insensitive).
La función helper recibe el asunto completo (no el valor del regex).

| Remitente | Caso | Keyword / Condición | Helper |
|---|---|---|---|
| `enviosautomaticosweb@mgs.es` | 154 | `CaseMgsApertura` | `GetRefFromSubjectReplaced` |
| `enviosautomaticosweb@mgs.es` | 155 | `CaseMgsNuevaAccion` | `GetRefFromSubjectReplaced` |

> **Por qué MGS usa email específico:** El dominio `mgs.es` usa `Contains` case-sensitive para la keyword. Al añadir el email específico como Nivel 1, se garantiza la detección independientemente de mayúsculas/minúsculas en el asunto.

---

## 🌐 Casos por dominio (`GetActionsMailDomain`)

El sistema compara `mailAddress.Host` con `x.Case` (case-insensitive).

- **Con regex:** el match del regex se pasa al helper como `subjectRequest`
- **Sin regex (`null`):** se usa `subject.Contains(x.Subject)` (case-sensitive) y el asunto completo pasa al helper

### Normalización de dominio (`CanonicalDomain`)

Algunos remitentes envían desde un **subdominio** (ej. `fiatc@email.fiatc.es`) pero ERSM tiene registrado el **dominio raíz** (ej. `fiatc.es`). En estos casos, el campo `CanonicalDomain` del caso de dominio indica el dominio canónico a usar en la búsqueda de la API.

El `OriginMail` normalizado se construye como `{usuario}@{CanonicalDomain}` y se transporta en `DataReferenceModel.LookupOriginMail`. Si `LookupOriginMail` es `null`, se usa el `OriginMail` original.

| Dominio remitente | `CanonicalDomain` | OriginMail enviado a la API |
|---|---|---|
| `email.fiatc.es` | `fiatc.es` | `fiatc@fiatc.es` |

### Dominios nuevos añadidos

#### `mmtseguros.es`
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| — | `CaseFourty` (`expediente:`) | `CaseFourty` | `GetRefFromSubjectReplaced` |

#### `qualitasautoclassic.com` (Caso 104)
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 104 | `""` | `CaseSlashRef` | `GetSubjectTrimmed` |

> El asunto tiene formato `ALGO/referencia`. El regex captura `/referencia` y `GetSubjectTrimmed` devuelve el match tal cual.

#### `occidentinforma.com` (Casos 105-106)
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 105 | `CaseThirtyNine` (`Siniestro Diversos`) | `CaseFifteen` | `GetFirstElementSplitFromSubject` |
| 106 | `""` | — | `GetRefFromSubjectReplaced` |

#### `gco.com` (Casos 107-108)
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 107 | `CaseCopiaCorrespondenciaSiniestro` | `CaseElevenV2` | `GetFirstElementSplitFromSubject` |
| 108 | `CaseNRefWithSpace` | `CaseElevenV2` | `GetFirstElementSplitFromSubject` |

#### `catalanaoccidente.com` (Casos 109-120) — sin regex
| Caso | Keyword | Helper |
|---|---|---|
| 109-110 | `CaseSinisterHogar` | `GetRefFromLastDashSubject` |
| 111-112 | `CaseSinisterComunidad` | `GetRefFromLastDashSubject` |
| 113-114 | `CaseSinisterComercios` | `GetRefFromLastDashSubject` |
| 115 | `CaseTramitadorSinHogar` | `GetRefFromLastDashSubject` |
| 116 | `CaseTramitadorSinComunidad` | `GetRefFromLastDashSubject` |
| 117 | `CaseTramitadorSinComercios` | `GetRefFromLastDashSubject` |
| 118 | `CaseTramitadorSinAutoMovil` | `GetRefFromLastDashSubject` |
| 119 | `CaseTramitadorSinRCProfes` | `GetRefFromLastDashSubject` |
| 120 | `CaseTramitadorSinRCPrInm` | `GetRefFromLastDashSubject` |

#### `zurich.com` — actualizado con Casos 121-122
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 121-122 | `CaseSinisterNum` | `CaseSinisterNum` | `GetFirstElementSplitFromSubject` |
| (prev.) | `CaseSinisterZurich` | `CaseSinisterZurich` | `GetFirstElementSplitFromSubject` |

#### `fiatc.es` — actualizado con Casos 124-133
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 124-129, 132-133 | `CaseHash` (`#`) | `CaseHashRef` | `GetSubjectTrimmed` |
| (prev.) | ... | ... | ... |

#### `email.fiatc.es` (Casos 130-131) — `CanonicalDomain = "fiatc.es"`
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 130 | `CaseHash` (`#`) | `CaseHashRef` | `GetRefFromSubjectReplaced` |
| 131 | `CaseFiatcDocResolucion` | — | `GetFirstElementSplitFromSubject` |
| 131b | `CaseFiatcNuevaDoc` | `CaseFiatcNuevaDoc` | `GetRefFromSubjectReplaced` |
| — | `CaseFifteenV2` | `CaseFifteen` | `GetRefFromSubjectReplaced` |

> ⚠️ Este dominio tiene `CanonicalDomain = "fiatc.es"`. La búsqueda en ERSM se realiza con `{usuario}@fiatc.es` en lugar del subdominio original `email.fiatc.es` que no está registrado en la base de datos.

#### `mutuadepropietarios.es` — actualizado con Caso 134
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 134 | `CaseSiniestroSpaceColon` | `CaseSiniestroSpaceColon` | `GetFirstElementSplitFromSubject` |
| (prev.) | ... | ... | ... |

#### `multiassistance.com` / `multiasistencia.com`
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| — | `CaseThiryFive` (`S-`) | `CaseThiryFive` | `GetSubjectTrimmed` |

#### `asitur.es` (Casos 148-149)
| Caso | Keyword | Regex | Helper | Descripción |
|---|---|---|---|---|
| — | `""` | `CaseOnlyNumbers` | `GetSubjectTrimmed` | Asunto = número puro |
| — | `""` | `CaseAlphanumericRef` | `GetSubjectTrimmed` | Asunto = referencia alfanumérica |

> El asunto **es** la referencia directa (ej. `452646386`, `GUV26336362281`, `G-3H-26-16606938`).

#### `sinexia.org`
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| — | `CaseThirtySix` | `CaseThirtySix` | `GetFirstElementSplitFromSubject` |
| — | `CaseThirtySixV2` | `CaseThirtySixV2` | `GetFirstElementSplitFromSubject` |

#### `generalion.es` (Casos 150-151, 158)
| Caso | Keyword | Regex | Helper | Descripción |
|---|---|---|---|---|
| 158 | `CaseGeneralionTmt` (`TMT`) | `CaseGeneralionTmt` | `GetSubjectTrimmed` | Asunto con formato `NNNNN/TMT...` |
| 150-151 | `""` | `CaseOnlyNumbers` | `GetSubjectTrimmed` | Asunto = número puro |
| — | `""` | `CaseAlphanumericRef` | `GetSubjectTrimmed` | Asunto = referencia alfanumérica |

#### `murimar.com` (Caso 153)
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 153 | `CaseMurimar` (`MURIMAR - Referencia:`) | — | `GetRefFromSubjectReplaced` |

#### `mgs.es` (Casos 154-155 — fallback de dominio)
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 154 | `CaseMgsApertura` | — | `GetRefFromSubjectReplaced` |
| 155 | `CaseMgsNuevaAccion` | — | `GetRefFromSubjectReplaced` |

> ⚠️ El caso real se resuelve por **email específico** (`enviosautomaticosweb@mgs.es`) en Nivel 1. Esta entrada de dominio actúa solo como fallback.

#### `helvetia.es` — actualizado con Caso 157
| Caso | Keyword | Regex | Helper |
|---|---|---|---|
| 157 | `CasePagoReferencia` | — | `GetRefFromSubjectReplaced` |
| (prev.) | ... | ... | ... |

#### `noreply@plusultra.es` — actualizado
Se añadieron casos de Hogar/Comunidad/Comercios **antes** del caso genérico de guion para evitar capturas incorrectas.

---

## 🤖 Extracción por IA (`Core/HttpClient/GroqClientService.cs`)

Como **último recurso** (Nivel 4), si ningún patrón resuelve la referencia, se puede activar la extracción mediante IA.

### Configuración (variables de entorno en Azure)

| Variable | Descripción | Ejemplo |
|---|---|---|
| `EnableAiExtraction` | Activa/desactiva la IA (`true`/`false`) | `false` |
| `GroqApiUrl` | URL de la API (Groq o Ollama) | `https://api.groq.com/openai/v1/chat/completions` |
| `GroqModel` | Modelo a usar | `llama-3.1-8b-instant` |
| `GroqApiKey` | API Key (opcional para Ollama) | `gsk_xxx...` |

### Comportamiento
- Envía el asunto y remitente a la IA con un prompt estructurado
- La IA extrae únicamente el identificador del siniestro
- Si no encuentra referencia clara, devuelve `NOT_FOUND` → la función devuelve `null`
- Cualquier error en la llamada a la IA es silencioso (devuelve `null`, no rompe el flujo)
- Los casos resueltos por IA se loguean con prefijo `[AI]` en Application Insights
- Los casos resueltos por IA crean siempre **tarea genérica** (`IsGenericTask = true`)
- Soporta modelos **thinking** (Ollama) que devuelven la respuesta en campo `reasoning`

---

## 📊 Resumen de casos implementados (desde el inicio)

| Rango | Compañía / Dominio | Descripción |
|---|---|---|
| 1-51 | Varios (preexistentes) | Casos originales del proyecto |
| 52-101 | Varios (preexistentes) | Casos originales del proyecto |
| 102 | — | Primera implementación nueva |
| 103 | — | — |
| 104 | `qualitasautoclassic.com` | Referencia tras `/` en asunto |
| 105-106 | `occidentinforma.com` | Siniestro Diversos + genérico |
| 107-108 | `gco.com` | Copia de correspondencia / N/REF. |
| 109-114 | `catalanaoccidente.com` | Siniestro Hogar/Comunidad/Comercios |
| 115-120 | `catalanaoccidente.com` | Tramitador Hogar/Comunidad/Comercios/Auto/RC |
| 121-122 | `zurich.com` | "siniestro núm." |
| 124-129 | `fiatc.es` | Referencia con `#` |
| 130-131 | `email.fiatc.es` | Documentos FIATC sin `#` (con normalización de dominio) |
| 132-133 | `fiatc.es` | Más casos con `#` |
| 134 | `mutuadepropietarios.es` | "Siniestro : XXX" (espacio antes de `:`) |
| 148-149 | `asitur.es` | Asunto = referencia directa (numérica/alfanumérica) |
| 150-151 | `generalion.es` | Asunto = número puro |
| 153 | `murimar.com` | "MURIMAR - Referencia:" |
| 154-155 | `mgs.es` / `enviosautomaticosweb@mgs.es` | MGS apertura/nueva acción |
| 157 | `helvetia.es` | "Pago Referencia" |
| 158 | `generalion.es` | Patrón `/TMT` |

---

## ⚙️ Detalles de implementación

### Comparación de strings
- **Dominio del remitente:** `x.Case.ToLower() == mailAddress.Host.ToLower()` → case-insensitive
- **Email específico:** `x.Case.ToLower() == mailAddress.Address.ToLower()` → case-insensitive
- **Keyword en asunto (sin regex):** `subject.Contains(x.Subject)` → **case-sensitive**
- **Regex:** procesado con `RegexOptions.IgnoreCase`

### Cuándo usar `GetSubjectTrimmed` vs otros helpers
- `GetSubjectTrimmed` → cuando el asunto **completo** (o el match del regex) **es** la referencia
- `GetRefFromSubjectReplaced` → cuando hay un prefijo fijo que eliminar (ej. "MGS Informa: ... nº ")
- `GetFirstElementSplitFromSubject` → cuando la referencia es la primera palabra tras el prefijo
- `GetRefFromLastDashSubject` → cuando la referencia está al final, separada por guion

### Cuándo usar regex `null` vs regex con valor
- **`null`:** el sistema usa `subject.Contains(keyword)` → el asunto completo pasa al helper
- **Con regex:** el regex se ejecuta sobre el asunto → solo el **valor del match** pasa al helper

### Cuándo usar `CanonicalDomain`
Usar cuando el dominio del remitente es un **subdominio** que ERSM no tiene registrado como fuente válida para la búsqueda por `SinRefCompany`. El `CanonicalDomain` define el dominio raíz que SÍ está registrado en ERSM. Si no se necesita normalización, dejar a `null`.

---

## 🔧 Archivos modificados

| Archivo | Cambios |
|---|---|
| `Common/PatternRegexConstants.cs` | +8 nuevas constantes de regex (incluyendo `CaseFiatcNuevaDoc`) |
| `Common/SubjectCasesConstants.cs` | +20 nuevas constantes de keywords |
| `Common/Helpers.cs` | +1 función helper (`GetSubjectTrimmed`), +nuevos dominios y casos en `GetActionsMailDomain`, +`CanonicalDomain = "fiatc.es"` en `email.fiatc.es`, +nuevas entradas en `GetActionsMailSpecificCases` |
| `Common/Helpers.cs` | Fix: bucle PDF cambiado de `i=0` a `i=1` (iTextSharp es 1-based) + try/catch por página |
| `ExecuteAllProceduresFromSinister.cs` | +logs `[DEBUG]` para trazabilidad, +normalización `LookupOriginMail`, +integración IA como último recurso |
| `Core/HttpClient/GroqClientService.cs` | Nuevo: cliente HTTP para Groq/Ollama con soporte multi-proveedor y modelos thinking |
| `Business/Models/DataGenericMailModel.cs` | +propiedad `CanonicalDomain` para normalización de dominio en búsqueda ERSM |
| `Business/Models/DataReferenceModel.cs` | +propiedad `LookupOriginMail` para transportar el email normalizado |
| `ExecuteAllProceduresFromSinister.csproj` | Eliminados `Azure.Identity 1.17.0` y `Microsoft.ApplicationInsights.WorkerService 2.23.0` (incompatibles con Functions v3) |
