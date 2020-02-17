/*
 * Description:             游戏入口
 * Author:                  tanghuan
 * Create Date:             2018/03/12
 */

using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 游戏入口
/// </summary>
public class GameLauncher : MonoBehaviour {

    /// <summary>
    /// UI挂在节点
    /// </summary>
    public GameObject UIRootCanvas;

    /// <summary>
    /// UI根节点
    /// </summary>
    public GameObject UIRoot;

    /// <summary>
    /// 参数1
    /// </summary>
    public InputField InputParam1;

    /// <summary>
    /// 参数2
    /// </summary>
    public InputField InputParam2;

    /// <summary>
    /// 原生消息数据显示文本
    /// </summary>
    public Text TxtNativeOutput;

    /// <summary>
    /// 测试背景访问资源
    /// </summary>
    public TImage TImgBG;

    /// <summary>
    /// 窗口实例对象
    /// </summary>
    private GameObject mMainWindow;

    /// <summary>
    /// 角色实例对象
    /// </summary>
    private GameObject mActorInstance;

    /// <summary>
    /// 角色实例对象2
    /// </summary>
    private GameObject mActorInstance2;

    /// <summary>
    /// 音效临时实例对象
    /// </summary>
    private GameObject mSFXInstance;

    /// <summary>
    /// 当前场景ABI，用于手动释放引用
    /// </summary>
    private AssetBundleInfo mCurrentSceneABI;

    /// <summary>
    /// 资源管理单例对象(快速访问)
    /// </summary>
    private ResourceModuleManager mRMM;

    /// <summary>
    /// 背景音乐音效组件
    /// </summary>
    private AudioSource mBGMAudioSource;

    /// <summary>
    /// 当前场景背景音乐的资源信息
    /// </summary>
    private AbstractResourceInfo mCurrentBGMARI;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        DontDestroyOnLoad(UIRoot);

        initSingletons();

        addMonoComponents();

        nativeInitilization();

        initilization();
    }

    private void Start ()
    {

    }

    private void Update()
    {
        Timer.Singleton.Update();
        ResourceModuleManager.Singleton.Update();
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= VisibleLogUtility.getInstance().HandleLog;
    }

    /// <summary>
    /// 初始化单例
    /// </summary>
    private void initSingletons()
    {
        // 因为SingletonTemplate采用的是惰性初始化(即第一次调用的时候初始化)
        // 会造成单例构造函数无法一开始被触发的问题
        AtlasManager.Singleton.startUp();
    }

    /// <summary>
    /// 添加Mono相关的组件
    /// </summary>
    private void addMonoComponents()
    {
        VisibleLogUtility visiblelog = gameObject.AddComponent<VisibleLogUtility>();
        visiblelog.setInstance(visiblelog);
        VisibleLogUtility.getInstance().mVisibleLogSwitch = FastUIEntry.LogSwitch;
        Application.logMessageReceived += VisibleLogUtility.getInstance().HandleLog;

        gameObject.AddComponent<FastUIEntry>();

        gameObject.AddComponent<CoroutineManager>();

        gameObject.AddComponent<NativeMessageHandler>();
        NativeMessageHandler.Singleton.TxtNativeOutput = TxtNativeOutput;
    }

    /// <summary>
    /// 原生初始化
    /// </summary>
    private void nativeInitilization()
    {
        NativeManager.Singleton.init();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void initilization()
    {
        mRMM = ResourceModuleManager.Singleton;

        // 资源模块初始化
        mRMM.init();

        //热更模块初始化
        HotUpdateModuleManager.Singleton.init();

        // 预加载Shader
        //ResourceManager.Singleton.loadAllShader("shaderlist", () =>
        //{

        //},
        //ResourceLoadType.PermanentLoad);
        mRMM.startResourceRecyclingTask();

        //初始化版本信息
        VersionConfigModuleManager.Singleton.initVerisonConfigData();

        //初始化表格数据读取
        GameDataManager.Singleton.loadAll();

        // 初始化逻辑层Manager
        GameSceneManager.Singleton.init();

        mBGMAudioSource = GetComponent<AudioSource>();
    }
    
    /// <summary>
    /// 加载窗口预制件
    /// </summary>
    public void onLoadWindowPrefab()
    {
        DIYLog.Log("onLoadWindowPrefab()");
        ResourceManager.Singleton.getUIInstance("mainwindow", "mainwindow", (arg) =>
        {
            mMainWindow = arg;
            mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
        });
        //mRMM.requstResource(
        //"mainwindow",
        //(abi) =>
        //{
        //    mMainWindow = abi.instantiateAsset("MainWindow");
        //    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
        //});
    }

    /// <summary>
    /// 销毁窗口实例对象
    /// </summary>
    public void onDestroyWindowInstance()
    {
        DIYLog.Log("onDestroyWindowInstance()");
        GameObject.Destroy(mMainWindow);
    }

    /// <summary>
    /// 加载Image Sprite
    /// </summary>
    public void onLoadImageSprite()
    {
        DIYLog.Log("onLoadImageSprite()");
        var param1 = InputParam1.text;
        DIYLog.Log("Param1 = " + param1);
        var param2 = InputParam2.text;
        DIYLog.Log("Param2 = " + param2);
        var image = mMainWindow.transform.Find("imgBG").GetComponent<Image>();
        mRMM.requstResource(
        param1,
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, param2);
            image.sprite = sp;
        });

    }

    /// <summary>
    /// 加载TImage Sprite
    /// </summary>
    public void onLoadTImageSprite()
    {
        DIYLog.Log("onLoadTImageSprite()");
        var param1 = InputParam1.text;
        DIYLog.Log("Param1 = " + param1);
        var param2 = InputParam2.text;
        DIYLog.Log("Param2 = " + param2);
        var timage = mMainWindow.transform.Find("timgBG").GetComponent<TImage>();
        AtlasManager.Singleton.setImageSprite(timage, param1, param2);
    }

    /// <summary>
    /// 加载TImage Sprite Atlas
    /// </summary>
    public void onLoadTImageSpriteAtlas()
    {
        DIYLog.Log("onLoadTImageSpriteAtlas()");
        var param1 = InputParam1.text;
        DIYLog.Log("Param1 = " + param1);
        var param2 = InputParam2.text;
        DIYLog.Log("Param2 = " + param2);
        var timage = mMainWindow.transform.Find("timgBG").GetComponent<TImage>();
        AtlasManager.Singleton.setImageSpriteAtlas(timage, param1, param2);
    }


    /// <summary>
    /// 加载背景TImage Sprite Atlas
    /// </summary>
    public void onLoadTImageBGSpriteAtlas()
    {
        DIYLog.Log("onLoadTImageBGSpriteAtlas()");
        var param1 = InputParam1.text;
        DIYLog.Log("Param1 = " + param1);
        var param2 = InputParam2.text;
        DIYLog.Log("Param2 = " + param2);
        AtlasManager.Singleton.setImageSpriteAtlas(TImgBG, param1, param2);
    }


    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void onPlayBGM()
    {
        DIYLog.Log("onPlayBGM()");
        AudioManager.Singleton.playBGM("backgroundmusic");

        //if (mBGMAudioSource != null)
        //{
        //    //背景音效是挂载GameLaucher上会导致永远无法满足卸载条件，所以需要手动移除对象绑定
        //    if (mCurrentBGMARI != null)
        //    {
        //        mCurrentBGMARI.releaseOwner(mBGMAudioSource);
        //    }

        //    mRMM.requstResource("backgroundmusic",
        //    (ari) =>
        //    {
        //        mCurrentBGMARI = ari;
        //        var clip = ari.getAsset<AudioClip>(mBGMAudioSource, "backgroundmusic");
        //        mBGMAudioSource.clip = clip;
        //        mBGMAudioSource.loop = true;
        //        mBGMAudioSource.Play();
        //    });
        //}
        //else
        //{
        //    DIYLog.LogError("背景音乐组件为空，无法播放!");
        //}
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public void onPlaySound()
    {
        DIYLog.Log("onPlaySound()");
        AudioManager.Singleton.playSFXSound("sfx1", "explosion");
        //mRMM.requstResource(
        //"sfxtemplate",
        //(abi) =>
        //{
        //    mSFXInstance = abi.instantiateAsset("SFXTemplate");
        //});

        //mRMM.requstResource(
        //"sfx1",
        //(abi) =>
        //{
        //    var ac = abi.getAsset<AudioClip>(mSFXInstance, "explosion");
        //    var audiosource = mSFXInstance.GetComponent<AudioSource>();
        //    audiosource.clip = ac;
        //    audiosource.Play();
        //});

        //// 延迟10秒后删除音效实体对象，测试AB加载管理回收
        //CoroutineManager.Singleton.delayedCall(10.0f, () => { GameObject.Destroy(mSFXInstance); });
    }


    /// <summary>
    /// 加载材质
    /// </summary>
    public void onLoadMaterial()
    {
        DIYLog.Log("onLoadMaterial()");
        var btnloadmat = UIRoot.transform.Find("SecondUICanvas/ButtonGroups/btnLoadMaterial");
        var image = btnloadmat.GetComponent<Image>();
        ResourceManager.Singleton.getShareMaterial(image, "sharematerial", "sharematerial", (arg)=>
        {
            Material mat = arg;
            image.material = mat;
            //延时测试材质回收
            CoroutineManager.Singleton.delayedCall(10.0f, () =>
            {
                Destroy(mat);
            });
        });

    }

    /// <summary>
    /// 加载角色
    /// </summary>
    public void onLoadActorPrefab()
    {
        DIYLog.Log("onLoadActorPrefab()");
        ModelManager.Singleton.getModelInstance("pre_zombunny", (arg) =>
        {
            mActorInstance = arg;
        });
        //mRMM.requstResource(
        //"pre_zombunny",
        //(abi) =>
        //{
        //    mActorInstance = abi.instantiateAsset("pre_Zombunny");
        //});
    }

    /// <summary>
    /// 销毁角色实例对象
    /// </summary>
    public void onDestroyActorInstance()
    {
        DIYLog.Log("onDestroyActorInstance()");
        GameObject.Destroy(mActorInstance);
    }


    /// <summary>
    /// 预加载图集资源
    /// </summary>
    public void onPreloadAtlas()
    {
        DIYLog.Log("onPreloadAtlas()");
        var param1 = InputParam1.text;
        DIYLog.Log("Param1 = " + param1);
        AtlasManager.Singleton.loadAtlas(param1, null, ResourceLoadType.Preload);
        //mRMM.requstResource(
        //param1,
        //(abi) =>
        //{
        //    abi.loadAllAsset<Sprite>();
        //},
        //ResourceLoadType.Preload);
    }


    /// <summary>
    /// 加载常驻Shader
    /// </summary>
    public void onLoadPermanentShaderList()
    {
        DIYLog.Log("onLoadPermanentShaderList()");
        ResourceManager.Singleton.loadAllShader("shaderlist", ()=>
        {


        },
        ResourceLoadType.PermanentLoad);
        //mRMM.requstResource(
        //"shaderlist",
        //(abi) =>
        //{
        //    abi.loadAllAsset<Shader>();
        //},
        //ResourceLoadType.PermanentLoad);          // Shader常驻
    }

    /// <summary>
    /// 预加载Shader变体
    /// </summary>
    public void onPreloadShaderVariants()
    {
        DIYLog.Log("onPreloadShaderVariants()");
        // Shader通过预加载ShaderVariantsCollection里指定的Shader来进行预编译
        ResourceModuleManager.Singleton.requstResource(
       "shaderlist",
       (abi) =>
       {
           var svc = abi.loadAsset<ShaderVariantCollection>("DIYShaderVariantsCollection");
           // Shader通过预加载ShaderVariantsCollection里指定的Shader来进行预编译
           svc.WarmUp();
       },
       ResourceLoadType.PermanentLoad);
    }


    /// <summary>
    /// AB异步加载
    /// </summary>
    public void onAsynABLoad()
    {
        DIYLog.Log("onTestAsynAndSyncABLoad()");
        if (mMainWindow == null)
        {
            onLoadWindowPrefab();
        }

        var image = mMainWindow.transform.Find("imgBG").GetComponent<Image>();
        var param1 = InputParam1.text;
        DIYLog.Log("Param1 = " + param1);
        mRMM.requstResource(
        param1,
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, param1);
            image.sprite = sp;
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);
    }

    /// <summary>
    /// 测试AB异步和同步加载
    /// </summary>
    public void onTestAsynAndSyncABLoad()
    {
        DIYLog.Log("onTestAsynAndSyncABLoad()");
        if(mMainWindow == null)
        {
            onLoadWindowPrefab();
        }

        // 测试大批量异步加载资源后立刻同步加载其中一个该源
        var image = mMainWindow.transform.Find("imgBG").GetComponent<Image>();
        mRMM.requstResource(
        "tutorialcellspritesheet",
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, "TextureShader");
            image.sprite = sp;
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);

        mRMM.requstResource(
        "ambient",
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, "ambient");
            image.sprite = sp;
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);

        mRMM.requstResource(
        "basictexture",
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, "basictexture");
            image.sprite = sp;
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);

        mRMM.requstResource(
        "diffuse",
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, "diffuse");
            image.sprite = sp;
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);

        mRMM.requstResource(
        "pre_zombunny",
        (abi) =>
        {
            mActorInstance = abi.instantiateAsset("pre_Zombunny");
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);

        //测试异步加载后立刻同步加载
        mRMM.requstResource(
        "pre_zombunny",
        (abi) =>
        {
            mActorInstance2 = abi.instantiateAsset("pre_Zombunny");
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Sync);
        DIYLog.Log("actorinstance2.transform.name = " + mActorInstance2.transform.name);

        var btnloadmat = UIRoot.transform.Find("SecondUICanvas/ButtonGroups/btnLoadMaterial");
        Material mat = null;
        mRMM.requstResource(
        "sharematerial",
        (abi) =>
        {
            var matasset = abi.getAsset<Material>(btnloadmat.gameObject, "sharematerial");
            mat = GameObject.Instantiate<Material>(matasset);
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);
        btnloadmat.GetComponent<Image>().material = mat;

        mRMM.requstResource(
        "sfxtemplate",
        (abi) =>
        {
            mSFXInstance = abi.instantiateAsset("SFXTemplate");
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);

        mRMM.requstResource(
        "sfx1",
        (abi) =>
        {
            var ac = abi.getAsset<AudioClip>(mSFXInstance, "explosion");
            var audiosource = mSFXInstance.GetComponent<AudioSource>();
            audiosource.clip = ac;
            audiosource.Play();
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);
    }

    /// <summary>
    /// 销毁异步和同步加载
    /// </summary>
    public void onDestroyAsynAndSyncLoad()
    {
        DIYLog.Log("onDestroyAsynAndSyncLoad()");
        GameObject.Destroy(mMainWindow);
        mMainWindow = null;
        GameObject.Destroy(mActorInstance);
        mActorInstance = null;
        GameObject.Destroy(mActorInstance2);
        mActorInstance2 = null;
        GameObject.Destroy(mSFXInstance);
        mSFXInstance = null;
    }

    /// <summary>
    /// 切换场景
    /// </summary>
    public void onChangeScene()
    {
        DIYLog.Log("onChangeScene()");
        var param1 = InputParam1.text;
        DIYLog.Log("Param1 = " + param1);
        var param2 = InputParam2.text;
        DIYLog.Log("Param2 = " + param2);

        //切换场景前关闭所有打开窗口，测试切场景资源卸载功能
        onDestroyWindowInstance();

        GameSceneManager.Singleton.loadSceneSync(param1);
    }

    /// <summary>
    /// 打印AB依赖信息
    /// </summary>
    public void onPrintABDepInfo()
    {
        DIYLog.Log("onPrintABDepInfo()");
        if(mRMM.CurrentResourceModule is AssetBundleModule)
        {
            (mRMM.CurrentResourceModule as AssetBundleModule).printAllResourceDpInfo();
        }
    }

    /// <summary>
    /// 打印已加载资源信息
    /// </summary>
    public void onPrintLoadedResourceInfo()
    {
        DIYLog.Log("onPrintLoadedResourceInfo()");
        mRMM.CurrentResourceModule.printAllLoadedResourceOwnersAndRefCount();
    }

    /// <summary>
    /// 卸载不再使用的Asset
    /// </summary>
    public void onUnloadUnsedAssets()
    {
        DIYLog.Log("onUnloadUnsedAssets()");
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 切换资源Log开关
    /// </summary>
    public void changeResourceLogSwitch()
    {
        DIYLog.Log("changeResourceLogSwitch()");
        ResourceLogger.LogSwitch = !ResourceLogger.LogSwitch;
    }

    /// <summary>
    /// 打印版本信息
    /// </summary>
    public void printVersionInfo()
    {
        DIYLog.Log("printVersionInfo()");
        VersionConfigModuleManager.Singleton.initVerisonConfigData();
        if(HotUpdateModuleManager.Singleton.ServerVersionConfig != null)
        {
            DIYLog.Log($"服务器版本信息:VersionCode:{HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode} ResourceVersionCode : {HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode}");
        }
        else
        {
            DIYLog.LogError("未获取服务器的版本信息!");
        }
    }

    /// <summary>
    /// 存储最新版本信息
    /// </summary>
    public void saveNewVersionInfo()
    {
        DIYLog.Log("saveNewVersionInfo()");
        var param1 = InputParam1.text;
        DIYLog.Log("Param1 = " + param1);
        var param2 = InputParam2.text;
        DIYLog.Log("Param2 = " + param2);
        double newversioncode = 0.0f;
        int newresourceversioncode = 0;
        if(double.TryParse(param1, out newversioncode))
        {
            if (int.TryParse(param2, out newresourceversioncode))
            {
                VersionConfigModuleManager.Singleton.saveNewVersionCodeOuterConfig(newversioncode);
                VersionConfigModuleManager.Singleton.saveNewResoueceCodeOuterConfig(newresourceversioncode);
            }
            else
            {
                DIYLog.LogError("新资源版本号解析出错！");
            }
        }
        else
        {
            DIYLog.LogError("新版本号解析出错！");
        }
    }

    /// <summary>
    /// 打印所有表格数据
    /// </summary>
    public void printAllExcellData()
    {
        DIYLog.Log("printAllExcellData()");
        var languagelist = GameDataManager.Singleton.t_languagecontainer.getList();
        foreach(var language in languagelist)
        {
            DIYLog.Log("----------------------------------------------");
            DIYLog.Log(string.Format("language id : {0}", language.id));
            DIYLog.Log(string.Format("language content : {0}", language.content));
        }
        var authorlist = GameDataManager.Singleton.t_author_Infocontainer.getList();
        foreach (var author in authorlist)
        {
            DIYLog.Log("----------------------------------------------");
            DIYLog.Log(string.Format("author id : {0}", author.id));
            DIYLog.Log(string.Format("author author : {0}", author.author));
            DIYLog.Log(string.Format("author age : {0}", author.age));
            DIYLog.Log(string.Format("author national : {0}", author.national));
            DIYLog.Log(string.Format("author sex : {0}", author.sex));
        }
        var globallist = GameDataManager.Singleton.t_globalcontainer.getList();
        foreach (var global in globallist)
        {
            DIYLog.Log("----------------------------------------------");
            DIYLog.Log(string.Format("global id : {0}", global.id));
            DIYLog.Log(string.Format("global intvalue : {0}", global.intvalue));
            DIYLog.Log(string.Format("global stringvalue : {0}", global.stringvalue));
            DIYLog.Log(string.Format("global floatvalue : {0}", global.floatvalue));
            DIYLog.Log(string.Format("global intarrayvalue : {0}", global.intarrayvalue.ToString()));
            DIYLog.Log(string.Format("global stringarrayvalue : {0}", global.stringarrayvalue.ToString()));
        }
    }

    /// <summary>
    /// 打印所有AB路径信息
    /// </summary>
    public void printAllABPath()
    {
        DIYLog.Log("printAllABPath()");
        AssetBundlePath.PrintAllPathInfo();
    }

    /// <summary>
    /// 调用原生方法
    /// </summary>
    public void onCallNativeMethodClick()
    {
        Debug.Log("onCallNativeMethodClick()");
        NativeManager.Singleton.callNativeMethod();
    }

    /// <summary>
    /// 获取服务器版本信息
    /// </summary>
    public void obtainServerVersionConfig()
    {
        DIYLog.Log("obtainServerVersionConfig()");
        HotUpdateModuleManager.Singleton.doObtainServerVersionConfig(serverVersionConfigHotUpdateCompleteCallBack);
    }

    /// <summary>
    /// 版本强更测试
    /// </summary>
    public void testVersionwHotUpdate()
    {
        DIYLog.Log("testVersionwHotUpdate()");
        if(HotUpdateModuleManager.Singleton.ServerVersionConfig != null)
        {
            if (HotUpdateModuleManager.Singleton.checkVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode))
            {
                HotUpdateModuleManager.Singleton.doNewVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode, versionHotUpdateCompleteCallBack);
            }
        }
        else
        {
            DIYLog.LogError("版本强更前，请先获取服务器版本信息!");
        }
    }

    /// <summary>
    /// 资源热更测试
    /// </summary>
    public void testResourceHotUpdate()
    {
        DIYLog.Log("testResourceHotUpdate()");
        if (HotUpdateModuleManager.Singleton.ServerVersionConfig != null)
        {
            if (HotUpdateModuleManager.Singleton.checkResourceHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode))
            {
                HotUpdateModuleManager.Singleton.doResourceHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode, resourceHotUpdateCompleteCallBack);
            }
        }
        else
        {
            DIYLog.LogError("资源热更新前，请先获取服务器版本信息!");
        }
    }
    
    /// <summary>
    /// 获取服务器版本信息回调
    /// </summary>
    /// <param name="result"></param>
    private void serverVersionConfigHotUpdateCompleteCallBack(bool result)
    {
        DIYLog.Log(string.Format("获取服务器版本结果 result : {0}", result));
    }

    /// <summary>
    /// 版本强更完成回调
    /// </summary>
    /// <param name="result">版本强更结果</param>
    private void versionHotUpdateCompleteCallBack(bool result)
    {
        DIYLog.Log(string.Format("版本强更结果 result : {0}", result));
    }

    /// <summary>
    /// 资源热更完成回调
    /// </summary>
    /// <param name="result">资源热更结果</param>
    private void resourceHotUpdateCompleteCallBack(bool result)
    {
        DIYLog.Log(string.Format("资源热更结果 result : {0}", result));
    }
    
    /// <summary>
    /// 测试热更新完整流程
    /// </summary>
    public void testHotUpdateFullWorkFlow()
    {
        DIYLog.Log("testHotUpdateFullWorkFlow()");
        VersionConfigModuleManager.Singleton.initVerisonConfigData();
        //检测是否强更过版本
        HotUpdateModuleManager.Singleton.checkHasVersionHotUpdate();
        //TODO:
        //拉去服务器列表信息(网络那一套待开发,暂时用本地默认数值测试)
        if(HotUpdateModuleManager.Singleton.checkVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode))
        {
            HotUpdateModuleManager.Singleton.doNewVersionHotUpdate(
                HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode, 
                (versionhotupdateresult) =>
                {
                    if (versionhotupdateresult)
                    {
                        DIYLog.Log("版本强更完成!触发自动安装！");
#if UNITY_ANDROID
                        (NativeManager.Singleton as AndroidNativeManager).installAPK(HotUpdateModuleManager.Singleton.VersionHotUpdateCacheFilePath);
#endif
                        return;
                    }
                    else
                    {
                        resourceHotUpdate();
                    }
                }
            );
        }
        else
        {
            resourceHotUpdate();
        }
    }

    private void resourceHotUpdate()
    {
        //不需要强更走后判定资源热更流程
        if (HotUpdateModuleManager.Singleton.checkResourceHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode))
        {
            //单独开启一个携程打印强更进度
            StartCoroutine(printVersionHotUpdateProgressCoroutine());
            HotUpdateModuleManager.Singleton.doResourceHotUpdate(
                HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode,
                (resourcehotupdateresult) =>
                {
                    if (resourcehotupdateresult)
                    {
                        DIYLog.Log("资源热更完成!请重进或重新触发热更流程！");
                        return;
                    }
                    else
                    {
                        DIYLog.Log("资源热更出错!");
                        return;
                    }
                }
            );
        }
        else
        {
            DIYLog.Log("无需资源热更，可以直接进入游戏！");
        }
    }

    private IEnumerator printVersionHotUpdateProgressCoroutine()
    {
        Debug.Log("printVersionHotUpdateProgressCoroutine()");
        while(HotUpdateModuleManager.Singleton.HotVersionUpdateRequest.TWRequestStatus == TWebRequest.TWebRequestStatus.TW_In_Progress)
        {
            yield return new WaitForSeconds(1.0f);
            Debug.Log(string.Format("当前版本热更进度 : {0}", HotUpdateModuleManager.Singleton.HotResourceUpdateProgress));
        }
    }

    /// <summary>
    /// 尝试进游戏(验证版本强更以及资源热更相关判定)
    /// </summary>
    public void tryEnterGame()
    {
        DIYLog.Log("tryEnterGame()");
        VersionConfigModuleManager.Singleton.initVerisonConfigData();
        if (HotUpdateModuleManager.Singleton.HotUpdateSwitch)
        {
            if (VersionConfigModuleManager.Singleton.needVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode))
            {
                DIYLog.Log(string.Format("服务器版本号 : {0}高于本地版本号 : {1}，需要强更！", HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode, VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode));
                DIYLog.Log("不允许进游戏！");
            }
            else
            {
                DIYLog.Log(string.Format("服务器版本号 : {0}小于或等于本地版本号 : {1}，不需要强更！", HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode, VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode));
                if (VersionConfigModuleManager.Singleton.needResourceHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode))
                {
                    DIYLog.Log(string.Format("服务器资源版本号 : {0}大于本地资源版本号 : {1}，需要资源热更！", HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode, VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode));
                    DIYLog.Log("不允许进游戏！");
                }
                else
                {
                    DIYLog.Log(string.Format("服务器资源版本号 : {0}小于或等于本地资源版本号 : {1}，不需要资源热更！", HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode, VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode));
                    DIYLog.Log("可以进游戏!");
                }
            }
        }
        else
        {
            DIYLog.Log("热更开关未打开，不允许热更！");
            DIYLog.Log("可以进游戏!");
        }
    }

    /// <summary>
    /// 强制卸载指定AB
    /// </summary>
    public void forceUnloadSpecificAB()
    {
        DIYLog.Log("forceUnloadSpecificAB()");
        if(ResourceModuleManager.Singleton.CurrentResourceModule.ResLoadMode == ResourceLoadMode.AssetBundle)
        {
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var assetbundleresourcemodule = ResourceModuleManager.Singleton.CurrentResourceModule as AssetBundleModule;
            assetbundleresourcemodule.forceUnloadSpecificResource(param1);

        }
        else
        {
            DIYLog.Log("未处于AB状态，无法卸载指定AB!");
        }
    }
}
