using UnityEngine;
using System.Collections;

public class Share : MonoBehaviour
{
		public string title;
		public string content;
		static AndroidJavaClass sharePluginClass;
		static AndroidJavaClass unityPlayer;
		static AndroidJavaObject currActivity;

		void Start ()
		{
#if UNITY_ANDROID
        sharePluginClass = new AndroidJavaClass ("com.ari.tool.UnityAndroidTool");
				if (sharePluginClass == null) {
						Debug.Log ("sharePluginClass is null");
				} else {
						Debug.Log ("sharePluginClass is not null");
				}
				unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
				currActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
#endif
    }

		public void OpenRate ()
		{
#if UNITY_ANDROID
            CallShare(title, "", content);
#elif UNITY_IOS
        NativeShare.CallSocialShare("Share Unbox Simulator","Open all your crates with Unbox Simulator! https://itunes.apple.com/us/app/unbox-simulator/id1141413615");
#endif
    }
		public static void CallShare (string handline, string subject, string text)
		{
            Debug.Log ("share call start");
            sharePluginClass.CallStatic ("share", new object[] {
						handline,
						subject,
						text
            });
            Debug.Log ("share call end");
		}
}
