using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;

public static class UWKEditorUtils
{

    public static string GetUWebKitInternalPath()
    {
        DirectoryInfo dataPath = new DirectoryInfo(Application.dataPath);
        FileInfo[] fileInfos = dataPath.GetFiles("UWKEditorUtils.cs", SearchOption.AllDirectories ); 

        if (fileInfos == null || fileInfos.Length == 0)
        {
            EditorUtility.DisplayDialog("uWebKit Error", "Unable to locate UWKEditorUtils.cs", "Ok");
            return null;
        }

        if (fileInfos.Length > 1)
        {
            EditorUtility.DisplayDialog("uWebKit Error", "Multiple UWKEditorUtils.cs found, duplicate uWebKit packages in project?", "Ok");
        }

        var info = fileInfos[0];

        var path = info.Directory.Parent.FullName;

        return path;

    }
}
