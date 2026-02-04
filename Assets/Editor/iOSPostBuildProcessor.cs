#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace DigitPark.Editor
{
    /// <summary>
    /// Post-build processor para iOS
    /// Configura ATT, Deep Linking y frameworks necesarios
    /// </summary>
    public class iOSPostBuildProcessor
    {
        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            ModifyPlist(pathToBuiltProject);
            ModifyPbxProject(pathToBuiltProject);
            AddNativeATTPlugin(pathToBuiltProject);
        }

        /// <summary>
        /// Modifica Info.plist: ATT description + URL scheme
        /// </summary>
        private static void ModifyPlist(string pathToBuiltProject)
        {
            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            // === ATT: NSUserTrackingUsageDescription ===
            rootDict.SetString("NSUserTrackingUsageDescription",
                "We use this identifier to provide personalized ads and improve your gaming experience.");

            // === Deep Linking: URL Scheme digitpark:// ===
            PlistElementArray urlTypes = rootDict.CreateArray("CFBundleURLTypes");
            PlistElementDict urlScheme = urlTypes.AddDict();
            urlScheme.SetString("CFBundleURLName", "com.digitpark.app");
            PlistElementArray schemes = urlScheme.CreateArray("CFBundleURLSchemes");
            schemes.AddString("digitpark");

            plist.WriteToFile(plistPath);
            UnityEngine.Debug.Log("[iOSPostBuild] Info.plist actualizado: ATT + URL Scheme");
        }

        /// <summary>
        /// Modifica el proyecto Xcode: agrega frameworks
        /// </summary>
        private static void ModifyPbxProject(string pathToBuiltProject)
        {
            string pbxPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxPath);

            string mainTarget = project.GetUnityMainTargetGuid();
            string frameworkTarget = project.GetUnityFrameworkTargetGuid();

            // Agregar AppTrackingTransparency.framework (weak link para iOS < 14.5)
            project.AddFrameworkToProject(mainTarget, "AppTrackingTransparency.framework", true);
            project.AddFrameworkToProject(frameworkTarget, "AppTrackingTransparency.framework", true);

            project.WriteToFile(pbxPath);
            UnityEngine.Debug.Log("[iOSPostBuild] Frameworks agregados: AppTrackingTransparency");
        }

        /// <summary>
        /// Agrega el plugin nativo de ATT como archivo .m en el proyecto
        /// </summary>
        private static void AddNativeATTPlugin(string pathToBuiltProject)
        {
            string pluginContent =
                "#import <AppTrackingTransparency/AppTrackingTransparency.h>\n" +
                "#import <AdSupport/AdSupport.h>\n" +
                "\n" +
                "extern \"C\" {\n" +
                "    int ATTService_GetTrackingStatus() {\n" +
                "        if (@available(iOS 14, *)) {\n" +
                "            return (int)[ATTrackingManager trackingAuthorizationStatus];\n" +
                "        }\n" +
                "        return 3; // Authorized for iOS < 14\n" +
                "    }\n" +
                "\n" +
                "    void ATTService_RequestTracking() {\n" +
                "        if (@available(iOS 14.5, *)) {\n" +
                "            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {\n" +
                "                dispatch_async(dispatch_get_main_queue(), ^{\n" +
                "                    NSString *statusStr = [NSString stringWithFormat:@\"%d\", (int)status];\n" +
                "                    UnitySendMessage(\"ATTService\", \"OnTrackingRequestComplete\", [statusStr UTF8String]);\n" +
                "                });\n" +
                "            }];\n" +
                "        } else {\n" +
                "            // iOS < 14.5: tracking always authorized\n" +
                "            UnitySendMessage(\"ATTService\", \"OnTrackingRequestComplete\", \"3\");\n" +
                "        }\n" +
                "    }\n" +
                "}\n";
            string pluginDir = Path.Combine(pathToBuiltProject, "Libraries", "DigitPark");
            if (!Directory.Exists(pluginDir))
            {
                Directory.CreateDirectory(pluginDir);
            }

            string pluginPath = Path.Combine(pluginDir, "ATTPlugin.mm");
            File.WriteAllText(pluginPath, pluginContent);

            // Agregar al proyecto Xcode
            string pbxPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxPath);

            string mainTarget = project.GetUnityMainTargetGuid();
            string relativePath = "Libraries/DigitPark/ATTPlugin.mm";
            string fileGuid = project.AddFile(relativePath, relativePath, PBXSourceTree.Source);
            project.AddFileToBuild(mainTarget, fileGuid);

            project.WriteToFile(pbxPath);
            UnityEngine.Debug.Log("[iOSPostBuild] Plugin nativo ATT agregado");
        }
    }
}
#endif
