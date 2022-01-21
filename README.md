# 功能模块

## 资源加载管理和打包模块

### 新版AssetBundle加载管理

基于索引计数+组件绑定的AssetBundle加载管理框架。(参考: tangzx/ABSystem思路)

为了弥补以前设计和实现上不足的地方，从而有了新版AssetBundle加载管理和打包的编写。

老版的资源加载管理缺点：

1. 面向AssetBundle级别，没有面向Asset级别的加载管理，无法做到Asset级别的加载异步以及Asset级别加载取消的。
2. 老版AssetDatabase模式要求资源必须在设置AB名字后才能正确使用(因为依赖了AB名字作为加载参数而非资源全路径)，无法做到资源导入即可快速使用的迭代开发
3. 资源加载类型分类(普通，预加载，常驻)设计过于面向切场景的游戏设计，不通用到所有游戏类型
4. 老版AssetBundle异步加载采用了开携程的方式，代码流程看起来会比较混乱
5. 老版异步加载没有考虑设计上支持逻辑层加载打断
6. 老版代码没有涉及考虑动态AB下载的设计(边玩边下)
7. 资源加载代码设计还比较混乱，不容易让人看懂看明白

综合上面4个问题，新版资源加载管理将支持:

1. 面向Asset级别加载管理，支持Asset和AssetBundle级别的同步异步加载。
2. 支持资源导入后AssetDatabase模式马上就能配置全路径加载
3. 资源加载类型只提供普通和常驻两种(且不支持运行时切换相同Asset或AssetBundle的加载类型，意味着一旦第一次加载设定了类型，就再也不能改变，同时第一次因为加载Asset而加载某个AssetBundle的加载类型和Asset一致)，同时提供统一的加载管理策略，细节管理策略由上层自己设计(比如对象池，预加载)
4. 新版异步加载准备采用监听回调的方式来实现，保证流程清晰易懂
5. 新版设计请求UID的概念来支持加载打断设计(仅逻辑层面的打断，资源加载不会打断，当所有逻辑回调都取消时，加载完成时会返还索引计数确保资源正确卸载)
6. 设计上支持动态AB下载(未来填坑)
7. 加载流程重新设计，让代码更清晰
8. **保留索引计数(Asset和AssetBundle级别)+对象绑定的设计(Asset和AssetBundle级别)+按AssetBundle级别卸载(依赖还原的Asset无法准确得知所以无法直接卸载Asset)+加载触发就提前计数(避免异步加载或异步加载打断情况下资源管理异常)**
9. **支持非回调式的同步加载返回(通过抽象Loader支持LoadImmediately的方式实现)**

Note:

1. 一直以来设计上都是加载完成后才添加索引计数和对象绑定，这样对于异步加载以及异步打断的资源管理来说是有漏洞的，**新版资源加载管理准备设计成提前添加索引计数，等加载完成后再考虑是否返还计数的方式确保异步加载以及异步加载打断的正确资源管理**

加载流程设计主要参考:

[XAsset](https://github.com/xasset/xasset)

对象绑定加索引计数设计主要参考:

[tangzx/ABSystem](https://github.com/tangzx/ABSystem)

#### 类说明

Manager统一管理：

    - ModuleManager(单例类 Manager of Manager的管理类)
    - ModuleInterface(模块接口类)

资源加载类：

    - ResourceLoadMethod(资源加载方式枚举类型 -- 同步 or 异步)
    - ResourceLoadMode(资源加载模式 -- AssetBundle or AssetDatabase(**限Editor模式下可切换，支持同步和异步(异步是本地模拟延迟加载来实现的)加载方式**))
    - ResourceLoadState(资源加载状态 -- 错误，等待加载， 加载中，完成，取消之类的)
    - ResourceLoadType(资源加载类型 -- 正常加载，常驻加载)
    - ResourceModuleManager(资源加载模块统一入口管理类)
    - AbstractResourceModule(资源加载模块抽象)
    - AssetBundleModule(AssetBundle模式下的实际加载管理模块)
    - AssetDatabaseModule(AssetDatabase模式下的实际加载管理模块)
    - AbstractResourceInfo(资源加载使用信息抽象)
    - AssetBundleInfo(AssetBundle资源使用信息)
    - AssetInfo(Asset资源使用信息)
    - LoaderManager(加载器管理单例类)
    - Loadable(资源加载器基类--抽象加载流程)
    - AssetLoader(Asset加载器基类抽象)
    - BundleAssetLoader(AssetBundle模式下的Asset加载器)
    - AssetDatabaseLoader(AssetDatabase模式下的Asset加载器)
    - BundleLoader(AssetBundle加载器基类抽象)
    - AssetBundleLoader(本地AssetBundle加载器)
    - DownloadAssetBundleLoader(动态资源AsserBundle加载器)
    - AssetDatabaseAsyncRequest(AssetDatabase模式下异步加载模拟)
    - AssetBundlePath(AB资源路径相关 -- 处理多平台以及热更资源加载路径问题)
    - ResourceDebugWindow.cs(Editor运行模式下可视化查看资源加载(AssetBundle和AssetDatabase两种都支持)详细信息的辅助工具窗口)
    - ResourceConstData(资源打包加载相关常量数据)
    - ResourceLoadAnalyse(资源加载统计分析工具)

#### AB加载管理方案

加载管理方案：

1. 加载指定资源
2. 加载自身AB(自身AB加载完通知资源加载层移除该AB加载任务避免重复的加载任务被创建)，自身AB加载完判定是否有依赖AB
3. 有则加载依赖AB(增加依赖AB的引用计数)(依赖AB采用和自身AB相同的加载方式(ResourceLoadMethod),但依赖AB统一采用ResourceLoadType.NormalLoad加载类型)
4. 自身AB和所有依赖AB加载完回调通知逻辑层可以开始加载Asset资源(AB绑定对象在这一步)
5. 判定AB是否满足引用计数为0，绑定对象为空，且为NormalLoad加载方式则卸载该AB(并释放依赖AB的计数减一)(通知资源管理层AB卸载，重用AssetBundleInfo对象)
6. 切场景，递归判定卸载PreloadLoad加载类型AB资源

相关设计：

1. 依赖AB与被依赖者采用同样的加载方式(ResourceLoadMethod)，但加载方式依赖AB统一采用ResourceLoadType.NormalLoad
2. 依赖AB通过索引计数管理，只要原始AB不被卸载，依赖AB就不会被卸载
3. 已加载的AB资源加载类型只允许从低往高变(NormalLoad -> Preload -> PermanentLoad)，不允许从高往低(PermanentLoad -> Preload -> NormalLoad)

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

### 新版AssetBundle打包

**新版AB打包主要参考[MotionFramework](https://github.com/gmhevinci/MotionFramework)里的AB打包思路(所以拷贝了不少该作者的核心代码)，细节部分个人做了一些扩展。**

**主要变动如下:**

**1. 打包AB的策略由抽象的目录打包策略设定决定**

**2. 打包后的AB保留目录结构，确保AB模式和AssetDatabase模式加载都面向Asset路径保持一致性**

#### 核心AB打包思想和流程

1. 通过抽象纯虚拟的打包策略设置(即AB收集打包策略设置界面--设置指定目录的打包策略)，做到AB打包策略设置完全抽象AB名字设置无关化(这样一来无需设置AB或清除AB名字，自己完全掌控AB打包策略和打包结论)
2. 打包时分析打包策略设置的所有有效资源信息，统计出所有有效资源的是否参与打包以及依赖相关等信息，然后结合所有的打包策略设置分析出所有有效Asset的AB打包名字(如果Asset满足多个打包策略设置，默认采用最里层的打包策略设置，找不到符合的采用默认收集打包规则)**(这一步是分析关键，下面细说一下详细步骤)**
   - 通过自定义设置的打包策略得到所有的有效参与打包路径列表
   - 通过AssetDatabase.FindAssets()结合打包路径列表得出所有需要分析的Asset
   - 通过AssetDatabase.GetDependencies(***, true)遍历所有需要分析的Asset列表得出所有Asset的使用信息(是否有依赖使用，是否是需要参与打包的灯)
   - 通过遍历得到的所有Asset使用信息的路径信息分析设置Asset的AB打包变体信息
   - 通过遍历得到的所有Asset信息列表结合AssetDatabase.GetDependencies(***, false)接口排除依赖未设置参与打包的Asset资源并得出最终的打包信息列表List<AssetBundleBuildInfo>
   - 在最后分析得出最后的打包结论之前，这里我个人将AB的依赖信息文件(AssetBuildInfo.asset)的生成和打包信息单独插入在这里，方便AB依赖信息可以跟AB资源一起构建参与热更
   - 最后根据分析得出的打包信息列表List<AssetBundleBuildInfo>构建真真的打包信息List<AssetBundleBuild>进行打包
   - AB打包完成后进行一些后续的特殊资源处理(比如视频单独打包。AB打包的依赖文件删除(个人采用自定义生成的AssetBuildInfo.Asset作为依赖加载信息文件)。循环依赖检查。创建打包说明文件等。)
3. 不同的打包规则通过反射创建每样一个来实现获取对应打包规则的打包AB名字结论获取(采用全路径AB名的方式，方便快速查看资源打包分布)
4. 然后根据所有有效Asset的所有AB名字打包结论来分析得出自定义的打包结论(即哪些Asset打包到哪个AB名里)
5. 接着根据Asset的AB打包结论来生成最新的AssetBuildInfo(Asset打包信息，可以理解成我们自己分析得出的Manifest文件，用于运行时加载作为资源加载的基础信息数来源)(手动将AssetBuildInfo添加到打包信息里打包成AB，方便热更新走统一流程)
6. 最后采用BuildPipeline.BuildAssetBundles(输出目录, 打包信息列表, ......)的接口来手动指定打包结论的方式触发AB打包。

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

关于Asset路径与AB路径关联信息以及AB依赖信息最终都存在一个叫assetbundlebuildinfo.asset的ScriptableObejct里(单独打包到assetbuildinfo的AB里)，通过Asset路径如何加载到对应AB以及依赖AB的关键就在这里。(这里和MotionFramework自定义Manifest文件输出不一样，采用打包AB的方式，为了和热更新AB走一套机制)

让我们先来看下大致数据信息结构:

![AssetBundleBuildInfoView1](./img/Unity/AssetBundle-Framework/AssetBundleBuildInfoView1.PNG)

![AssetBundleBuildInfoView2](./img/Unity/AssetBundle-Framework/AssetBundleBuildInfoView2.PNG)

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

之前是使用的[HFS](http://www.rejetto.com/hfs/)快速搭建的一个资源本地资源服务器，后来使用阿里的ISS静态资源服务器做了一个网络端的资源服务器。

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

PersistentAsset -> Config -> VersionConfig.json(包外版本信息--用于进游戏前强更和热更判定)

PersistentAsset -> HotUpdate -> 版本强更包

## 导表模块

集成导表工具[XbufferExcellToData](https://github.com/TonyTang1990/XbufferExcellToData)

### 详情:

[**XbufferExcellToData**](https://github.com/TonyTang1990/XbufferExcellToData)

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

# 待做事项

5.  支持真机代码热更(Lua + XLua)
7.  热更新资源正确性校验

# 个人博客

详细的博客记录学习:

[AssetBundle资源打包加载管理](http://tonytang1990.github.io/2018/10/24/AssetBundle%E8%B5%84%E6%BA%90%E6%89%93%E5%8C%85%E5%8A%A0%E8%BD%BD%E7%AE%A1%E7%90%86%E5%AD%A6%E4%B9%A0/)

[热更新](http://tonytang1990.github.io/2019/05/03/%E7%83%AD%E6%9B%B4%E6%96%B0/)

[导表工具](http://tonytang1990.github.io/2018/03/18/%E5%AF%BC%E8%A1%A8%E5%B7%A5%E5%85%B7/)

# 鸣谢

感谢tangzx/ABSystem作者的无私分享，tangzx/ABSystem的Github链接:

[tangzx/ABSystem](https://github.com/tangzx/ABSystem)

感谢MotionFramework作者的无私分享,MotionFramework的Github链接:

[MotionFramework](https://github.com/gmhevinci/MotionFramework)

感谢XAsset作者的无私分享,XAsset的GitHub链接:

[XAsset](https://github.com/xasset/xasset)