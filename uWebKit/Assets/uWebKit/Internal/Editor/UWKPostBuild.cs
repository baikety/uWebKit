using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System;

public class UWKPostBuild {

    [PostProcessBuildAttribute(1)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
        var internalPath = UWKEditorUtils.GetUWebKitInternalPath();

        if (internalPath == null || internalPath.Length == 0)
        {
            Debug.LogError("uWebKit: Error locating Internal folder, package installed?");
            return;
        }

        Debug.Log("uWebKit Deployment: Internal folder found, " + internalPath);

        if (target == BuildTarget.StandaloneWindows64)
        {			
            string sourcePath, dstPath;

            sourcePath = internalPath + "/Editor/Binaries/Windows/x86_64/UWKProcess";

			dstPath = pathToBuiltProject;

			if ( dstPath.EndsWith(".exe"))
				dstPath = dstPath.Replace(".exe", "_Data");
			else if (!dstPath.EndsWith("_Data"))
				dstPath += "_Data";
				            
            FileUtil.CopyFileOrDirectory(sourcePath, dstPath + "/UWKProcess");
        }
        else if (target == BuildTarget.StandaloneWindows)
        {
            string sourcePath, dstPath;

            sourcePath = internalPath + "/Editor/Binaries/Windows/x86/UWKProcess";

			dstPath = pathToBuiltProject;

			if ( dstPath.EndsWith(".exe"))
				dstPath = dstPath.Replace(".exe", "_Data");
			else if (!dstPath.EndsWith("_Data"))
				dstPath += "_Data";
			
            FileUtil.CopyFileOrDirectory(sourcePath, dstPath + "/UWKProcess");

            string[] files = Directory.GetFiles(dstPath + "/UWKProcess");

            foreach (var filename in files)
            {
                if (filename.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (filename.Contains(".dll_x86"))
                {
                    var nfilename = filename.Replace(".dll_x86", ".dll");

                    if (File.Exists(nfilename))
                    {
                        File.Delete(nfilename);
                    }

                    File.Move(filename, nfilename);
                }
            }

        }
		else if (target == BuildTarget.StandaloneOSXIntel || target == BuildTarget.StandaloneOSXUniversal)
		{
			EditorUtility.DisplayDialog("uWebKit Deployment", "uWebKit3 supports 64 bit Mac deployment.  It does not support 32 bit builds, please switch to x86_64 Architecture in Build Settings dialog\n", "Ok");			
		}
        else if (target == BuildTarget.StandaloneOSXIntel64)
		{

			if (!pathToBuiltProject.EndsWith (".app"))
				pathToBuiltProject += ".app";
					
			// TODO: Hopefully CEF3 fixes this, can't have the framework/helpers in a sub app bundle
			// has to be in main bundle

			string[] frameworks = new string[] { "Chromium Embedded Framework.framework"};
			string[] apps = new string[] { "UWKProcess Helper", "UWKProcess Helper EH", "UWKProcess Helper NP"};

			string sourcePath, dstPath;

			foreach (var framework in frameworks)
			{
				sourcePath = internalPath + "/Editor/Binaries/Mac/x86_64/UWKProcess.app/Contents/Frameworks/" + framework;
				dstPath = pathToBuiltProject + "/Contents/Frameworks/" + framework;
				FileUtil.CopyFileOrDirectory (sourcePath, dstPath);				
			}

			foreach (var app in apps)
			{
				sourcePath = internalPath + "/Editor/Binaries/Mac/x86_64/UWKProcess.app/Contents/Frameworks/" + app + ".app";
				dstPath = pathToBuiltProject + "/Contents/Frameworks/" + app + ".app";
				FileUtil.CopyFileOrDirectory (sourcePath, dstPath);		
			}

			var uwkProcesssDir = pathToBuiltProject + "/Contents/UWKProcess/";

			if (!Directory.Exists (uwkProcesssDir))
				Directory.CreateDirectory (uwkProcesssDir);

            sourcePath = internalPath + "/Editor/Binaries/Mac/x86_64/UWKProcess.app/Contents/MacOS/UWKProcess";
			dstPath = uwkProcesssDir + "UWKProcess";

			FileUtil.CopyFileOrDirectory (sourcePath, dstPath);				

		}
	}

}
