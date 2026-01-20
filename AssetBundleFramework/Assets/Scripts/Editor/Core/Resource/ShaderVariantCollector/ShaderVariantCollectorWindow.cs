#if UNITY_2019_4_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using YooAsset.Editor;
using TResource;

public class ShaderVariantCollectorWindow : EditorWindow
{
    [MenuItem("Tools/着色器变种收集器", false, 100)]
    public static void OpenWindow()
    {
        ShaderVariantCollectorWindow window = GetWindow<ShaderVariantCollectorWindow>("着色器变种收集工具", true);
        window.minSize = new Vector2(800, 600);
    }

    private Button _collectButton;
    private TextField _collectOutputField;
    private Label _currentShaderCountField;
    private Label _currentVariantCountField;
    private SliderInt _processCapacitySlider;
    private PopupField<string> _packageField;

    public void CreateGUI()
    {
        try
        {
            VisualElement root = this.rootVisualElement;

            // 加载布局文件
            var visualAsset = UxmlLoader.LoadWindowUXML<ShaderVariantCollectorWindow>();
            if (visualAsset == null)
                return;

            visualAsset.CloneTree(root);

            // 文件输出目录
            _collectOutputField = root.Q<TextField>("CollectOutput");
            _collectOutputField.SetValueWithoutNotify(ResourceConstData.ShaderVariantsAssetRelativePath);
            _collectOutputField.RegisterValueChangedCallback(evt =>
            {
                //不允许修改ShaderVariantCollection的存储路径
                _collectOutputField.SetValueWithoutNotify(ResourceConstData.ShaderVariantsAssetRelativePath);
                //ShaderVariantCollectorSetting.SetFileSavePath(_currentPackageName, _collectOutputField.value);
            });
            // 容器值
            _processCapacitySlider = root.Q<SliderInt>("ProcessCapacity");
            _processCapacitySlider.SetValueWithoutNotify(ShaderVariantCollectorSetting.GeProcessCapacity());
#if !UNITY_2020_3_OR_NEWER
            _processCapacitySlider.label = $"Capacity ({_processCapacitySlider.value})";
            _processCapacitySlider.RegisterValueChangedCallback(evt =>
            {
                ShaderVariantCollectorSetting.SetProcessCapacity(_processCapacitySlider.value);
                _processCapacitySlider.label = $"Capacity ({_processCapacitySlider.value})";
            });
#else
            _processCapacitySlider.RegisterValueChangedCallback(evt =>
            {
                ShaderVariantCollectorSetting.SetProcessCapacity(_processCapacitySlider.value);
            });
#endif

            _currentShaderCountField = root.Q<Label>("CurrentShaderCount");
            _currentVariantCountField = root.Q<Label>("CurrentVariantCount");

            // 变种收集按钮
            _collectButton = root.Q<Button>("CollectButton");
            _collectButton.clicked += CollectButton_clicked;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    private void Update()
    {
        if (_currentShaderCountField != null)
        {
            int currentShaderCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
            _currentShaderCountField.text = $"Current Shader Count : {currentShaderCount}";
        }

        if (_currentVariantCountField != null)
        {
            int currentVariantCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();
            _currentVariantCountField.text = $"Current Variant Count : {currentVariantCount}";
        }
    }

    private void CollectButton_clicked()
    {
        string savePath = ResourceConstData.ShaderVariantsAssetRelativePath;
        int processCapacity = _processCapacitySlider.value;
        ShaderVariantCollector.Run(savePath, processCapacity, null);
    }
}
#endif