/******************************************
  * uWebKit 
  * (c) 2014 THUNDERBEAST GAMES, LLC
  * http://www.uwebkit.com
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Simple menu for WebGUI Example
/// </summary>
public class WebGUIExample : MonoBehaviour
{
	WebGUI webGUI;
	UWKWebView UWKWebView;

	SourceCodePopup sourcePopup;

	// Use this for initialization
	void Start ()
	{		
		webGUI = gameObject.GetComponent<WebGUI>();
		UWKWebView = gameObject.GetComponent<UWKWebView>();
		
		webGUI.Position.x = Screen.width / 2 - UWKWebView.MaxWidth / 2;
		webGUI.Position.y = 0;

	}

	void SourcePopupClosed()
	{
		UnityEngine.Object.Destroy(gameObject.GetComponent<SourceCodePopup>());		
		webGUI.HasFocus = true;
	}

	void OnGUI ()
	{
		Rect brect = new Rect (0, 0, 120, 40);

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
				sourcePopup.URL = "https://github.com/uWebKit/uWebKit/blob/uWebKit2-Beta/uWebKit/Assets/uWebKitExamples/Scripts/WebGUI.cs";
				webGUI.HasFocus = false;
			}
			else
			{
				gameObject.SendMessage("SourcePopupClosed");
			}
		}		
	}
	
}