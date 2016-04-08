/******************************************
  * uWebKit 
  * (c) 2014 THUNDERBEAST GAMES, LLC
  * http://www.uwebkit.com
  * sales@uwebkit.com
*******************************************/

using UnityEngine;

public class WebGUI : MonoBehaviour
{
    // The position of the gui on the screen
    public Vector2 Position;

    // Whether the GUI accepts mouse/keyboard input
    public bool HasFocus = true;

    void OnGUI()
    {
        // get the attached view component
        UWKWebView view = gameObject.GetComponent<UWKWebView>();

        // if we have a view attached and it is visible
        if (view != null && view.Visible())
        {

            // draw it
            Rect r = new Rect(Position.x, Position.y, view.Width, view.Height);
            view.DrawTexture(r);

            // if we have focus, handle input
            if (HasFocus)
            {
                // get the mouse coordinate
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                // translate based on position
                mousePos.x -= Position.x;
                mousePos.y -= Position.y;

                view.ProcessMouse(mousePos);

                // process keyboard     
                if (Event.current.isKey)
                    view.ProcessKeyboard(Event.current);
            }
        }
    }
}