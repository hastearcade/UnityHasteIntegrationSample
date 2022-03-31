using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MobileDetection : MonoBehaviour
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool IsIOSDevice();
#else
  [DllImport("mobileDetection.jslib")]
  private static extern bool IsIOSDevice();
#endif

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern string GetServerUrlAddress();
#else
  [DllImport("mobileDetection.jslib")]
  private static extern string GetServerUrlAddress();
#endif

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern string GetToken();
#else
  [DllImport("mobileDetection.jslib")]
  private static extern string GetToken();
#endif
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool PerformLogout();
#else
  [DllImport("mobileDetection.jslib")]
  private static extern bool PerformLogout();
#endif
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool BackToArcade();
#else
  [DllImport("mobileDetection.jslib")]
  private static extern bool BackToArcade();
#endif
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool IsLandscape();
#else
  [DllImport("mobileDetection.jslib")]
  private static extern bool IsLandscape();
#endif

    public static bool CheckForIOS()
    {
#if UNITY_EDITOR
        return false; // value to return in Play Mode (in the editor)
#elif UNITY_WEBGL
    return IsIOSDevice(); // value based on the current browser
#else
    return false; // value for builds other than WebGL
#endif
    }

    public static string GetServerUrl()
    {
#if UNITY_EDITOR
        return "localhost"; // value to return in Play Mode (in the editor)
#elif UNITY_WEBGL
    var address = GetServerUrlAddress();
    return address; // value based on the current browser
#else
    var address = GetServerUrlAddress();
    return address; // value based on the current browser
#endif
    }

    public static string GetTokenFromBrowser()
    {
#if UNITY_EDITOR
        return "";
#elif UNITY_WEBGL
    return GetToken();
#else
    return GetToken();
#endif
    }

    public static bool Logout()
    {
#if UNITY_EDITOR
        return false;
#elif UNITY_WEBGL
    return PerformLogout();
#else
    return PerformLogout();
#endif
    }

    public static bool GoBackToArcade()
    {
#if UNITY_EDITOR
        return false;
#elif UNITY_WEBGL
    return BackToArcade();
#else
    return BackToArcade();
#endif
    }

    public static bool GetIsLandscape()
    {
#if UNITY_EDITOR
        return Screen.height < Screen.width;
#elif UNITY_WEBGL
    return IsLandscape();
#else
    return Screen.height < Screen.width;
#endif
    }
}
