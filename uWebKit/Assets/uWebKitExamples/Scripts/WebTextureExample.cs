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
/// Simple menu for WebTexture Example
/// </summary>
public class WebTextureExample : MonoBehaviour
{
	WebTexture webTexture;

	void Start()
	{
		webTexture = GameObject.FindObjectOfType (typeof(WebTexture)) as WebTexture;
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
				sourcePopup.URL = "https://github.com/uWebKit/uWebKit/blob/uWebKit2-Beta/uWebKit/Assets/uWebKitExamples/Scripts/WebTexture.cs";
				webTexture.HasFocus = false;
			}
			else
			{
				gameObject.SendMessage("SourcePopupClosed");
			}
		}		

	}

	void SourcePopupClosed()
	{
		UnityEngine.Object.Destroy(gameObject.GetComponent<SourceCodePopup>());		
		webTexture.HasFocus = true;
	}

	SourceCodePopup sourcePopup;

	
}