/******************************************
  * uWebKit 
  * (c) 2014 THUNDERBEAST GAMES, LLC
  * http://www.uwebkit.com
  * sales@uwebkit.com
*******************************************/


//#define ENABLE_DEV_BUTTONS

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple control menu for the Browser Example
/// </summary>
public class WebBrowserExample : MonoBehaviour
{

	WebBrowser browser;

	SourceCodePopup sourcePopup;

	void Awake()
	{		
		browser = GameObject.FindObjectOfType (typeof(WebBrowser)) as WebBrowser;	

		browser.X = 160 + 32;
	}	

	void SourcePopupClosed()
	{
		UnityEngine.Object.Destroy(gameObject.GetComponent<SourceCodePopup>());		
		//webGUI.HasFocus = true;
	}


	void OnGUI ()
	{
		Rect brect = new Rect (0, 0, 160, 40);

		if (UWKCore.BetaVersion)
		{
			GUI.Label(new Rect (0, 0, 200, 60), "UWEBKIT BETA VERSION\nCheck http://www.uwebkit.com\nfor updates");
			brect.y += 50;
		}		
		
		if (GUI.Button (brect, "Back")) 
		{
            SceneManager.LoadScene("ExampleLoader");
		}

		brect.y += 50;

		if (SourceCodePopup.usePopup)
		if (GUI.Button (brect, "View Source")) 
		{	
			if (gameObject.GetComponent<SourceCodePopup>() == null)
			{
				sourcePopup = gameObject.AddComponent<SourceCodePopup>(); 		
				sourcePopup.URL = "https://github.com/uWebKit/uWebKit/blob/uWebKit2-Beta/uWebKit/Assets/uWebKitExamples/Scripts/WebBrowser.cs";
				//webGUI.HasFocus = false;
			}
			else
			{
				gameObject.SendMessage("SourcePopupClosed");
			}
		}		


#if ENABLE_DEV_BUTTONS

		brect.y += 50;

		if (GUI.Button (brect, "Crash Unity Process")) 
		{			
			UWKPlugin.UWK_DevelopmentOnlyCrashProcess();
		}

		brect.y += 50;

		if (GUI.Button (brect, "Hang Unity Process")) 
		{			
			for (uint i = 0; i < 1;)
			{

			}
		}

		brect.y += 50;

		if (GUI.Button (brect, "Crash Web Process")) 
		{		
			UWKPlugin.UWK_DevelopmentOnlyCrashWebProcess();	
		}


		brect.y += 50;

		if (GUI.Button (brect, "Hang Web Process")) 
		{		
			UWKPlugin.UWK_DevelopmentOnlyHangWebProcess();	
		}

#endif
		
	}
}