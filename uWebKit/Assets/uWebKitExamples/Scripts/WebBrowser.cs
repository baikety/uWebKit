/******************************************
  * uWebKit
  * (c) 2014 THUNDERBEAST GAMES, LLC
  * http://www.uwebkit.com
  * sales@uwebkit.com
*******************************************/

using UnityEngine;

class WebBrowser : MonoBehaviour
{

	// position and dimensions of browser
	public int X = 0;
	public int Y = 0;

	public int Width = 1024;
	public int Height = 600;

	public bool DynamicWidth = true;

	// The browser skin being used
	public GUISkin Skin = null;

	void Start ()
	{	
		if (DynamicWidth) {
			Width = Screen.width - 256;
			// mask to 16 bit boundry
			Width = (int)((uint)Width & 0xfffffff0);

			Height = Screen.height - 192;
			// mask to 16 bit boundry
			Height = (int)((uint)Height & 0xfffffff0);

			Width = Width < 1024 ? 1024 : Width;
			Width = Width > 2048 ? 2048 : Width;

			Height = Height < 512 ? 512 : Height;
			Height = Height > 2048 ? 2048 : Height;
		}

		// generate some textures
		Color c = new Color (0, 1, 0, .2f);
		texProgress = new Texture2D (32, 32);
		for (int x = 0; x < 32; x++)
			for (int y = 0; y < 32; y++)
				texProgress.SetPixel (x, y, c);

		c = new Color (1, 0, 0, .05f);
		texCloseTab = new Texture2D (16, 16);
		for (int x = 0; x < 16; x++)
			for (int y = 0; y < 16; y++)
				texCloseTab.SetPixel (x, y, c);

		c = new Color (1, 0, 0, 1.0f);
		texCloseTabHover = new Texture2D (16, 16);
		for (int x = 0; x < 16; x++)
			for (int y = 0; y < 16; y++)
				texCloseTabHover.SetPixel (x, y, c);

		texProgress.Apply ();
		texCloseTab.Apply ();
		texCloseTabHover.Apply ();

		windowRect = new Rect (X, Y, Width + 8, Height + 138);

		// Create our view
		createTab ();
		setActiveTab (0);

	}

	int findTab (UWKWebView view)
	{
		for (int i = 0; i < maxTabs; i++)
			if (tabs [i].View == view)
				return i;

		return -1;
	}

	// Delegate called when a tab's URL has changed (redirects, etc)
	void urlChanged (UWKWebView view, string url)
	{
		int i = findTab (view);

		if (i >= 0)
			tabs [i].URL = url;

		if (i == activeTab)
			currentURL = url;

	}

	void newViewRequested (UWKWebView view, string url)
	{
		int i = findTab (view);

		if (i >= 0) {
			if (numTabs >= maxTabs) {
				// load into current view
				view.LoadURL (url);
			} else {
				createTab (url);
				setActiveTab (numTabs - 1);
			}
		}

	}

	// Delegate called when a tab's (HTML) title has changed
	void titleChanged (UWKWebView view, string title)
	{
		int i = findTab (view);

		if (i >= 0)
			tabs [i].Title = title;

	}

	// Delegate called when a tab's page is loaded
	void loadFinished (UWKWebView view)
	{
		// ensure 100%
		pageLoadProgress = 100;

	}

	void loadProgress (UWKWebView view, int progress)
	{
		pageLoadProgress = progress;
	}


	// Create a new tab (requires Pro for > 1 UWKWebView)
	void createTab (string url = "http://www.google.com")
	{
		if (numTabs == 4)
			return;

		Tab t = tabs [numTabs];

		t.URL = url;

		t.View = UWKWebView.AddToGameObject (gameObject, url, Width, Height);

		// TODO: uWebKit3

		t.View.URLChanged += urlChanged;
		t.View.TitleChanged += titleChanged;

		/*
		t.View.LoadProgress += loadProgress;
		t.View.LoadFinished += loadFinished;
		*/

		// clear default handler
		//TODO: uWebKit3
		//t.View.NewViewRequested = null;
		//t.View.NewViewRequested += newViewRequested;

		t.Title = "";

		tabs [numTabs] = t;

		numTabs++;

	}

	void closeTab (int index)
	{
		if (numTabs == 0 || index >= numTabs)
			return;

		bool settab = false;
		UWKWebView aview = null;
		if (index == activeTab)
			settab = true;
		else
			aview = tabs [activeTab].View;


		UnityEngine.Object.Destroy (tabs [index].View);
		tabs [index].View = null;

		int j = 0;
		for (int i = 0; i < numTabs; i++) {
			if (i == index)
				continue;

			tabs [j++] = tabs [i];
		}

		numTabs--;

		if (numTabs > 0) {
			if (settab)
				setActiveTab (0);
			else
				setActiveTab (findTab (aview));
		}

	}

	// Sets the currently active tab
	void setActiveTab (int idx)
	{
		activeTab = idx;

		for (int i = 0; i < numTabs; i++) {
			if (i != idx) {
				tabs [i].View.Hide ();
			} else {
				currentURL = tabs [i].URL;
				tabs [i].View.Show ();
			}
		}
	}

	void OnGUI ()
	{

		GUI.skin = null;

		windowRect = GUILayout.Window (unityWindowId, windowRect, windowFunction, "");

		GUI.skin = null;

		UWKWebView view = tabs [activeTab].View;

		if (view != null) {

			if (Event.current.type == EventType.Layout) {
				Vector3 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;

				if (windowRect.Contains (mousePos)) {
					mousePos.x -= windowRect.x;
					mousePos.y -= windowRect.y;

					if (browserRect.Contains (mousePos)) {
						mousePos.x -= browserRect.x;
						mousePos.y -= browserRect.y;

						view.ProcessMouse (mousePos);
					}
				}
			}

			if (Event.current.isKey) {
				
				view.ProcessKeyboard (Event.current);

				if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
					Event.current.Use ();

			}
		}
	}

	void checkTabs ()
	{
		if (numTabs == 0) {
			createTab ();
			setActiveTab (0);
		}

	}

	void guiTabs (ref GUIStyle buttonStyle)
	{
		UWKWebView view = null;

		if (numTabs != 0)
			view = tabs [activeTab].View;

		GUILayout.BeginHorizontal ();

		buttonStyle.normal.background = null;
		buttonStyle.active.background = null;
		buttonStyle.normal.textColor = new Color (.65f, .65f, .65f, 1.0f);
		buttonStyle.hover.textColor = new Color (.35f, .35f, .35f, 1.0f);

		GUILayoutOption width = GUILayout.MaxWidth (128);

		// Bookmark Buttons

		if (GUILayout.Button ("uWebKit", buttonStyle, width)) {
			checkTabs ();
			view = tabs [activeTab].View;
			view.LoadURL ("http://www.uwebkit.com");
		}

		if (GUILayout.Button ("uWebKit GitHub", buttonStyle, width)) {
			checkTabs ();
			view = tabs [activeTab].View;
			view.LoadURL ("https://github.com/uWebKit/uWebKit");
		}

		if (GUILayout.Button ("Google", buttonStyle, width)) {
			checkTabs ();
			view = tabs [activeTab].View;
			view.LoadURL ("https://www.google.com");
		}


		if (GUILayout.Button ("YouTube", buttonStyle, width)) {
			checkTabs ();
			view = tabs [activeTab].View;
			view.LoadURL ("https://www.youtube.com/embed/8lWpnvNxs8k");
		}

		if (GUILayout.Button ("Unity3D", buttonStyle, width)) {
			checkTabs ();
			view = tabs [activeTab].View;
			view.LoadURL ("https://www.unity3d.com");
		}

		if (GUILayout.Button ("HTC Vive", buttonStyle, width)) {
			checkTabs ();
			view = tabs [activeTab].View;
			view.LoadURL ("http://store.steampowered.com/app/358040/");
		}

		if (GUILayout.Button ("Google Maps", buttonStyle, width)) {
			checkTabs ();
			view = tabs [activeTab].View;
			view.LoadURL ("https://maps.google.com");
		}

		//TODO: uWebKit3
		if (UWKCore.IMEEnabled) {
			if (GUILayout.Button ("WoW China", buttonStyle, width)) {
				checkTabs ();
				view = tabs [activeTab].View;
				view.LoadURL ("http://www.battlenet.com.cn/wow/zh");
			}
		} else {
			if (GUILayout.Button ("Penny Arcade", buttonStyle, width)) {
				checkTabs ();
				view = tabs [activeTab].View;
				view.LoadURL ("http://www.penny-arcade.com");
			}
		}

		if (GUILayout.Button ("Unity Info", buttonStyle, width)) {
			checkTabs ();
			view = tabs [activeTab].View;
			UnityInfoPage.SetProperties ();
			view.LoadHTML (UnityInfoPage.GetHTML (view), "http://browser/");
		}

		GUILayout.EndHorizontal ();

		// tabs
		GUILayout.BeginHorizontal ();


		// setup style for close widget
		GUIStyle closeStyle = new GUIStyle (buttonStyle);
		closeStyle.hover.background = texCloseTabHover;
		closeStyle.normal.background = texCloseTab;
		closeStyle.fontSize = 8;
		closeStyle.fixedWidth = 12;
		closeStyle.fixedHeight = 12;
		closeStyle.stretchWidth = false;
		closeStyle.stretchHeight = false;

		for (int i = 0; i < 4; i++) {
			if (i < numTabs) {
				string title = tabs [i].Title;

				if (title == null)
					title = "";

				if (title.Length > 24) {
					title = title.Substring (0, 24);
					title += "...";
				}

				if (GUILayout.Button (title, GUILayout.MaxWidth (256))) {
					setActiveTab (i);
				}

				Rect br = GUILayoutUtility.GetLastRect ();

				br.x += 8;
				br.y += 4;

				br.height = 16;
				br.width = 16;

                // TODO: uWebKit3
                // UWKWebView v = tabs[i].View;

                //if (v.WebIcon != null) {
                //GUI.DrawTexture (br, v.WebIcon);
                //}

                br = GUILayoutUtility.GetLastRect ();

				br.x += br.width;
				br.y += 4;
				br.height = 14;
				br.width = 14;

				if (GUI.Button (br, "x", closeStyle)) {
					closeTab (i);
				}

				GUILayout.Space (14);
			}
		}

		if (numTabs < maxTabs) {
			if (GUILayout.Button ("New Tab", GUILayout.MaxWidth (72))) {
				createTab ();
				setActiveTab (numTabs - 1);
			}
		}

		GUILayout.EndHorizontal ();

	}


	// Main Window function of browser, used to draw GUI
	void windowFunction (int windowID)
	{
		GUI.skin = Skin;

		UWKWebView view = null;

		if (numTabs != 0)
			view = tabs [activeTab].View;

		GUIStyle buttonStyle = new GUIStyle (GUI.skin.button);
		buttonStyle.padding = new RectOffset (2, 2, 2, 2);

		GUI.color = new Color (1.0f, 1.0f, 1.0f, transparency);
		browserRect = new Rect (4, 118 + 8, Width, Height);

		Rect headerRect = new Rect (4, 4, Width, 118 + 4);
		GUI.DrawTexture (headerRect, texHeader);

		int titleHeight = 24;
		Rect titleRect = new Rect (0, 0, Width, titleHeight);

		GUI.DragWindow (titleRect);

		GUILayout.BeginVertical ();
		// Main Vertical
		GUILayout.BeginArea (new Rect (8, 4, Width, 118));

		GUILayout.BeginHorizontal ();

		GUILayout.BeginVertical ();

		// title
		Texture2D bxTex = GUI.skin.box.normal.background;
		GUI.skin.box.normal.background = null;
		GUI.skin.box.normal.textColor = new Color (.25f, .25f, .25f, 1.0f);

		//TODO: uWebKit3
		GUILayout.Box (view == null ? "" : view.Title);
		GUI.skin.box.normal.background = bxTex;

		GUILayout.BeginHorizontal ();

		GUI.enabled = view != null && view.CanGoBack;

		if (GUILayout.Button (texBack, buttonStyle, GUILayout.Width (texBack.width), GUILayout.Height (texBack.height))) {

			if (view != null)
				view.Back ();

		}

		GUI.enabled = view != null && view.CanGoForward;

		if (GUILayout.Button (texForward, buttonStyle, GUILayout.Width (texForward.width), GUILayout.Height (texForward.height))) {

			if (view != null)
				view.Forward ();
		}

		GUI.enabled = view != null && !view.Loading;

		if (GUILayout.Button (texReload, buttonStyle, GUILayout.Width (texReload.width), GUILayout.Height (texReload.height))) {
			if (view != null)
				view.LoadURL (currentURL);
		}

		GUI.enabled = true;


		bool nav = false;
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
			nav = true;

		GUI.SetNextControlName ("BrowserURL");

		currentURL = GUILayout.TextField (currentURL, GUILayout.MaxWidth (Width - 196));

		// this is grey for some reason
		if (pageLoadProgress != 100) {
			Rect urlRect = GUILayoutUtility.GetLastRect ();
			urlRect.width *= (float)pageLoadProgress / 100.0f;
			GUI.DrawTexture (urlRect, texProgress);
		}

		if (nav && GUI.GetNameOfFocusedControl () == "BrowserURL") {
			GUIUtility.keyboardControl = 0;
			if (view != null) {

				string URL = currentURL.Replace (" ", "%20");

				if (!URL.Contains ("."))
					URL = "http://www.google.com/search?q=" + URL;

				if (!URL.Contains ("://"))
					URL = "http://" + URL;

				currentURL = URL;
				view.LoadURL (URL);
			}
		}

		GUILayout.EndHorizontal ();

		guiTabs (ref buttonStyle);

		GUILayout.EndVertical ();
		buttonStyle.normal.background = null;

		buttonStyle.normal.background = null;
		buttonStyle.hover.background = null;
		buttonStyle.active.background = null;
		buttonStyle.padding = new RectOffset (0, 0, 0, 0);

		if (GUILayout.Button (texLogo, buttonStyle, GUILayout.Width (84), GUILayout.Height (100))) {

		}

		GUILayout.EndHorizontal ();

		GUILayout.EndArea ();

		GUILayout.EndVertical ();

		// End Main Vertical

		if (view != null) {

            view.DrawTexture(browserRect, false);            
			//TODO: uWebKit3
			//view.DrawTextIME ((int)browserRect.x, (int)browserRect.y);
		}


		Rect footerRect = new Rect (4, browserRect.yMax, Width, 8);
		GUI.DrawTexture (footerRect, texFooter);

	}

	// the rect of the browser as a whole
	Rect windowRect;
	// the view area of the browser
	Rect browserRect;

	string currentURL = "http://www.google.com";
	int pageLoadProgress = 100;

	float transparency = 1.0f;
	int unityWindowId = 1;

	int numTabs = 0;
	const int maxTabs = 4;
	// the tabs (up to 4)
	Tab[] tabs = new Tab[4];
	int activeTab = 0;

	// A Browser Tab
	struct Tab
	{
		// A unique Id for this tab
		public uint ID;

		// the view itself
		public UWKWebView View;

		// the tab's currenly loaded URL
		public string URL;

		// the tab's title (from HTML)
		public string Title;

	}


	// GUI Textures
	public Texture2D texHeader = null;
	public Texture2D texFooter = null;
	public Texture2D texBack = null;
	public Texture2D texForward = null;
	public Texture2D texReload = null;
	public Texture2D texLogo = null;

	Texture2D texProgress;
	Texture2D texCloseTab;
	Texture2D texCloseTabHover;


}
