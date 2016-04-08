
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Example of using UWKCore across multiple scene loads (and unloads)
/// Please note to view this sample, the included example scenes (including the ExampleLoader scene) 
/// must be added to the Build Settings 
/// </summary>
public class ExampleLoader : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        if (SceneManager.sceneCountInBuildSettings != 8)
        {
#if UNITY_EDITOR
            //EditorUtility.DisplayDialog("Example Loader", "This example features dynamic scene loading and thus requires the example scenes (including the ExampleLoader scene) be added to the Build Settings", "Ok");
            //EditorApplication.ExecuteMenuItem("Edit/Play");
#endif
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {

        int buttonWidth = 180;

        // get the attached view component
        UWKWebView view = gameObject.GetComponent<UWKWebView>();

        int x = Screen.width / 2 - 1024 / 2 + 84;
        int y = Screen.height / 2 - 720 / 2;

        // draw it
        Rect r = new Rect(x, y, view.Width, view.Height);
        view.DrawTexture(r);

        // get the mouse coordinate
        Vector3 mousePos = Input.mousePosition;
        mousePos.y = Screen.height - mousePos.y;

        // translate based on position
        mousePos.x -= x;
        mousePos.y -= y;

        view.ProcessMouse(mousePos);

        // process keyboard     
        if (Event.current.isKey)
            view.ProcessKeyboard(Event.current);

        x -= (buttonWidth + 32);
        y = Screen.height / 2 - 720 / 2;

        if (y < 0)
            y = 0;

        GUI.BeginGroup(new Rect(x, y, buttonWidth, Screen.height));

        Rect brect = new Rect(0, 0, buttonWidth, 60);
        if (GUI.Button(brect, "Example 1 - Web Browser"))
        {
            SceneManager.LoadScene("Example1WebBrowser");
        }

        brect.y += 80;
        if (GUI.Button(brect, "Example 2 - Web GUI"))
        {
            SceneManager.LoadScene("Example2WebGUI");
        }

        brect.y += 80;
        if (GUI.Button(brect, "Example 3 - Web Texture"))
        {
            SceneManager.LoadScene("Example3WebTexture");
        }

        brect.y += 80;
        if (GUI.Button(brect, "Example 4 - Scene"))
        {
            SceneManager.LoadScene("Example4Scene");
        }

        brect.y += 80;
        if (GUI.Button(brect, "Example 5 - Javascript"))
        {
            SceneManager.LoadScene("Example5Javascript");
        }

        brect.y += 80;
        if (GUI.Button(brect, "Example 6 - WebQuery"))
        {
            SceneManager.LoadScene("Example6WebQuery");
        }

/*

        brect.y += 80;
        if (GUI.Button(brect, "Example 7- Alpha Mask"))
        {
            SceneManager.LoadScene("Example7AlphaMask");
        }

        brect.y += 80;
        if (GUI.Button(brect, "Clear Cookies"))
        {
            // TODO: uWebKit Issue #8
            // UWKCore.ClearCookies();
        }

        */

        brect.y += 80;
        if (GUI.Button(brect, "Quit"))
        {
            Application.Quit();
        }

        GUI.EndGroup();

        if (UWKCore.BetaVersion)
        {
            GUI.Label(new Rect(0, 0, 200, 60), "UWEBKIT BETA VERSION");
        }


    }

}