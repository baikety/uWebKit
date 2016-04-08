using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UWKPlugin
{

    public static bool Initialize()
    {

        Dictionary<string, object> djson = new Dictionary<string, object>();
        Dictionary<string, object> app;

        djson["application"] = app = new Dictionary<string, object>();

        string dataPath = Application.dataPath;

#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
		// Unity 5 bug
		dataPath += "/Resources";
#endif

        app["dataPath"] = dataPath;
        app["persistentDataPath"] = Application.persistentDataPath;
        app["temporaryCachePath"] = Application.temporaryCachePath;
        app["streamingAssetsPath"] = Application.streamingAssetsPath;
        app["isEditor"] = Application.isEditor;
        app["platform"] = Application.platform;
        app["unityVersion"] = Application.unityVersion;
        app["targetFrameRate"] = Application.targetFrameRate;
        app["graphicsDeviceVersion"] = SystemInfo.graphicsDeviceVersion;
        app["hasProLicense"] = Application.HasProLicense();
        //app["imeEnabled"] = UWKCore.IMEEnabled;

        // TODO: Proxy Support
        /*
		app["proxyEnabled"] = UWKConfig.ProxyEnabled;
		app["proxyHostName"] = UWKConfig.ProxyHostname;
		app["proxyUsername"] = UWKConfig.ProxyUsername;
		app["proxyPassword"] = UWKConfig.ProxyPassword;
		app["proxyPort"] = UWKConfig.ProxyPort;
		app["authEnabled"] = UWKConfig.AuthEnabled;
		app["authUsername"] = UWKConfig.AuthUsername;
		app["authPassword"] = UWKConfig.AuthPassword;
		*/

        var json = UWKJson.Serialize(djson);
        var nativeString = NativeUtf8FromString(json);
        var success = UWK_Initialize(log, error, processUWKEvent, beta, nativeString);
        Marshal.FreeHGlobal(nativeString);

        return success;
    }


    public static void Shutdown()
    {
        UWK_Shutdown();
    }

    public static void Update()
    {
        UWK_Update();
    }

    static void log(string message, int level)
    {
        Debug.Log(message);
    }

    static void error(string message, bool fatal)
    {
        Debug.Log(message);
    }

    static void beta(int betaDays)
    {
		string betaMessage = "If you require a non-expiring build without beta watermark, please visit http://uwebkit.com/store/ for purchasing information (15% discount during beta period)";

        UWKCore.BetaVersion = true;
		if (betaDays == -1)
        {
            Debug.LogError("uWebKit Beta Expired");
#if UNITY_EDITOR                
			EditorUtility.DisplayDialog("uWebKit Beta Expired", "This BETA version of uWebKit has expired.\nPlease check http://www.uwebkit.com for a new version.\n\n" + betaMessage, "Ok");
#endif                
        }
        else
        {
            string message = String.Format("There are {0} days left of this expiring uWebKit BETA", betaDays);

            if (betaDays == 0)
                message = String.Format("There is less than a day left of this expiring uWebKit BETA");

            if (!UWK_HasDisplayedBetaMessage())
            {
#if UNITY_EDITOR                
				EditorUtility.DisplayDialog("uWebKit BETA Version", "\n" + message + "\n\n"+ betaMessage, "Ok");
#endif
            }

            Debug.Log(message);

        }
    }

    public static IntPtr NativeUtf8FromString(string managedString)
    {
        int len = Encoding.UTF8.GetByteCount(managedString);
        byte[] buffer = new byte[len + 1];
        Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
        IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
        Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
        return nativeUtf8;
    }

    [DllImport("UWKPlugin")]
    public static extern bool UWK_Initialize(LogCallbackDelegate logcb, LogErrorDelegate errorcb, ProcessUWKEventDelegate processcb, UWKBetaDelegate betacb, IntPtr initJson);

    [DllImport("UWKPlugin")]
    public static extern bool UWK_HasDisplayedBetaMessage();

    [DllImport("UWKPlugin")]
    public static extern void UWK_Update();

    [DllImport("UWKPlugin")]
    public static extern void UWK_Shutdown();

    [DllImport("UWKPlugin")]
    public static extern uint UWK_CreateView(int width, int height, int maxWidth, int maxHeight, [MarshalAs(UnmanagedType.LPStr)]String url, IntPtr nativeTexturePtr);

    [DllImport("UWKPlugin")]
    public static extern void UWK_DestroyView(uint browserID);

    [DllImport("UWKPlugin")]
    public static extern IntPtr UWK_GetRenderEventFunc();

    [DllImport("UWKPlugin")]
    public static extern void UWK_MsgMouseMove(uint browserID, int x, int y);

    [DllImport("UWKPlugin")]
    public static extern void UWK_MsgSetSize(uint browserID, int width, int height);

    [DllImport("UWKPlugin", CharSet = CharSet.Ansi)]
    public static extern uint UWK_PostUnityKeyEvent(uint browserID, ref UnityKeyEvent keyEvent);

    [DllImport("UWKPlugin")]
    public static extern void UWK_MsgMouseButton(uint browserID, int x, int y, int button, bool down);

    [DllImport("UWKPlugin")]
    public static extern void UWK_MsgMouseScroll(uint browserID, int x, int y, float scroll);

    [DllImport("UWKPlugin")]
    public static extern void UWK_MsgLoadURL(uint browserID, [MarshalAs(UnmanagedType.LPStr)]String url);

    [DllImport("UWKPlugin")]
    public static extern void UWK_MsgLoadHTML(uint browserID, [MarshalAs(UnmanagedType.LPStr)]String html, [MarshalAs(UnmanagedType.LPStr)]String url);

    [DllImport("UWKPlugin")]
    public static extern uint UWK_MsgNavigate(uint browserId, int value);

    [DllImport("UWKPlugin")]
    public static extern void UWK_MsgExecuteJavaScript(uint browserID, [MarshalAs(UnmanagedType.LPStr)]String javaScript);

    [DllImport("UWKPlugin")]
    public static extern void UWK_MsgEvalJavaScript(uint browserID, uint evalID, [MarshalAs(UnmanagedType.LPStr)]String javaScript);

    [DllImport("UWKPlugin")]
    public static extern void UWK_MsgWebMessageResponse(uint browserID, double queryID, bool success, [MarshalAs(UnmanagedType.LPStr)]String response);

    [DllImport("UWKPlugin")]
    public static extern void UWK_SetGlobalBoolProperty([MarshalAs(UnmanagedType.LPStr)]String globalVarName, [MarshalAs(UnmanagedType.LPStr)]String propertyName, bool value);

    [DllImport("UWKPlugin")]
    public static extern void UWK_SetGlobalNumberProperty([MarshalAs(UnmanagedType.LPStr)]String globalVarName, [MarshalAs(UnmanagedType.LPStr)]String propertyName, double value);

    [DllImport("UWKPlugin")]
    public static extern void UWK_SetGlobalStringProperty([MarshalAs(UnmanagedType.LPStr)]String globalVarName, [MarshalAs(UnmanagedType.LPStr)]String propertyName, [MarshalAs(UnmanagedType.LPStr)]String value);

    // Variant Maps

    [DllImport("UWKPlugin")]
    public static extern void UWK_VariantMapDispose(IntPtr vmap);

    [DllImport("UWKPlugin")]
    extern static IntPtr UWK_VariantMapGetString(IntPtr vmap, [MarshalAs(UnmanagedType.LPStr)]String url);

    static public string VariantMapGetString(IntPtr vmap, string key)
    {
        return Marshal.PtrToStringAnsi(UWK_VariantMapGetString(vmap, key));
    }

    [DllImport("UWKPlugin")]
    public static extern uint UWK_VariantMapGetUInt(IntPtr vmap, [MarshalAs(UnmanagedType.LPStr)]String url);

    [DllImport("UWKPlugin")]
    public static extern bool UWK_VariantMapGetBool(IntPtr vmap, [MarshalAs(UnmanagedType.LPStr)]String url);

    [DllImport("UWKPlugin")]
    public static extern double UWK_VariantMapGetDouble(IntPtr vmap, [MarshalAs(UnmanagedType.LPStr)]String url);

    private static void processUWKEvent(StringBuilder eventType, IntPtr variantMap)
    {
        using (UWKEvent e = new UWKEvent())
        {

            e.variantMap = variantMap;
            e.eventType = eventType.ToString();

            if (e.eventType.StartsWith("E_IPCWEBVIEW"))
            {

                UWKCore.ProcessWebViewEvent(e);

            }
        }
    }

}

public class UWKEvent : IDisposable
{
    public String eventType;
    public IntPtr variantMap;

    public void Dispose()
    {
        UWKPlugin.UWK_VariantMapDispose(variantMap);
    }

    public String GetString(String key)
    {
        return UWKPlugin.VariantMapGetString(variantMap, key);
    }

    public uint GetUInt(String key)
    {
        return UWKPlugin.UWK_VariantMapGetUInt(variantMap, key);
    }

    public double GetDouble(String key)
    {
        return UWKPlugin.UWK_VariantMapGetDouble(variantMap, key);
    }

    public bool GetBool(String key)
    {
        return UWKPlugin.UWK_VariantMapGetBool(variantMap, key);
    }


}

[Flags]
enum UnityKeyModifiers
{
    Shift = 0x1,
    Control = 0x2,
    Alt = 0x4,
    CommandWin = 0x8, // windows or command key
    Numeric = 0x10,
    CapsLock = 0x20,
    FunctionKey = 0x40
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct UnityKeyEvent
{
    public uint Type; // 1 for down 0 for up
    public uint Modifiers;
    public uint KeyCode;
    public uint Character;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void LogCallbackDelegate(string message, int level);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void LogErrorDelegate(string message, bool fatal);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ProcessUWKEventDelegate(StringBuilder eventType, IntPtr variantMap);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void UWKBetaDelegate(int betaDays);