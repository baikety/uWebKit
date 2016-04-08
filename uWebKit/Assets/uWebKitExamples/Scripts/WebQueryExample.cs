/******************************************
  * uWebKit 
  * (c) 2014 THUNDERBEAST GAMES, LLC
  * http://www.uwebkit.com
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Simple menu for WebGUI Example
/// </summary>
public class WebQueryExample : MonoBehaviour
{
    WebGUI webGUI;
    UWKWebView view;

    string messageReceived = "";

    bool loaded = false;

    void onWebQuery(UWKWebQuery query)
    {
        var request = UWKJson.Deserialize(query.Request) as Dictionary<string, object>;

        var message = request["message"] as string;

        if (message == "UnityMessage")
        {

            var payload = request["payload"] as Dictionary<string, object>;

            var messageCount = (long)payload["messageCount"];

            query.Success("Query Response from Unity: Message Count = " + messageCount);
        }
    }

    void onLoadFinished(UWKWebView view)
    {
        loaded = true;
    }

    // Use this for initialization
    void Start()
    {

        webGUI = gameObject.GetComponent<WebGUI>();
        view = gameObject.GetComponent<UWKWebView>();

        view.LoadFinished += onLoadFinished;
        view.WebQuery += onWebQuery;

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

        if (GUI.Button(brect, "Get Count"))
        {

            if (loaded)
            {
                view.EvaluateJavascript("getMessageCount();", (success, value) =>
                {

                    messageReceived = "JSEval Result: Message Count = " + value;

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

    static WebQueryExample()
    {        
        HTML = @"<html>
<head>
<title>uWebKit WebQuery Example</title>
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

	var messageCount = 0;

  function getMessageCount() {
			return messageCount;
  }

  function sendWebQuery(msgType, msgJSON) {

      const data = {
          message: msgType,
          payload: msgJSON
      };

      window.atomicQuery({
          request: JSON.stringify(data),
          persistent: false,
          onSuccess: function(response) { document.getElementById('Messages').innerText = response; },
          onFailure: function(error_code, error_message) {
              console.log(""Error getting code"");
          }
      });
  }

</script>

<body>

<h2><center>uWebKit WebQuery Example</center></h2>

Send message from Javascript to Unity<br>

<input type='button' value=""Bump Count"" onclick='sendWebQuery(""UnityMessage"", { ""messageCount"": ++messageCount})' /><br>

Javascript message received from Unity<br>
<textarea id='Messages' rows=""4"" cols=""50""></textarea>

</body>

</html>
";

    }

}