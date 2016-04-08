/******************************************
  * uWebKit 
  * (c) 2014 THUNDERBEAST GAMES, LLC
  * http://www.uwebkit.com
  * sales@uwebkit.com
*******************************************/

using System;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Example of a web page generated in Unity, using the Javascript Bridge
/// </summary>
public class UnityInfoPage
{

	public static string GetHTML(UWKWebView view)
	{
		view.WebQuery = null;
        view.WebQuery += onWebQuery;            
        return HTML;
	}

	private static void onWebQuery (UWKWebQuery query)
	{
		var request = UWKJson.Deserialize (query.Request) as Dictionary<string,object>;

		var message = request ["message"] as string;

		if (message == "ButtonClicked") {

			var payload = UWKJson.Serialize(request ["payload"] as Dictionary<string,object>);

			query.Success ("Query Response from Unity: Success!");

			#if UNITY_EDITOR
			EditorUtility.DisplayDialog ("Hello!", "Button clicked, value passed:\n" + payload, "OK!" );
			#endif
		}
	}


	private static string HTML = "";

	// Generate the page HTML
	static UnityInfoPage ()
	{
		
		string[] props = new string[] { "platform", "unityVersion", "systemLanguage", "runInBackground", "isEditor", "dataPath", "persistentDataPath" };
		
		StringBuilder sb = new StringBuilder ();
		
		// Some nice CSS
		sb.Append (@"<html> <head>
		<title>Unity Info Page</title>
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
		<body>
		<script type='text/javascript'>

		  function sendWebQuery(msgType, msgJSON) {

		      const data = {
		          message: msgType,
		          payload: msgJSON
		      };

		      window.atomicQuery({
		          request: JSON.stringify(data),
		          persistent: false,
		          onSuccess: function(response) { 
					  console.log(response);
				  },
		          onFailure: function(error_code, error_message) {
		              console.log(""Error sending web query"");
		          }
		      });
		  }
</script>
");
		
		sb.Append ("<h1>uWebKit Unity Info Page</h1>");

		sb.Append ("<input type='button' value=\"Hello\" onclick='sendWebQuery(\"ButtonClicked\", { value1: 1, value2: \"Testing123\", value3: \"45678\"})' />");

		sb.Append (@"<table border=""1"">");
		
		foreach (string p in props) {
			sb.AppendFormat (@"
			<tr>
			<td>Unity.{0}</td>
			<td id = Unity_{0}></td>
			</tr>", p);
		}
		
		sb.Append ("</table>");
		
		sb.Append ("<script type='text/javascript'>\n");

		foreach (string p in props) {
			sb.AppendFormat ("document.getElementById('Unity_{0}').innerText = Unity.{0};\n", p);
		}
		
		sb.Append ("</script>\n");	
		
		sb.AppendFormat ("<h4>This page generated in Unity on {0}</h4>", DateTime.UtcNow.ToLocalTime ());
		
		sb.Append ("</body> </html>");
		
		HTML = sb.ToString ();
		
	}

	static bool props = false;

	public static void SetProperties ()
	{		
		if (props)
			return;
		
		props = true;
				
		// Export a bunch of unity variables to JavaScript properties which can then 
		// be accessed on pages	
		UWKWebView.SetGlobalProperty ("Unity", "platform", Application.platform.ToString ());
		UWKWebView.SetGlobalProperty ("Unity", "unityVersion", Application.unityVersion);
		UWKWebView.SetGlobalProperty ("Unity", "systemLanguage", Application.systemLanguage.ToString ());
		UWKWebView.SetGlobalProperty ("Unity", "runInBackground", Application.runInBackground);
		UWKWebView.SetGlobalProperty ("Unity", "isEditor", Application.isEditor);
		UWKWebView.SetGlobalProperty ("Unity", "dataPath", Application.dataPath);
		UWKWebView.SetGlobalProperty ("Unity", "persistentDataPath", Application.persistentDataPath);
	}
}

