/******************************************
  * uWebKit 
  * (c) 2014 THUNDERBEAST GAMES, LLC
  * http://www.uwebkit.com
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Basic example of using a UWKWebView on a 3D Unity surface
/// </summary>

// IMPORTANT: Please see the WebGUI.cs example for 2D support

public class WebTexture : MonoBehaviour
{

	#region Inspector Fields
	public bool KeyboardEnabled = true;
	public bool MouseEnabled = true;
	public bool Rotate = false;
	public bool HasFocus = true;
	public bool AlphaMask = false;
	#endregion

	UWKWebView view;

	// Use this for initialization
	void Start ()
	{   

		view = gameObject.GetComponent<UWKWebView>();

		//view.SetAlphaMask(AlphaMask);

		if (GetComponent<Renderer>() != null)
			GetComponent<Renderer>().material.mainTexture = view.WebTexture;

		if (GetComponent<GUITexture>() != null)
			GetComponent<GUITexture>().texture = view.WebTexture;

	}

	// Update is called once per frame
	void Update ()
	{

		if (Rotate)
			gameObject.transform.Rotate (0, Time.deltaTime * 16.0f, 0);

		if (!MouseEnabled || !HasFocus)
			return;         

		RaycastHit rcast;

		if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out rcast)) 
		{

			if (rcast.collider != GetComponent<MeshCollider> ())
				return;

			int x = (int)(rcast.textureCoord.x * (float)view.MaxWidth);
			int y = view.MaxHeight - (int)(rcast.textureCoord.y * (float)view.MaxHeight);

			Vector3 mousePos = new Vector3();
			mousePos.x = x; 
			mousePos.y = y;
			view.ProcessMouse(mousePos);  

		}

	}

	void OnGUI ()
	{       
		if (!KeyboardEnabled || !HasFocus)
			return;

		if (Event.current.isKey)
		{
			//view.ProcessKeyboard(Event.current);
		}

	}

}