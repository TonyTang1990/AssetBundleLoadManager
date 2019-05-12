package com.tonytang.assetbundleframewok;

import android.content.Intent;
import android.content.res.Resources;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;

import android.app.Activity;
import android.content.Context;
import android.content.res.Configuration;
import android.os.Environment;
import android.support.v4.content.FileProvider;
import android.util.DisplayMetrics;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import java.io.File;

public class MainActivity extends UnityPlayerActivity {

    //CS层响应原生消息的GameObject名字
    public final static String mUnityCallBackHandler = "GameLauncher";
    public Context mContext = null;

    public Activity mCurrentActivity = null;

    private String mPackagename;

    private Resources mResources;

    private int mWidth;

    private int mHeight;

    private final String LogTag = "JNI";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        DIYLog.d("onCreate()");
        mContext = this.getApplicationContext();
        mCurrentActivity = this;
        mPackagename = mContext.getPackageName();
        DIYLog.d("mPackagename :" + mPackagename);
        mResources = getResources();

        DisplayMetrics dm = getResources().getDisplayMetrics();
        mWidth = dm.widthPixels;
        mHeight = dm.heightPixels;
        DIYLog.d("mWidth :" + mWidth);
        DIYLog.d("mHeight :" + mHeight);

        DIYLog.d("Environment.getExternalStorageDirectory() = " + Environment.getExternalStorageDirectory());
    }

    protected void onStart()
    {
        super.onStart();

        DIYLog.d("onStart()");
    }

    protected void onResume()
    {
        super.onResume();

        DIYLog.d("onResume()");
    }

    protected void onPause()
    {
        super.onPause();

        DIYLog.d("onPause()");
    }

    protected void onStop()
    {
        super.onStop();

        DIYLog.d("onStop()");
    }

    protected void onDestroy()
    {
        super.onDestroy();

        DIYLog.d("onDestroy()");
    }

    public void onWindowFocusChanged(boolean hasFocus)
    {
        super.onWindowFocusChanged(hasFocus);

        DIYLog.d("onWindowFocusChanged(" + hasFocus + ")");
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig)
    {
        super.onConfigurationChanged(newConfig);
        DIYLog.d("onConfigurationChanged()");
    }

    //Unity call part
    public void javaMethod(String csparam)
    {
        DIYLog.d("javaMethod() with parameter:" + csparam);
        UnityPlayer.UnitySendMessage(mUnityCallBackHandler,"resUnityMsg", "java param");
    }

    //测试安装APK强更
    public void installAPK(String apkfilepath)
    {
        DIYLog.d("installAPK(" + apkfilepath + ")");
        Intent install = new Intent(Intent.ACTION_VIEW);
        install.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        //File apkfile = new File(mContext.getExternalFilesDir(null).getPath() + "/download/" + "app.apk");
        File apkfile = new File(apkfilepath);
        DIYLog.d("----------------------------------------------");
        DIYLog.d(Environment.getExternalStorageDirectory().getPath());
        DIYLog.d(mContext.getFilesDir().getPath());
        DIYLog.d(getCacheDir().getPath());
        DIYLog.d(mContext.getExternalFilesDir(null).getPath());
        DIYLog.d(mContext.getExternalCacheDir().getPath());
        DIYLog.d("----------------------------------------------");

        Uri uri = null;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
            DIYLog.d("Build.VERSION.SDK_INT >= Build.VERSION_CODES.N");
            install.setFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            install.addFlags(Intent.FLAG_GRANT_WRITE_URI_PERMISSION);
            install.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            DIYLog.d(mPackagename + ".fileprovider");
            uri = FileProvider.getUriForFile(mContext, mPackagename + ".fileprovider", apkfile);
        } else {
            DIYLog.d("Build.VERSION.SDK_INT < Build.VERSION_CODES.N");
            uri = Uri.fromFile(apkfile);
        }
        install.setDataAndType(uri, "application/vnd.android.package-archive");

        /*
        //解决安卓8.0安装界面不弹出
        //查询所有符合 intent 跳转目标应用类型的应用，注意此方法必须放置在 setDataAndType 方法之后
        List<ResolveInfo> resolveLists = mCurrentActivity.getPackageManager().queryIntentActivities(install, PackageManager.MATCH_DEFAULT_ONLY);
        // 然后全部授权
        for (ResolveInfo resolveInfo : resolveLists)
        {
            String packageName = resolveInfo.activityInfo.packageName;
            mCurrentActivity.grantUriPermission(packageName, uri, Intent.FLAG_GRANT_READ_URI_PERMISSION | Intent.FLAG_GRANT_WRITE_URI_PERMISSION);
        }
        */
        startActivity(install);
    }
}
