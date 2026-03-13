// ATTBridge.mm
// Plugin nativo iOS para App Tracking Transparency (ATT)
// Requerido por ATTService.cs — DllImport("__Internal")
// AUDIT-FIXED [2026-03-10] B-02: Creado para resolver crash por DllNotFoundException en iOS

#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>

extern "C" {

    /// Retorna el estado actual de ATT:
    /// 0 = NotDetermined, 1 = Restricted, 2 = Denied, 3 = Authorized
    int ATTService_GetTrackingStatus() {
        if (@available(iOS 14, *)) {
            return (int)[ATTrackingManager trackingAuthorizationStatus];
        }
        // iOS < 14: siempre autorizado
        return 3;
    }

    /// Muestra el dialog nativo de ATT y llama de vuelta a Unity
    /// con el status resultante vía SendMessage
    void ATTService_RequestTracking() {
        if (@available(iOS 14.5, *)) {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                dispatch_async(dispatch_get_main_queue(), ^{
                    // Enviar resultado a ATTService en Unity
                    NSString *statusStr = [NSString stringWithFormat:@"%d", (int)status];
                    UnitySendMessage("ATTService", "OnTrackingRequestComplete", [statusStr UTF8String]);
                });
            }];
        } else {
            // iOS < 14.5: tracking no requiere permiso, reportar como autorizado
            dispatch_async(dispatch_get_main_queue(), ^{
                UnitySendMessage("ATTService", "OnTrackingRequestComplete", "3");
            });
        }
    }

} // extern "C"
