// StoreKitBridge.mm
// Placeholder para bridge nativo de StoreKit 2.
//
// Unity IAP 5.1.2+ ya soporta StoreKit 2 internamente en iOS 15+.
// Este archivo es un PLACEHOLDER — no implementar hasta que se necesiten
// features avanzados de StoreKit 2 que Unity IAP no exponga:
//   - JWS transaction verification custom
//   - App Store promotional offers
//   - Transaction.currentEntitlements (personalizado)
//
// Si se implementa en el futuro:
//   - Registrar UnitySendMessage callback para comunicar con C#
//   - Usar StoreKit 2 API: Product, Transaction, AppStore
//   - #if TARGET_OS_IPHONE obligatorio
//
// Versión: Placeholder V1 — 2026-03-10

#include <Foundation/Foundation.h>

// Función vacía para evitar linker warnings
void _StoreKitBridgePlaceholder(void) {
    // No implementado en V1 — Unity IAP cubre todo lo necesario
}
