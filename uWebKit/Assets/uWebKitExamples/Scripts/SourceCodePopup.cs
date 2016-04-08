/******************************************
  * uWebKit 
  * (c) 2014 THUNDERBEAST GAMES, LLC
  * http://www.uwebkit.com
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System.Collections;
using System;

public class SourceCodePopup : MonoBehaviour
{

	public static bool usePopup = false;

	// the view itself
	UWKWebView view;

	// position and dimensions of View
	public float X;
	public float Y;

	public int Width = 1024;
	public int Height = 600;

	// the view area of the browser
	Rect windowRect;
		
	int toolbarHeight = 24;

	public string URL;

	// Get the center position
	public void GetCenterPos (ref Vector2 pos)
	{
		pos.x = Screen.width / 2 - windowRect.width / 2;
		pos.y = Screen.height / 2 - windowRect.height / 2;
	}

	// Center the browser on the screen
	public void Center ()
	{
		Vector2 v = new Vector2 ();
		
		GetCenterPos (ref v);
		
		X = windowRect.x = v.x;
		Y = windowRect.y = v.y;
	}


	// Use this for initialization
	void Start ()
	{		
		windowRect = new Rect (X, Y, Width + 8, Height + 8 + toolbarHeight);

		view = UWKWebView.AddToGameObject(gameObject, URL, Width, Height);

		Center ();		
	}

	// Main Window function of browser, used to draw GUI
	void windowFunction (int windowID)
	{	
		
		if (GUI.Button (new Rect (windowRect.width - 28, 4,24,24), "X"))
		{
			Close();
			return;
		}
		
 		GUI.DragWindow(new Rect(0, 0, Width, toolbarHeight));

 		Rect browserRect = new Rect(4, 4 + toolbarHeight, Width, Height);

 		view.DrawTexture(browserRect);
		
			
	}
	
	void Close ()
	{	
		gameObject.SendMessage("SourcePopupClosed");
	}
	
	void OnGUI ()
	{
				
		GUI.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		
		windowRect = GUI.Window (255, windowRect, windowFunction, "Source Code");

        if (Event.current.type == EventType.Layout)
        {
	        Vector3 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y; 

            mousePos.x -= X;
            mousePos.y -= Y + toolbarHeight + 4;    

			view.ProcessMouse(mousePos);            
        }

	}
	
}