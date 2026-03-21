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
            // S17-NEW-02: AddNativeATTPlugin eliminado — ATTBridge.mm en Assets/Plugins/iOS/ ya lo cubre
            // Generarlo aquí causaba "duplicate symbols" en el linker de Xcode
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

    }
}
#endif
