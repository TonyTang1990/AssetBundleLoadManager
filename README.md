# 工具

1. Unity版本(2019 4.31f1c1或2020.3.34f1c2)
2. Visual Studio 2019

# 功能模块

## 资源加载管理和打包模块

### AssetBundle加载管理

基于索引计数+组件绑定的AssetBundle加载管理框架。(参考: tangzx/ABSystem思路)

资源加载管理设计:

1. 面向Asset级别加载管理，支持Asset和AssetBundle级别的同步异步加载。
2. 支持资源导入后AssetDatabase模式马上就能配置全路径加载
3. 资源加载类型只提供普通和常驻两种(且不支持运行时切换相同Asset或AssetBundle的加载类型，意味着一旦第一次加载设定了类型，就再也不能改变，同时第一次因为加载Asset而加载某个AssetBundle的加载类型和Asset一致)，同时提供统一的加载管理策略，细节管理策略由上层自己设计(比如对象池，预加载)
4. 异步加载准备采用监听回调的方式来实现，保证流程清晰易懂
5. 设计请求UID的概念来支持加载打断设计(仅逻辑层面的打断，资源加载不会打断，当所有逻辑回调都取消时，加载完成时会返还索引计数确保资源正确卸载)
6. 设计上支持动态AB下载(未来填坑)
7. 加载流程重新设计，让代码更清晰
8. **保留索引计数(Asset和AssetBundle级别)+对象绑定的设计(Asset和AssetBundle级别)+按AssetBundle级别卸载(依赖还原的Asset无法准确得知所以无法直接卸载Asset)+加载触发就提前计数(避免异步加载或异步加载打断情况下资源管理异常)**
9. **支持非回调式的同步加载返回(通过抽象Loader支持LoadImmediately的方式实现)**

Note:

1. 一直以来设计上都是加载完成后才添加索引计数和对象绑定，这样对于异步加载以及异步打断的资源管理来说是有漏洞的，**资源加载管理准备设计成提前添加索引计数，等加载完成后再考虑是否返还计数的方式确保异步加载以及异步加载打断的正确资源管理**

加载流程设计主要参考:

[XAsset](https://github.com/xasset/xasset)

对象绑定加索引计数设计主要参考:

[tangzx/ABSystem](https://github.com/tangzx/ABSystem)

#### AB加载管理方案

加载管理方案：

1. 加载指定资源
2. 加载自身AB(自身AB加载完通知资源加载层移除该AB加载任务避免重复的加载任务被创建)，自身AB加载完判定是否有依赖AB
3. 有则加载依赖AB(增加依赖AB的引用计数)(依赖AB采用和自身AB相同的加载方式(ResourceLoadMethod),但依赖AB统一采用ResourceLoadType.NormalLoad加载类型)
4. 自身AB和所有依赖AB加载完回调通知逻辑层可以开始加载Asset资源(AB绑定对象在这一步)
5. 判定AB是否满足引用计数为0，绑定对象为空，且为NormalLoad加载方式则卸载该AB(并释放依赖AB的计数减一)(通知资源管理层AB卸载，重用AssetBundleInfo对象)
6. 切场景，递归判定卸载NormalLoad加载类型AB资源(上层使用逻辑调用接口触发)

相关设计：

1. 依赖AB与被依赖者采用同样的加载方式(ResourceLoadMethod)，但加载方式依赖AB统一采用ResourceLoadType.NormalLoad
2. 依赖AB通过索引计数管理，只要原始AB不被卸载，依赖AB就不会被卸载
3. 已加载的AB资源加载类型不允许改变(直到加载后被卸载再次加载才能切换)

#### Demo使用说明

先打开资源调试工具

Tools->Debug->资源调试工具

1. AssetBundle和AssetDatabase资源加载模式切换![AssetDatabaseModuleSwitch](./img/Unity/AssetBundle-Framework/AssetDatabaseModuleSwitch.png)

2. AB依赖信息查看界面

   ![AssetBundleDepInfoUI](./img/Unity/AssetBundle-Framework/AssetBundleDepInfoUI.png)

3. AB运行时加载管理详细信息界面

   ![AssetBundleLoadManagerUI](./img/Unity/AssetBundle-Framework/AssetBundleLoadManagerUI.png)

4. 加载器信息查看界面

   ![AssetBundleAsyncUI](./img/Unity/AssetBundle-Framework/LoaderDebugUI.png)

5. 测试界面

   ![AssetBundleTestUI](./img/Unity/AssetBundle-Framework/AssetBundleTestUI.png)

6. 点击加载窗口预制件按钮后:

   ```CS
   ResourceManager.Singleton.getPrefabInstance(
       "Assets/Res/windows/MainWindow.prefab",
       (prefabInstance, requestUid) =>
       {
           mMainWindow = prefabInstance;
           mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
       }
   );
   ```
   
   ![AssetBundleLoadManagerUIAfterLoadWindow](./img/Unity/AssetBundle-Framework/AssetBundleLoadManagerUIAfterLoadWindow.png)
   可以看到窗口mainwindow依赖于loadingscreen，导致我们加载窗口资源时，loadingscreen作为依赖AB被加载进来了(引用计数为1)，窗口资源被绑定到实例出来的窗口对象上(绑定对象MainWindow)
   
7. 点击测试异步转同步加载窗口

```CS
/// <summary>
/// 测试异步转同步窗口加载
/// </summary>
public void onAsynToSyncLoadWindow()
{
    DIYLog.Log("onAsynToSyncLoadWindow()");
    if (mMainWindow == null)
    {
        onDestroyWindowInstance();
    }
    AssetLoader assetLoader;
    var requestUID = ResourceManager.Singleton.getPrefabInstanceAsync(
        "Assets/Res/windows/MainWindow.prefab",
        out assetLoader,
        (prefabInstance, requestUid) =>
        {
            mMainWindow = prefabInstance;
            mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
        }
    );
    // 将异步转同步加载
    assetLoader.loadImmediately();
}
```

8. 点击销毁窗口实例对象后

```CS
/// <summary>
/// 销毁窗口实例对象
/// </summary>
public void onDestroyWindowInstance()
{
    DIYLog.Log("onDestroyWindowInstance()");
    GameObject.Destroy(mMainWindow);
}
窗口销毁后可以看到之前加载的资源所有绑定对象都为空了，因为被销毁了(MainWindow被销毁了)
```

​		![AssetBundleLoadManagerUIAfterDestroyWindow](./img/Unity/AssetBundle-Framework/AssetBundleLoadManagerUIAfterDestroyWindow.png)

9. 等待回收检测回收后
   ![AssetBundleLoadManagerUIAfterUnloadAB](./img/Unity/AssetBundle-Framework/AssetBundleLoadManagerUIAfterUnloadAB.png)
   上述资源在窗口销毁后，满足了可回收的三大条件(1. 索引计数为0 2. 绑定对象为空 3. NormalLoad加载方式)，最终被成功回收。

Note:

读者可能注意到shaderlist索引计数为0，也没绑定对象，但没有被卸载，这是因为shaderlist是被我预加载以常驻资源的形式加载进来的(PermanentLoad)，所以永远不会被卸载。

```CS
/// <summary>
/// 加载常驻Shader
/// </summary>
public void onLoadPermanentShaderList()
{
    DIYLog.Log("onLoadPermanentShaderList()");
    ResourceManager.Singleton.loadAllShader("shaderlist", () =>
    {
    },
    ResourceLoadType.PermanentLoad);
}
```

### AssetBundle打包

**AB打包主要参考[MotionFramework](https://github.com/gmhevinci/MotionFramework)里的AB打包思路，细节部分个人做了一些改动和扩展。**

**AB打包设计:**

1. **打包AB的策略由抽象的目录打包策略设定决定**

2. **打包后的AB保留目录结构，确保AB模式和AssetDatabase模式加载都面向Asset路径保持一致性**

3. **支持打包策略级别的AB压缩格式设置(Note: 仅限使用ScriptableBuildPipeline打包模式)。老版AB打包流程AB压缩格式默认由打包面板压缩格式设置决定。**

4. **不支持AB变体功能(ScriptableBuildPipeline也不支持变体功能)，AB后缀名统一由打包和加载平台统一添加**

5. **老版AB依赖信息采用原始打包输出的*Manifest文件。新版ScriptableBuildPipeline采用自定义输出打包的CompatibilityAssetBundleManifest文件。**

#### 打包策略支持

1. 按目录打包(打包策略递归子目录判定)

2. 按文件打包(打包策略递归子目录判定)

3. 按固定名字打包(扩展支持固定名字打包--比如所有Shader打包到shaderlist)(打包策略递归子目录判定)

4. 按文件或子目录打包(打包策略递归子目录判定，设定目录按文件打包，其他下层目录按目录打包)

5. 不参与打包(打包策略递归子目录判定)

#### 相关操作UI

这里先简单的看下新的AB搜集和打包界面:

![AssetBundleCollectWindow](./img/Unity/AssetBundle-Framework/AssetBundleCollectWindow.PNG)

![AssetBundleBuildWindow](./img/Unity/AssetBundle-Framework/AssetBundleBuildWindow.PNG)

关于Asset路径与AB路径关联信息存在一个叫assetbundlebuildinfo.asset的ScriptableObejct里(单独打包到assetbuildinfo的AB里)，通过Asset路径如何加载到对应AB的关键就在这里。这里和MotionFramework自定义Manifest文件输出不一样，assetbundlebuildinfo.asset只记录AssetPath和AB相关信息映射，不记录AB依赖信息，依赖信息依然采用AB打包生成的*Manifest文件，同时assetbundlebuildinfo.asset采用打包AB的方式（方便和热更新AB走一套机制）

让我们先来看下大致数据信息结构:

![AssetBundleBuildInfoView1](./img/Unity/AssetBundle-Framework/AssetBundleBuildInfoView1.PNG)

![AssetBundleBuildInfoView2](./img/Unity/AssetBundle-Framework/AssetBundleBuildInfoView2.PNG)

**2022/1/26支持了资源打包后缀名黑名单可视化配置+资源名黑名单可视化配置**

![PostFixBlackListAndAssetNameBlackList](./img/Unity/AssetBundle-Framework/PostFixBlackListAndAssetNameBlackList.PNG)

**2023/2/8底层支持了新版ScriptableBuildPipeline打包工具打包，加快打包速度(需添加SCRIPTABLE_ASSET_BUILD_PIPELINE宏)**

## 热更新模块

### 类说明

热更类：

```csharp
- HotUpdateModuleManager.cs(热更新管理模块单例类)
- TWebRequest.cs(资源下载http抽象类)
```

版本信息类：

```csharp
- VersionConfigModuleManager.cs(版本管理模块单例类)
- VersionConfig.cs(版本信息抽象类)
```

### 功能支持

1. 支持游戏内版本强更(完成 -- 暂时限Android，IOS待测试)
2. 支持游戏内资源热更(完成 -- 暂时限Android， IOS待测试)
3. 支持游戏内代码热更(未做)

### 热更测试说明

使用阿里的ISS静态资源服务器做了一个网络端的资源服务器。

版本强更流程：

1. 比较包内版本信息和包外版本信息检查是否强更过版本
2. 如果强更过版本清空包外相关信息目录
3. 通过资源服务器下载最新服务器版本信息(ServerVersionConfig.json)和本地版本号作对比，决定是否强更版本
4. 结合最新版本号和资源服务器地址(Json配置)拼接出最终热更版本所在的资源服务器地址
5. 下载对应版本号下的强更包并安装
6. 安装完成，退出游戏重进

资源热更流程：

   1. 初始化本地热更过的资源列表信息(暂时存储在:Application.persistentDataPath + "/ResourceUpdateList/ResourceUpdateList.txt"里)

   2. 通过资源服务器下载最新服务器版本信息(ServerVersionConfig.json)和本地资源版本号作对比，决定是否资源热更

3. 结合最新版本号，最新资源版本号和资源服务器地址(Json配置)拼接出最终资源热更所在的资源服务器地址

4. 下载对应地址下的AssetBundleMD5.txt(里面包含了对应详细资源MD5信息)

      AssetBundleMD5.txt

      ```tex
      assetbuildinfo.bundle|ca830d174533e87efad18f1640e5301d
      shaderlist.bundle|2ac2d75f7d91fda7880f447e21b2e289
      ******
      ```

5. 根据比较对应地址下的AssetBundleMD5.txt里的资源MD5信息和本地资源MD5信息(优先包外的MD5文件)得出需要更新下载的资源列表

6. 根据得出的需要更新的资源列表下载对应资源地址下的资源并存储在包外(Application.persistentDataPath + "/Android/")，同时写入最新的资源MD5信息文件(本地AssetBundleMD5.txt)到本地

7. 直到所有资源热更完成，退出重进游戏

**问题:**

1. **上述方案包外AssetBundleMD5.txt文件可能被篡改，其次热更下载的AB文件可能出现损坏但被记录到包外AssetBundleMD5.txt的情况，这会导致热更AB出现不可逆的热更问题。**

**解决方案:**

1. **包外资源采用实时计算MD5，然后结合包内AssetBundleMD5.txt数据与热更AssetBundleMD5.txt文件进行对比决定哪些资源需要热更。(TODO)**

### 流程图

![HotUpdateFlowChat](./img/Unity/HotUpdate/HotUpdateFlowChat.png)

### 热更新辅助工具

Tools->HotUpdate->热更新操作工具

![HotUpdateToolsUI](./img/Unity/HotUpdate/HotUpdateToolsUI.png)

2. 主要分为以下2个阶段：

   - 热更新准备阶段:
   
     1. 每次资源打包会在包内Resource目录生成一个AssetBundleMd5.txt文件用于记录和对比哪些资源需要热更
   
     ​	![AssetBundleMD5File](./img/Unity/HotUpdate/AssetBundleMD5File.png)
   
     2. 执行热更新准备操作，生成热更新所需服务器最新版本信息文件(ServerVersionConfig.json)并将包内对应平台资源拷贝到热更新准备目录
   
     ![HotUpdatePreparationFolder](./img/Unity/HotUpdate/HotUpdatePreparationFolder.png)
   
   - 热更新判定阶段
   
     1. 初始化包内(AssetBundleMd5.txt)和包外(AssetBundleMd5.txt)热更新的AssetBundle MD5信息(先读包内后读包外以包外为准)
   
     2. 游戏运行拉去服务器版本和资源版本信息进行比较是否需要版本强更或资源热更新
     3. 需要资源热更新则拉去对应最新资源版本的资源MD5信息文件(AssetBundleMD5.txt)进行和本地资源MD5信息进行比较判定哪些资源需要热更新
     4. 拉去所有需要热更新的资源并写入最新的资源MD5信息到包外，完成后进入游戏
   
   Note:
   
   1. 每次打包版本时会拷贝一份AssetBundleMD5.txt到打包输出目录(保存一份方便查看每个版本的资源MD5信息)

### 热更包外目录结构

PersistentAsset -> HotUpdate -> Platform(资源热更新目录)

PersistentAsset -> HotUpdate -> AssetBundleMd5.txt(记录热更新的AssetBundle路径和MD5信息--兼顾进游戏前资源热更和动态资源热更)(格式:热更AB路径:热更AB的MD5/n热更AB路径:热更AB的MD5******)

PersistentAsset -> HotUpdate-> Config -> VersionConfig.json(包外版本信息--用于进游戏前强更和热更判定)

PersistentAsset -> HotUpdate -> Download -> 版本强更包

## 辅助功能模块

### 资源处理分析

1. 支持资源依赖统计(不限资源类型)
2. 支持内置资源引用分析
3. 支持内置资源提取(限材质和纹理，不包含Shader是考虑到Shader可以自行下载) 
4. 支持shader变体搜集(半成品)

资源辅助工具五件套：

Tools->AssetBundle->AssetBundle操作工具

Tools->Assets->Asset相关处理

1. AB删除判定工具

   ![DeleteRemovedAssetBundle](/img/Unity/AssetBundle-Framework/DeleteRemovedAssetBundle.png)

2. 资源依赖查看工具

   ![AssetDependenciesBrowser](./img/Unity/AssetBundle-Framework/AssetDependenciesBrowser.png)

3. 内置资源依赖统计工具(只统计了*.mat和*.prefab，场景建议做成Prefab来统计)

   ![BuildInResourceReferenceAnalyze](./img/Unity/AssetBundle-Framework/BuildInResourceReferenceAnalyze.png)

 4. 内置资源提取工具

    ![BuildInResourceExtraction](./img/Unity/AssetBundle-Framework/BuildInResourceExtraction.png)

5. Shader变体搜集工具(半成品，只是简单的把所有材质放到场景中用摄像机照射一次让Unity能搜集到变体，**可能会遗漏一些特殊情况下的变体**)

   ![ShaderVariantsCollection](./img/Unity/AssetBundle-Framework/ShaderVariantsCollection.png) 

# 注意事项

1. **AB打包和加载使用ScriptableBuildPipeline模式需添加宏:SCRIPTABLE_ASSET_BUILD_PIPELINE**
2. **老版AB打包只支持AssetName小写全路径，所以针对老版AB打包和加载AssetName本人都统一成了小写处理**

# 重大问题修复

1. **修复资源打包在2020和2021版本会报错(BuildPipeline error is thrown when building Asset Bundles](https://issuetracker.unity3d.com/issues/buildpipeline-error-is-thrown-when-building-asset-bundles))问题(2022/06/03)**

# 待做事项

**1. 支持真机代码热更(Lua + XLua)**

**2. 热更新资源正确性校验(MD5校验)**

# 个人博客

详细的博客记录学习:

[AssetBundle资源打包加载管理](http://tonytang1990.github.io/2018/10/24/AssetBundle%E8%B5%84%E6%BA%90%E6%89%93%E5%8C%85%E5%8A%A0%E8%BD%BD%E7%AE%A1%E7%90%86%E5%AD%A6%E4%B9%A0/)

[热更新](http://tonytang1990.github.io/2019/05/03/%E7%83%AD%E6%9B%B4%E6%96%B0/)

# 鸣谢

感谢tangzx/ABSystem作者的无私分享，tangzx/ABSystem的Github链接:

[tangzx/ABSystem](https://github.com/tangzx/ABSystem)

感谢MotionFramework作者的无私分享,MotionFramework的Github链接:

[MotionFramework](https://github.com/gmhevinci/MotionFramework)

感谢XAsset作者的无私分享,XAsset的GitHub链接:

[XAsset](https://github.com/xasset/xasset)