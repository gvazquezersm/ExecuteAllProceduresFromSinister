## 📋 Análisis del Proyecto

**ExecuteAllProceduresFromSinister** — Azure Logic App (C#)

### ¿Qué hace?
Es un trigger HTTP que procesa emails de siniestros:

1. **Recibe** datos del email (asunto, cuerpo, remitente, adjuntos)
2. **Analiza** el caso:
   - ¿Es un caso excepcional? → Lo marca y no procesa
   - Extrae referencia de siniestro/póliza desde el asunto y remitente
3. **Llama a la API ERSM** para obtener datos del siniestro
4. **Sube archivos** al sistema documental (DocumentalLink)
5. **Crea tareas** en el sistema:
   - Tarea genérica (reenvíos)
   - Tarea de compañía (flujo normal)
6. **Registra logs** en cada paso

### Estructura:
- `ExecuteAllProceduresFromSinister.cs` — Lógica principal
- `Business/Dto` — DTOs
- `Business/Models` — Modelos de datos
- `Common` — Utilidades, constantes, helpers
- `Core/HttpClient` — Cliente HTTP para APIs

---
