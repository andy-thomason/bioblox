using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class GetFile
{

#if UNITY_WEBGL

    [DllImport("__Internal")]
    private static extern void getFileFromBrowser(string objectName, string callbackFuncName);

#endif

    static public void GetFileFromUserAsync(string objectName, string callbackFuncName)
    {
#if UNITY_WEBGL

        getFileFromBrowser(objectName, callbackFuncName);

#else

            Debug.LogError("Not implemented in this platform");

#endif
    }
}