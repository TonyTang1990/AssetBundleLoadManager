package com.tonytang.assetbundleframewok;

import android.util.Log;

import com.unity3d.player.UnityPlayer;

//封装一层Log，方便往CS层转发Log显示
public class DIYLog {

    //CS层响应原生消息的GameObject名字
    public final static String mUnityCallBackHandler = "GameLauncher";

    //Java层Log的Tag
    private final static String LogTag = "JNI";

    public static void d(String msg)
    {
        Log.d(LogTag, msg);
        UnityPlayer.UnitySendMessage(mUnityCallBackHandler,"resJavaLog", msg);
    }

    public static void i(String msg)
    {
        Log.i(LogTag, msg);
        UnityPlayer.UnitySendMessage(mUnityCallBackHandler,"resJavaLog", msg);
    }

    public static void e(String msg)
    {
        Log.e(LogTag, msg);
        UnityPlayer.UnitySendMessage(mUnityCallBackHandler,"resJavaLog", msg);
    }
}
