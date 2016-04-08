
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Internal Web Core Component
/// </summary>
public class UWKCore : MonoBehaviour
{

    /// <summary>
    /// Main initialization of web core
    /// </summary>
    static public bool Init()
    {
        // if we're already initialized, return immediately
        if (sInstance != null)
            return true;

        if (initFailed)
            return false;

        // we want to run in the background
        Application.runInBackground = true;

        // check for IME and enable it if necessary
        // string lang = Application.systemLanguage.ToString();

        // if (lang == "Chinese" || lang == "Japanese" || lang == "Korean")
        //    IMEEnabled = true;

        // initialize the native plugin
        var success = UWKPlugin.Initialize();

        if (!success)
        {
            initFailed = true;
            return false;
        }

        // add ourselves to a new game object
        GameObject go = new GameObject ("UWKCore");
        UnityEngine.Object.DontDestroyOnLoad (go);
        UWKCore core = go.AddComponent<UWKCore> ();

        // we're all ready to go
        sInstance = core;

        return true;
    }

    void Update()
    {
        UWKPlugin.Update();
    }

	IEnumerator Start()
	{
		yield return StartCoroutine("CallPluginAtEndOfFrames");
	}

	private IEnumerator CallPluginAtEndOfFrames()
	{
		while (true) {

			yield return new WaitForEndOfFrame();
			GL.IssuePluginEvent(UWKPlugin.UWK_GetRenderEventFunc(), 1);
		}
	}


    void OnDestroy()
    {
        // Core is coming down, close up shop and clean up
        UWKPlugin.Shutdown();
        sInstance = null;
    }

	/// <summary>
	/// Internal View Creation
	/// </summary>
	public static uint CreateView(UWKWebView view, int width, int height, int maxWidth, int maxHeight, string url, IntPtr nativeTexture)
	{
		uint id = UWKPlugin.UWK_CreateView(width, height, maxWidth, maxHeight, url, nativeTexture);
		viewLookup[id] = view;
		return id;
	}

	/// <summary>
	/// Internal View Destruction
	/// </summary>
	public static void DestroyView(UWKWebView view)
	{
		if (viewLookup.ContainsKey(view.ID))
			viewLookup.Remove(view.ID);

		UWKPlugin.UWK_DestroyView(view.ID);
	}


	public static void ProcessWebViewEvent(UWKEvent e)
	{
		uint id = e.GetUInt ("ID");

		UWKWebView webView;

		if (viewLookup.TryGetValue (id, out webView)) {

			webView.ProcessUWKEvent (e);
		}
		
	}

    public static bool BetaVersion = false;
    public static bool IMEEnabled = false;

    static Dictionary<uint, UWKWebView> viewLookup = new Dictionary<uint, UWKWebView>();
    static UWKCore sInstance = null;
    static bool initFailed = false;

  }
