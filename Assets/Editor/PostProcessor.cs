using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;
using System.Linq;
using System.Xml.Linq;

public class BuildSettings
{
	public string configuration;
}

/**
 * Unity Cloud Build compatible post-export method script
 * 
 * - Conditionally adds Xcode namespace above (XCode Manipulation API)
 *     API Docs: http://docs.unity3d.com/ScriptReference/iOS.Xcode.PBXProject.html
 * - Uses UNITY_CLOUD_BUILD define that is present during cloud builds
 *     to include specified
 **/
public class PostProcessor : MonoBehaviour {

	#if UNITY_CLOUD_BUILD
	/**
	 * Cloud Build-specific post-export method
	 * --------------------------------------
	 * 
	 * This method should be configured in Cloud Build build target
	 * advanced options. It will execute with the export path provided
	 * by Cloud Build, and provide proper signature for ProcessPostBuild method.
	 **/
	public static void OnPostExportIos(string exportPath)
	{
		Debug.Log("[UCB] OnPostExportIos started.");
		ProcessPostBuild(BuildTarget.iOS, exportPath);
	}
	#endif

	/**
	 * Classic post-export method
	 * --------------------------
	 * 
	 * This method uses PostProcessBuild attribute in order to execute
	 * after export while building in the editor on your local machine.
	 **/
	[PostProcessBuild]
	public static void OnPostExportBuild(BuildTarget buildTarget, string path)
	{
		#if !UNITY_CLOUD_BUILD
		Debug.Log("[Editor] OnPostExportBuild started.");
		ProcessPostBuild(buildTarget, path);
		#endif
	}

	/**
	 * Private post-export method
	 * --------------------------
	 * 
	 * This is being executed in two separate fashions as seen above,
	 * based on whether or not the build is occurring in the Cloud.
	 * 
	 * - Path parameter points to root XCode project directory
	 *     where xcodeproj file is located
	 * - Uses XCode Manipulation API to:
	 *   - Force Debug configuration before XCode build step is executed
	 *   - Disable ENABLE_BITCODE to prevent large app filesizes
	 **/
	private static void ProcessPostBuild(BuildTarget buildTarget, string path)
	{
		// Restricting post-export behavior to only builds where specific Scene is active
		if (buildTarget == BuildTarget.iOS && IsSceneActive ("Assets/Scenes/CounterScene.unity")) 
		{
			// Initialize build settings
			var buildSettings = new BuildSettings {
				configuration = "Debug"
			};

			/**
			 * Manual manipulation of .xcscheme file
			 * -------------------------------------
			 * 
			 * .xcscheme file contains all information related to build schemes as seen
			 * in Project -> Schemes -> Edit Schemes dialog in XCode. In this case, we
			 * are forcing Debug configuration.
			 **/

			// Access xcscheme file and ingest xml
			string schemePath = path + "/Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme";
			var schemeReader = new StreamReader (schemePath);
			var xDoc = XDocument.Load(schemeReader);
			schemeReader.Close();

			Debug.Log(string.Format("Loaded scheme file: {0}", schemePath));

			// Set debug configuration for launch action
			foreach (XElement element in xDoc.Descendants("LaunchAction")) {
				element.SetAttributeValue("buildConfiguration", buildSettings.configuration);
				Debug.Log(string.Format("Set launch configuration to {0}", buildSettings.configuration));
			}

			// Write file back out
			xDoc.Save(schemePath);
			Debug.Log(string.Format("Saved scheme file: {0}", schemePath));

			/**
			 * XCode Project manipulation examples
			 * -----------------------------------
			 * 
			 * .pbxproj file contains information related to frameworks, build properties
			 * and other settings within an XCode project. See the Manipulation API docs:
			 * 
			 * http://docs.unity3d.com/ScriptReference/iOS.Xcode.PBXProject.html
			 **/

			// Access pbxproj file to add frameworks and build properties
			string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
			
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projPath));
			string target = proj.TargetGuidByName("Unity-iPhone");

			// Add user packages to project. Most other source or resource files and packages 
			// can be added the same way.
			  //CopyAndReplaceDirectory ("NativeAssets/TestLib.bundle", Path.Combine (path, "Frameworks/TestLib.bundle"));
			  //proj.AddFileToBuild (target, proj.AddFile ("Frameworks/TestLib.bundle", 
			  //                                       "Frameworks/TestLib.bundle", PBXSourceTree.Source));
			
			  //CopyAndReplaceDirectory ("NativeAssets/TestLib.framework", Path.Combine (path, "Frameworks/TestLib.framework"));
			  //proj.AddFileToBuild (target, proj.AddFile ("Frameworks/TestLib.framework", 
			  //                                       "Frameworks/TestLib.framework", PBXSourceTree.Source));
			
			// Add custom system frameworks. Duplicate frameworks are ignored.
			// needed by our native plugin in Assets/Plugins/iOS
			  //proj.AddFrameworkToProject (target, "AssetsLibrary.framework", false /*not weak*/);
			
			// Add our framework directory to the framework include path
			  //proj.SetBuildProperty (target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
			  //proj.AddBuildProperty (target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks");
			
			// Set a custom link flag
			  //proj.AddBuildProperty (target, "OTHER_LDFLAGS", "-ObjC");

			// Write changes back to file
			File.WriteAllText(projPath, proj.WriteToString());
		}
	}


	static bool IsSceneActive (string sceneName)
	{
		string[] levels = FillLevels();
		for (int i = 0; i < levels.Length; ++i) {
			if (levels [i] == sceneName) {
				return true;
			}
		}
		return false;
	}

	private static string[] FillLevels()
	{
		return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
	}
}