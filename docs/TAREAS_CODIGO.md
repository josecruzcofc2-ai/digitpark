# TAREAS AUTOMATIZABLES POR CÓDIGO
**Ultima actualizacion**: 2026-03-25

---

## PENDIENTES — Con prerequisitos manuales

### C-05. IAP Localized Prices
- Reemplazar precios hardcodeados en USD con `product.metadata.localizedPriceString` de Unity IAP
- **ESPERAR**: Tarea manual #6 (IAP products en tiendas) completada

### C-06. Ad-Free guard en wrapper de ads
- Añadir guard: `if (PremiumManager.Instance?.IsAdFree == true) return;`
- **ESPERAR**: SDK de ads instalado (tarea manual #13)
