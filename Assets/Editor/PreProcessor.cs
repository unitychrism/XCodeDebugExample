using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.IO;
using System.Linq;

/**
 * Unity Cloud Build compatible pre-export method script
 * - Uses UNITY_CLOUD_BUILD define that is present during cloud builds
 **/
public class PreProcessor : MonoBehaviour {

	#if UNITY_CLOUD_BUILD
	/**
	 * Cloud Build-specific pre-export method
	 * --------------------------------------
	 * 
	 * This method should be configured in Cloud Build build target
	 * advanced options. It will execute with the export path provided
	 * by Cloud Build, and provide proper build target for ProcessPreBuild method.
	 **/
	public static void OnPreExportIos(string exportPath)
	{
		Debug.Log("[UCB] OnPreExportIos started.");
		ProcessPreBuild(BuildTarget.iOS, exportPath);
	}
	#endif

	/**
	 * Pre-export method
	 **/
	private static void ProcessPreBuild(BuildTarget buildTarget, string path)
	{
		// Restrict execution to only when specific scene is active
	}
}
