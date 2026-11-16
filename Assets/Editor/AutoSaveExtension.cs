using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EckTechGames
{
	[InitializeOnLoad]
	public class AutoSaveExtension
	{
		// Static constructor that gets called when unity fires up.
		static AutoSaveExtension()
		{
			EditorApplication.playModeStateChanged += AutoSaveWhenPlaymodeStarts;
		}

		private static void AutoSaveWhenPlaymodeStarts(PlayModeStateChange mode)
		{
			// If we're about to run the scene...
			if (mode == PlayModeStateChange.EnteredPlayMode)
			{
                // Save the scene and the assets.
                //EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				//AssetDatabase.SaveAssets();
			}
		}
	}
}