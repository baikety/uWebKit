using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/// <summary>
/// JSEvalDelegate - Event fired when return value of some evaluated Javascript is returned
/// </summary>
public delegate void JSEvalDelegate(bool success, string value);

/// <summary>
/// URLChangedDelegate - Event fired when the URL has been changed
/// either by user input or due to a page redirect
/// </summary>
public delegate void URLChangedDelegate(UWKWebView view, string url);

/// <summary>
///  TitleChangedDelegate - Event fired when the title of a web page has changed
/// </summary>
public delegate void TitleChangedDelegate(UWKWebView view, string title);

/// <summary>
/// LoadFinishedDelegate - Event fired once the page has been fully loaded
/// </summary>
public delegate void LoadFinishedDelegate(UWKWebView view);

/// <summary>
/// LoadStateChangedDelegate
/// </summary>
public delegate void LoadStateChangedDelegate(UWKWebView view, bool loading, bool canGoBack, bool canGoForward);

/// <summary>
/// PopupRequestDelegate - Event fired if the page requests a popup window
/// </summary>
public delegate void PopupRequestDelegate(UWKWebView view, string url);


public class UWKWebQuery
{
    public UWKWebView View;
    public double QueryID;
    public string Request;

    public void Success(string result)
    {
        UWKPlugin.UWK_MsgWebMessageResponse(View.ID, QueryID, true, result);
    }

    public void Failure(int errCode, string result)
    {
        UWKPlugin.UWK_MsgWebMessageResponse(View.ID, QueryID, false, result);
    }
}

public delegate void UWKWebQueryDelegate(UWKWebQuery query);

/// <summary>
/// WebView Component
/// </summary>
public class UWKWebView : MonoBehaviour
{
    #region Inspector Fields

    /// <summary>
    /// The initial URL to load
    /// </summary>
    public string URL;

    /// <summary>
    /// Gets the current width of the UWKWebView
    /// </summary>
    public int InitialWidth = 1024;

    /// <summary>
    /// Gets the current height of the UWKWebView
    /// </summary>
    public int InitialHeight = 1024;

    /// <summary>
    /// Max width of the UWKWebView, defined at creation time.  It is possible to set the
    /// view's current width to be equal or smaller than this value
    /// </summary>
    public int MaxWidth = 1024;

    /// <summary>
    /// Max height of the UWKWebView, defined at creation time.  It is possible to set the
    /// view's current height to be equal or smaller than this value
    /// </summary>
    public int MaxHeight = 1024;

    /// <summary>
    /// Used to make the scroll wheel/trackpad more sensitive
    /// </summary>
    public float ScrollSensitivity = 1.0f;

    [HideInInspector]
    public bool CanGoForward = false;

    [HideInInspector]
    public bool CanGoBack = false;

    [HideInInspector]
    public bool Loading = false;

    #endregion

    /// <summary>
    /// Navigation for going backwards and forwards through the UWKWebView's history
    /// view's current height to be equal or smaller than this value
    /// </summary>
    public enum Navigation
    {
        Forward = 0,
        Back
    }

    /// <summary>
    /// The title of the current loaded page
    /// </summary>
    [HideInInspector]
    public string Title = "";

    /// <summary>
    /// Delegate fired when the page has finished loaded
    /// </summary>
    public LoadFinishedDelegate LoadFinished;

    /// <summary>
    /// Delegate fired when the URL of the page has changed
    /// </summary>
    public URLChangedDelegate URLChanged;

    /// <summary>
    /// Delegate fired when the Title of the page has changed
    /// </summary>
    public TitleChangedDelegate TitleChanged;

    public UWKWebQueryDelegate WebQuery;

    public LoadStateChangedDelegate LoadStateChanged;

    public PopupRequestDelegate PopupRequested;

    /// <summary>
    /// Texture2D which is used for the page's contents
    /// </summary>
    //[HideInInspector]
    public Texture2D WebTexture;

    /// <summary>
    /// A unique ID for this UWKWebView
    /// </summary>
    [HideInInspector]
    public uint ID;

    /// <summary>
    /// Dynamically adds a UWKWebView component to a GameObject with initialization that isn't possible using GameObject.AddComponent due
    /// to lack of constructor parameters
    /// </summary>
    public static UWKWebView AddToGameObject(GameObject gameObject, string url = "", int initialWidth = 1024, int initialHeight = 1024, int maxWidth = 1024, int maxHeight = 1024)
    {
        // setup some construction parameters used in Awake method
        createMode = true;
        createURL = url;
        createInitialWidth = initialWidth;
        createInitialHeight = initialHeight;
        createMaxWidth = maxWidth;
        createMaxHeight = maxHeight;

        // create the view component
        UWKWebView view = gameObject.AddComponent<UWKWebView>();

        // no longer in create mode
        createMode = false;

        return view;
    }

    void Awake()
    {
        // ensure core is up
        if (!UWKCore.Init())
        {
            return;
        }

        if (createMode)
        {
            URL = createURL;
            MaxWidth = createMaxWidth;
            MaxHeight = createMaxHeight;
            InitialWidth = createInitialWidth;
            InitialHeight = createInitialHeight;
        }

        if (InitialWidth <= 0)
            InitialWidth = MaxWidth;

        if (InitialHeight <= 0)
            InitialHeight = MaxHeight;

        InitialWidth = Mathf.Clamp(InitialWidth, 64, 4096);
        InitialHeight = Mathf.Clamp(InitialHeight, 64, 4096);
        MaxWidth = Mathf.Clamp(MaxWidth, 64, 4096);
        MaxHeight = Mathf.Clamp(MaxHeight, 64, 4096);

        if (InitialWidth > MaxWidth)
            MaxWidth = InitialWidth;

        if (InitialHeight > MaxHeight)
            MaxHeight = InitialHeight;

        TextureFormat format = TextureFormat.ARGB32;

        if (SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D 11") != -1)
        {
            format = TextureFormat.BGRA32;
        }

        // note that on Direct3D11 shared gpu textures, mipmapping is not allowed
        WebTexture = new Texture2D(MaxWidth, MaxHeight, format, false);

        Color32[] colors = new Color32[MaxWidth * MaxHeight];

        for (int i = 0; i < MaxWidth * MaxHeight; i++)
        {
            colors[i].r = colors[i].g = colors[i].b = colors[i].a = 0;
        }

        WebTexture.SetPixels32(colors);
        WebTexture.Apply();

        width = InitialWidth;
        height = InitialHeight;

        ID = UWKCore.CreateView(this, InitialWidth, InitialHeight, MaxWidth, MaxHeight, URL, WebTexture.GetNativeTexturePtr());

        // default delegate handlers
        LoadFinished += loadFinished;
        URLChanged += urlChanged;
        TitleChanged += titleChanged;
        LoadStateChanged += loadStateChanged;
        PopupRequested += popupRequested;

        WebQuery += webQuery;

    }

    public void DrawTexture(Rect position, bool alphaBlend = true)
    {
		float th = (float)MaxHeight;

		Rect sourceRect = new Rect(0, 0, Width, Height);

		sourceRect.x = 0 ;

#if !UNITY_EDITOR_OSX && UNITY_STANDALONE_WIN
        sourceRect.y = 0.0f;
#else
        sourceRect.y = 1.0f  -  ( sourceRect.height / th ) ;
#endif

        sourceRect.width = sourceRect.width / (float)MaxWidth;
        sourceRect.height = sourceRect.height / th ;

        GUI.DrawTextureWithTexCoords ( position , WebTexture , sourceRect ,  alphaBlend );
    }

    /// <summary>
    /// Set the size of view, must be >= 64 and < max width and height
    /// </summary>
    public void SetSize(int nwidth, int nheight)
    {
        width = Mathf.Clamp(nwidth, 64, MaxWidth);
        height = Mathf.Clamp(nheight, 64, MaxHeight);

        UWKPlugin.UWK_MsgSetSize(ID, width, height);
    }

    /// <summary>
    /// Navigate the view to the specified URL (http://, file://, etc)
    /// </summary>
    public void LoadURL(string url)
    {

        if (url == null || url.Length == 0)
            return;

        UWKPlugin.UWK_MsgLoadURL(ID, url);
    }

    public void LoadHTML(string html, string url = "http://localcontent/")
    {

        if (html == null || html.Length == 0)
            return;

        UWKPlugin.UWK_MsgLoadHTML(ID, html, url);
    }

	/// <summary>
	/// Execute Javascript on the page discarding any return value
	/// </summary>
	public void ExecuteJavascript(string javascript)
	{
		UWKPlugin.UWK_MsgExecuteJavaScript(ID, javascript);
	}
		
    /// <summary>
    /// Evaluates Javascript on the page
    /// Example with return value: EvaluateJavascript("document.title", (success, value) => { Debug.Log(value); });
    /// </summary>
    public void EvaluateJavascript(string javascript, JSEvalDelegate callback = null)
    {
		if (callback == null) 
		{
			ExecuteJavascript (javascript);
			return;
		}

        evalCallbacks[evalIDCounter] = callback;
        UWKPlugin.UWK_MsgEvalJavaScript(ID, evalIDCounter, javascript);
        evalIDCounter++;
    }

    /// <summary>
    /// Process the mouse given mousePos coordinates
    /// </summary>
    public void ProcessMouse(Vector3 mousePos)
    {
        if (inputDisabled)
            return;

        //mousePos.y = Screen.height - mousePos.y;

        if ((int)mousePos.x != lastMouseX || (int)mousePos.y != lastMouseY)
        {
            UWKPlugin.UWK_MsgMouseMove(ID, (int)mousePos.x, (int)mousePos.y);
            lastMouseX = (int)mousePos.x;
            lastMouseY = (int)mousePos.y;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f)
        {
#if UNITY_STANDALONE_WIN
            scroll *= 15.0f;
#else
			scroll *= 1.2f;
#endif

            scroll *= ScrollSensitivity;

            UWKPlugin.UWK_MsgMouseScroll(ID, lastMouseX, lastMouseY, scroll);
        }

        for (int i = 0; i < 3; i++)
        {

            if (Input.GetMouseButtonDown(i))
            {
                if (!mouseStates[i])
                {
                    mouseStates[i] = true;
                    UWKPlugin.UWK_MsgMouseButton(ID, (int)mousePos.x, (int)mousePos.y, i, true);
                }
            }

            if (Input.GetMouseButtonUp(i))
            {
                if (mouseStates[i])
                {
                    mouseStates[i] = false;
                    UWKPlugin.UWK_MsgMouseButton(ID, (int)mousePos.x, (int)mousePos.y, i, false);
                }
            }
        }
    }

    /// <summary>
    /// Process a Unity keyboard event
    /// </summary>
    public void ProcessKeyboard(Event keyEvent)
    {

        if (inputDisabled)
            return;

        UnityKeyEvent uevent = new UnityKeyEvent();

        uevent.Type = keyEvent.type == EventType.KeyDown ? 1u : 0u;
        uevent.KeyCode = (uint)keyEvent.keyCode;
        uevent.Character = (uint)keyEvent.character;

        // Do not forward newline
        if (uevent.Character == 10)
            return;

        // Fix mac deployment Unity key handling bug
#if !UNITY_EDITOR
#if UNITY_STANDALONE_OSX
			if (keyEvent.command && (keyEvent.keyCode == KeyCode.V || keyEvent.keyCode == KeyCode.A || keyEvent.keyCode == KeyCode.C))
			{
					if (keyEvent.type != EventType.KeyDown)
							return;

					uevent.Type = 1u;
					uevent.KeyCode = (uint) keyEvent.keyCode;
					uevent.Modifiers |= (uint) UnityKeyModifiers.CommandWin;

					if (keyEvent.keyCode == KeyCode.V)
							uevent.Character = (uint) 'v';
					if (keyEvent.keyCode == KeyCode.A)
							uevent.Character = (uint) 'a';
					if (keyEvent.keyCode == KeyCode.C)
							uevent.Character = (uint) 'c';


					UWKPlugin.UWK_PostUnityKeyEvent(ID, ref uevent);

					uevent.Type = 0u;
					UWKPlugin.UWK_PostUnityKeyEvent(ID, ref uevent);

					return;

			}
#endif
#endif

        // encode modifiers
        uevent.Modifiers = 0;

        if (keyEvent.command)
            uevent.Modifiers |= (uint)UnityKeyModifiers.CommandWin;

        if (keyEvent.alt)
            uevent.Modifiers |= (uint)UnityKeyModifiers.Alt;

        if (keyEvent.control)
            uevent.Modifiers |= (uint)UnityKeyModifiers.Control;

        if (keyEvent.shift)
            uevent.Modifiers |= (uint)UnityKeyModifiers.Shift;

        if (keyEvent.numeric)
            uevent.Modifiers |= (uint)UnityKeyModifiers.Numeric;

        if (keyEvent.functionKey)
            uevent.Modifiers |= (uint)UnityKeyModifiers.FunctionKey;

        if (keyEvent.capsLock)
            uevent.Modifiers |= (uint)UnityKeyModifiers.CapsLock;

        UWKPlugin.UWK_PostUnityKeyEvent(ID, ref uevent);

    }

    /// <summary>
    /// Moves forward in the page history
    /// </summary>
    public void Forward()
    {
        Navigate(Navigation.Forward);
    }

    /// <summary>
    /// Moves back in the page history
    /// </summary>
    public void Back()
    {
        Navigate(Navigation.Back);
    }

    /// <summary>
    /// Navigates forward or back in the page history
    /// </summary>
    public void Navigate(Navigation n)
    {
        UWKPlugin.UWK_MsgNavigate(ID, (int)n);
    }

    public static void SetGlobalProperty(string globalVarName, string propertyName, object value)
    {
        if (value is bool)
        {
            UWKPlugin.UWK_SetGlobalBoolProperty(globalVarName, propertyName, (bool)value);
        }
        else if (value is int)
        {
            UWKPlugin.UWK_SetGlobalNumberProperty(globalVarName, propertyName, (double)((int)value));
        }
        else if (value is float)
        {
            UWKPlugin.UWK_SetGlobalNumberProperty(globalVarName, propertyName, (double)((float)value));
        }
        else if (value is double)
        {
            UWKPlugin.UWK_SetGlobalNumberProperty(globalVarName, propertyName, (double)value);
        }
        else if (value is string)
        {
            UWKPlugin.UWK_SetGlobalStringProperty(globalVarName, propertyName, (string)value);
        }

    }

    public void ProcessUWKEvent(UWKEvent e)
    {
        if (e.eventType == "E_IPCWEBVIEWTITLECHANGE")
        {

            TitleChanged(this, e.GetString("TITLE"));

        }
        else if (e.eventType == "E_IPCWEBVIEWADDRESSCHANGE")
        {

            URLChanged(this, e.GetString("URL"));

        }
        else if (e.eventType == "E_IPCWEBVIEWLOADSTATECHANGE")
        {

            Loading = e.GetBool("LOADING");
            CanGoBack = e.GetBool("CANGOBACK");
            CanGoForward = e.GetBool("CANGOFORWARD");

            LoadStateChanged(this, Loading, CanGoBack, CanGoForward);

        }
        else if (e.eventType == "E_IPCWEBVIEWWEBMESSAGE")
        {

            UWKWebQuery query = new UWKWebQuery();
            query.View = this;
            query.QueryID = e.GetDouble("QUERYID");
            query.Request = e.GetString("REQUEST");

            WebQuery(query);

            //Debug.Log ("IPCWEBVIEWMESSAGE: " + e.GetString ("REQUEST"));
            //UWKPlugin.UWK_MsgWebMessageResponse (ID, e.GetDouble ("QUERYID"), true, "Atomic Game Engine!!!");

        }
        else if (e.eventType == "E_IPCWEBVIEWLOADEND")
        {

            LoadFinished(this);

        }
        else if (e.eventType == "E_IPCWEBVIEWJSEVALRESULT")
        {

            JSEvalDelegate callback;
            if (evalCallbacks.TryGetValue(e.GetUInt("EVALID"), out callback))
            {
                callback(e.GetBool("RESULT"), e.GetString("VALUE"));
            }

        }
        else if (e.eventType == "E_IPCWEBVIEWPOPUPREQUEST")
        {
            PopupRequested(this, e.GetString("URL"));
        }
    }

    /// <summary>
    /// Makes the page visible, the page will be updated and refreshed by the Wweb rendering process
    /// </summary>
    public void Show()
    {
        visible = true;
        //UWKPlugin.UWK_MsgShow(ID, true);
    }

    /// <summary>
    /// Hide the page, the page will no longer be rendered by the web rendering process saving CPU time
    /// </summary>
    public void Hide()
    {
        visible = false;
        //UWKPlugin.UWK_MsgShow(ID, false);
    }

    /// <summary>
    /// Gets whether the page is visible or not
    /// </summary>
    public bool Visible()
    {
        return visible;
    }

    /// <summary
    /// Returns the file:// URL of the applications data path
    /// </summary>
    public static string GetApplicationDataURL()
    {
#if UNITY_STANDALONE_WIN
        return "file:///" + Application.dataPath;
#else
		if (Application.isEditor)
			return "file://" + Application.dataPath;
		else
			return "file://" + Application.dataPath + "/Data";
#endif
    }

    public int Width
    {
        get { return width; }
    }

    public int Height
    {
        get { return height; }
    }


    #region Default delegate handlers

    void loadFinished(UWKWebView view)
    {
    }

    void loadProgress(UWKWebView view, int progress)
    {

    }

    void urlChanged(UWKWebView view, string url)
    {

    }

    void loadStateChanged(UWKWebView view, bool loading, bool canGoBack, bool canGoForward)
    {

    }

    void popupRequested(UWKWebView view, string url)
    {
        // Default handler loads in view
        view.LoadURL(url);
    }

    void titleChanged(UWKWebView view, string title)
    {
        Title = title;
    }

    void webQuery(UWKWebQuery query)
    {

    }

    #endregion

    void OnDestroy()
    {
        UWKCore.DestroyView(this);

        if (WebTexture != null)
        {
            UnityEngine.Object.Destroy(WebTexture);
            WebTexture = null;
        }

    }

    bool visible = true;

    // we need to track mouse states and Unity's OnGUI method method may be called more than once
    bool[] mouseStates = new bool[3] { false, false, false };


    int lastMouseX = -1;
    int lastMouseY = -1;

    int width;
    int height;

    Dictionary<uint, JSEvalDelegate> evalCallbacks = new Dictionary<uint, JSEvalDelegate>();

    static uint evalIDCounter = 1;

    static bool inputDisabled = false;

    // create mode construction paramaters
    static bool createMode = false;
    static int createMaxWidth;
    static int createMaxHeight;
    static int createInitialWidth;
    static int createInitialHeight;
    static string createURL;

}
