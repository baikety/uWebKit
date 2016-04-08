/******************************************
  * uWebKit 
  * (c) 2014 THUNDERBEAST GAMES, LLC
  * http://www.uwebkit.com
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple menu for WebGUI Example
/// </summary>
public class JavascriptExample : MonoBehaviour
{
    WebGUI webGUI;
    UWKWebView view;

    string messageReceived = "";

    bool loaded = false;

    void onLoadFinished(UWKWebView view)
    {
        loaded = true;
    }

    // Use this for initialization
    void Start()
    {
        // Set a global variable, accessible in the webview as 'UWKExample.UnityVersion'
        UWKWebView.SetGlobalProperty("UWKExample", "unityVersion", Application.unityVersion);

        webGUI = gameObject.GetComponent<WebGUI>();
        view = gameObject.GetComponent<UWKWebView>();

        view.LoadFinished += onLoadFinished;
        view.LoadHTML(HTML);

        webGUI.Position.x = Screen.width / 2 - view.MaxWidth / 2;
        webGUI.Position.y = 0;

    }

    void OnGUI()
    {
        Rect brect = new Rect(0, 0, 120, 40);

        if (UWKCore.BetaVersion)
        {
            GUI.Label(new Rect(0, 0, 200, 60), "UWEBKIT BETA VERSION\nCheck http://www.uwebkit.com\nfor updates");
            brect.y += 50;
        }

        if (GUI.Button(brect, "Back"))
        {
            SceneManager.LoadScene("ExampleLoader");
        }

        brect.y += 50;

        if (GUI.Button(brect, "Get Unity Version"))
        {

            if (loaded)
            {
                view.EvaluateJavascript("getUnityVersion();", (success, value) =>
                {

                    messageReceived = "JSEval Result: getUnityVersion() = " + value;

                });
            }

        }

        if (messageReceived.Length != 0)
        {
            brect.y += 50;
            Rect trect = new Rect(brect);
            trect.width += 32;
            trect.height += 32;
            GUI.TextArea(trect, messageReceived);
        }

    }

    private static string HTML = "";

    static JavascriptExample()
    {
        HTML = @"<html>
<head>
<title>uWebKit JavaScript Example</title>
<style type=""text/css"">
	body
	{
		background-color: transparent;
	}
	h1
	{
		color:black;
		text-align:left;
	}
	p
	{
		font-family:""Times New Roman"";
		font-size:20px;
	}
</style>

</head>

<script type='text/javascript'>

function getUnityVersion() {

    return UWKExample.unityVersion;
    
}

</script>

<body>

<h2><center>uWebKit JavaScript Example</center></h2>

</body>

Click ""Get Unity Version"" button on left to evaluate JavaScript in the WebView with return value

</html>
";

    }

}