/*
 * Description:             游戏入口
 * Author:                  tanghuan
 * Create Date:             2018/03/12
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    /// 窗口实例对象
    /// </summary>
    private GameObject mMainWindow;

    /// <summary>
    /// 角色实例对象
    /// </summary>
    private GameObject mActorInstance;

    /// <summary>
    /// 音效临时实例对象
    /// </summary>
    private GameObject mSFXInstance;

    /// <summary>
    /// 当前场景ABI，用于手动释放引用
    /// </summary>
    private AssetBundleInfo mCurrentSceneABI;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        DontDestroyOnLoad(UIRoot);

        addMonoComponents();

        nativeInitilization();

        registerModules();

        initilization();
    }

    private void Start () {

    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= VisibleLogUtility.getInstance().HandleLog;

        unregisterModules();
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

        var rmm = gameObject.AddComponent<ResourceModuleManager>();
        rmm.setInstance(rmm);
    }

    /// <summary>
    /// 原生初始化
    /// </summary>
    private void nativeInitilization()
    {

    }

    /// <summary>
    /// 注册模块
    /// </summary>
    private void registerModules()
    {
        ModuleManager.Singleton.registerModule<ResourceModuleManager>(ResourceModuleManager.getInstance());
        ModuleManager.Singleton.registerModule<ResourceManager>(ResourceManager.Singleton);
        ModuleManager.Singleton.registerModule<GameSceneManager>(GameSceneManager.Singleton);
        ModuleManager.Singleton.registerModule<WindowManager>(WindowManager.Singleton);
        ModuleManager.Singleton.registerModule<AtlasManager>(AtlasManager.Singleton);
        ModuleManager.Singleton.registerModule<AudioManager>(AudioManager.Singleton);
        ModuleManager.Singleton.registerModule<ModuleManager>(ModuleManager.Singleton);
        ModuleManager.Singleton.registerModule<EffectManager>(EffectManager.Singleton);
        ModuleManager.Singleton.registerModule<SharedTextureManager>(SharedTextureManager.Singleton);
        ModuleManager.Singleton.registerModule<SharedMaterialManager>(SharedMaterialManager.Singleton);
    }

    /// <summary>
    /// 取消模块注册
    /// </summary>
    private void unregisterModules()
    {
        // 取消所有注册模块
        ModuleManager.Singleton.unregisterAllModule();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void initilization()
    {
        // 初始化资源模块
        ModuleManager.Singleton.getModule<ResourceModuleManager>().init();
        // 预加载Shader
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("shaderlist",
        (abi) =>
        {
            abi.loadAllAsset<UnityEngine.Object>();
        },
        ABLoadType.PermanentLoad);          // Shader常驻
        ModuleManager.Singleton.getModule<ResourceModuleManager>().startUnsedABRecyclingTask();

        // 初始化逻辑层Manager
        ModuleManager.Singleton.getModule<GameSceneManager>().init();
    }
    
    /// <summary>
    /// 加载窗口预制件
    /// </summary>
    public void onLoadWindowPrefab()
    {
        Debug.Log("onLoadWindowPrefab()");
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("MainWindow",
        (abi) =>
        {
            mMainWindow = abi.instantiateAsset("MainWindow");
            mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
        });
    }

    /// <summary>
    /// 销毁窗口实例对象
    /// </summary>
    public void onDestroyWindowInstance()
    {
        Debug.Log("onDestroyWindowInstance()");
        GameObject.Destroy(mMainWindow);
    }

    /// <summary>
    /// 加载Sprite
    /// </summary>
    public void onLoadSprite()
    {
        Debug.Log("onLoadSprite()");
        var param1 = InputParam1.text;
        Debug.Log("Param1 = " + param1);
        var param2 = InputParam2.text;
        Debug.Log("Param2 = " + param2);
        var image = mMainWindow.transform.Find("imgBG").GetComponent<Image>();
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource(param1,
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, param2);
            image.sprite = sp;
        });
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public void onPlaySound()
    {
        Debug.Log("onPlaySound()");
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("SFXTemplate",
        (abi) =>
        {
            mSFXInstance = abi.instantiateAsset("SFXTemplate");
        });

        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("sfx1",
        (abi) =>
        {
            var ac = abi.getAsset<AudioClip>(mSFXInstance, "explosion");
            var audiosource = mSFXInstance.GetComponent<AudioSource>();
            audiosource.clip = ac;
            audiosource.Play();
        });

        // 延迟10秒后删除音效实体对象，测试AB加载管理回收
        CoroutineManager.Singleton.delayedCall(10.0f, () => { GameObject.Destroy(mSFXInstance); });
    }


    /// <summary>
    /// 加载材质
    /// </summary>
    public void onLoadMaterial()
    {
        Debug.Log("onLoadMaterial()");
        var btnloadmat = UIRoot.transform.Find("SecondUICanvas/ButtonGroups/btnLoadMaterial");
        Material mat = null;
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("sharematerial",
        (abi) =>
        {
            var matasset = abi.getAsset<Material>(btnloadmat.gameObject, "sharematerial");
            mat = GameObject.Instantiate<Material>(matasset);
        });
        btnloadmat.GetComponent<Image>().material = mat;

        //延时测试材质回收
        CoroutineManager.Singleton.delayedCall(10.0f, () =>
        {
            Destroy(mat);
        });
    }

    /// <summary>
    /// 加载角色
    /// </summary>
    public void onLoadActorPrefab()
    {
        Debug.Log("onLoadActorPrefab()");
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("pre_Zombunny",
        (abi) =>
        {
            mActorInstance = abi.instantiateAsset("pre_Zombunny");
        });  
    }

    /// <summary>
    /// 销毁角色实例对象
    /// </summary>
    public void onDestroyActorInstance()
    {
        Debug.Log("onDestroyActorInstance()");
        GameObject.Destroy(mActorInstance);
    }


    /// <summary>
    /// 预加载图集资源
    /// </summary>
    public void onPreloadAtlas()
    {
        Debug.Log("onPreloadAtlas()");
        var param1 = InputParam1.text;
        Debug.Log("Param1 = " + param1);
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource(param1,
        (abi) =>
        {
            abi.loadAllAsset<Sprite>();
        },
        ABLoadType.Preload);
    }


    /// <summary>
    /// 加载常驻Shader
    /// </summary>
    public void onLoadPermanentShaderList()
    {
        Debug.Log("onLoadPermanentShaderList()");
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("shaderlist",
        (abi) =>
        {

        },
        ABLoadType.PermanentLoad);          // Shader常驻
    }


    /// <summary>
    /// AB异步加载
    /// </summary>
    public void onAsynABLoad()
    {
        Debug.Log("onTestAsynAndSyncABLoad()");
        if (mMainWindow == null)
        {
            onLoadWindowPrefab();
        }

        var image = mMainWindow.transform.Find("imgBG").GetComponent<Image>();
        var param1 = InputParam1.text;
        Debug.Log("Param1 = " + param1);
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource(param1,
          (abi) =>
          {
              var sp = abi.getAsset<Sprite>(image, param1);
              image.sprite = sp;
          },
          ABLoadType.NormalLoad,
          ABLoadMethod.Async);
    }

    /// <summary>
    /// 测试AB异步和同步加载
    /// </summary>
    public void onTestAsynAndSyncABLoad()
    {
        Debug.Log("onTestAsynAndSyncABLoad()");
        if(mMainWindow == null)
        {
            onLoadWindowPrefab();
        }

        // 测试大批量异步加载资源后立刻同步加载其中一个该源
        var image = mMainWindow.transform.Find("imgBG").GetComponent<Image>();
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("tutorialcellspritesheet",
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, "TextureShader");
            image.sprite = sp;
        },
        ABLoadType.NormalLoad,
        ABLoadMethod.Async);

        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("Ambient",
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, "Ambient");
            image.sprite = sp;
        },
        ABLoadType.NormalLoad,
        ABLoadMethod.Async);

        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("BasicTexture",
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, "BasicTexture");
            image.sprite = sp;
        },
        ABLoadType.NormalLoad,
        ABLoadMethod.Async);

        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("Diffuse",
        (abi) =>
        {
            var sp = abi.getAsset<Sprite>(image, "Diffuse");
            image.sprite = sp;
        },
        ABLoadType.NormalLoad,
        ABLoadMethod.Async);

        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("pre_Zombunny",
        (abi) =>
        {
            mActorInstance = abi.instantiateAsset("pre_Zombunny");
        },
        ABLoadType.NormalLoad,
        ABLoadMethod.Async);

        //测试异步加载后立刻同步加载
        GameObject actorinstance2 = null;
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("pre_Zombunny",
        (abi) =>
        {
            actorinstance2 = abi.instantiateAsset("pre_Zombunny");
        },
        ABLoadType.NormalLoad,
        ABLoadMethod.Sync);
        Debug.Log("actorinstance2.transform.name = " + actorinstance2.transform.name);

        var btnloadmat = UIRoot.transform.Find("SecondUICanvas/ButtonGroups/btnLoadMaterial");
        Material mat = null;
        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("sharematerial",
        (abi) =>
        {
            var matasset = abi.getAsset<Material>(btnloadmat.gameObject, "sharematerial");
            mat = GameObject.Instantiate<Material>(matasset);
        },
        ABLoadType.NormalLoad,
        ABLoadMethod.Async);
        btnloadmat.GetComponent<Image>().material = mat;

        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("SFXTemplate",
        (abi) =>
        {
            mSFXInstance = abi.instantiateAsset("SFXTemplate");
        },
        ABLoadType.NormalLoad,
        ABLoadMethod.Async);

        ModuleManager.Singleton.getModule<ResourceModuleManager>().requstResource("sfx1",
        (abi) =>
        {
            var ac = abi.getAsset<AudioClip>(mSFXInstance, "explosion");
            var audiosource = mSFXInstance.GetComponent<AudioSource>();
            audiosource.clip = ac;
            audiosource.Play();
        },
        ABLoadType.NormalLoad,
        ABLoadMethod.Async);
    }

    /// <summary>
    /// 切换场景
    /// </summary>
    public void onChangeScene()
    {
        Debug.Log("onChangeScene()");
        var param1 = InputParam1.text;
        Debug.Log("Param1 = " + param1);
        var param2 = InputParam2.text;
        Debug.Log("Param2 = " + param2);

        //切换场景前关闭所有打开窗口，测试切场景资源卸载功能
        onDestroyWindowInstance();

        GameSceneManager.Singleton.loadSceneSync(param1);
    }

    /// <summary>
    /// 打印AB依赖信息
    /// </summary>
    public void onPrintABDepInfo()
    {
        Debug.Log("onPrintABDepInfo()");
        ModuleManager.Singleton.getModule<ResourceModuleManager>().printAllABDpInfo();
    }

    /// <summary>
    /// 打印已加载AB信息
    /// </summary>
    public void onPrintLoadedABInfo()
    {
        Debug.Log("onPrintLoadedABInfo()");
        ModuleManager.Singleton.getModule<ResourceModuleManager>().printAllLoadedABOwnersAndRefCount();
    }
}
