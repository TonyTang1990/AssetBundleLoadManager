/*
 * Description:             BuildWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/27
 */

using System;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 打包窗口配置数据。
/// 使用ScriptableSingleton将配置序列化到ProjectSettings中，确保关闭并再次打开窗口时保留上次参数。
/// </summary>
[FilePath("ProjectSettings/BuildWindowConfig.asset", FilePathAttribute.Location.ProjectFolder)]
internal sealed class BuildWindowConfig : ScriptableSingleton<BuildWindowConfig>
{
    /// <summary>
    /// 打包平台。
    /// </summary>
    public BuildTarget BuildTarget = BuildTarget.Android;

    /// <summary>
    /// 打包渠道。
    /// </summary>
    public Channel Channel = Channel.None;

    /// <summary>
    /// 是否生成开发版本。
    /// </summary>
    public bool IsDevelopment;

    /// <summary>
    /// 应用版本号。
    /// </summary>
    public double VersionCode = 1.00d;

    /// <summary>
    /// 资源版本号。
    /// </summary>
    public int ResourceVersionCode = 1;

    /// <summary>
    /// 是否完全重新打包AssetBundle。
    /// </summary>
    public bool ForceRebuildAB = true;

    /// <summary>
    /// 将当前配置保存到ProjectSettings目录。
    /// </summary>
    public void SaveConfig()
    {
        Save(true);
    }
}

/// <summary>
/// 本地打包编辑器窗口。
/// 负责编辑、校验、确认打包参数，并调用BuildTool执行本地打包。
/// </summary>
/// <remarks>
/// 通过菜单Build/打包窗口打开。
/// </remarks>
public sealed class BuildWindow : EditorWindow
{
    /// <summary>
    /// 窗口最小宽度，确保属性名称和中文说明能够完整显示。
    /// </summary>
    private const float MinimumWindowWidth = 820f;

    /// <summary>
    /// 窗口最小高度。
    /// </summary>
    private const float MinimumWindowHeight = 460f;

    /// <summary>
    /// 参数标签宽度，给属性名称和中文说明预留足够空间。
    /// </summary>
    private const float ParameterLabelWidth = 380f;

    /// <summary>
    /// 当前窗口的滚动位置。
    /// </summary>
    private Vector2 mScrollPosition;

    /// <summary>
    /// 当前是否正在执行打包，用于防止重复点击打包按钮。
    /// </summary>
    private bool mIsBuilding;

    /// <summary>
    /// 持久化的打包窗口配置。
    /// </summary>
    private BuildWindowConfig Config => BuildWindowConfig.instance;

    /// <summary>
    /// 从Unity菜单打开打包窗口。
    /// </summary>
    [MenuItem("Build/打包窗口")]
    public static void OpenWindow()
    {
        var window = GetWindow<BuildWindow>("打包窗口");
        window.minSize = new Vector2(MinimumWindowWidth, MinimumWindowHeight);

        // 已存在的窗口可能仍使用旧尺寸，打开时主动扩宽以保证参数说明完整显示。
        if (window.position.width < MinimumWindowWidth)
        {
            var windowPosition = window.position;
            windowPosition.width = MinimumWindowWidth;
            window.position = windowPosition;
        }

        window.Show();
    }

    /// <summary>
    /// 窗口关闭或Unity重载脚本前保存当前配置。
    /// </summary>
    private void OnDisable()
    {
        Config.SaveConfig();
    }

    /// <summary>
    /// 绘制打包窗口界面。
    /// </summary>
    private void OnGUI()
    {
        mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("打包参数", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("请配置本地打包参数。字段名称后括号内为参数说明。", MessageType.Info);
        EditorGUILayout.Space(4f);

        using (new EditorGUI.DisabledScope(mIsBuilding))
        {
            // 修改任意参数后立即保存，即使校验失败或Unity异常退出也尽量保留最新输入。
            EditorGUI.BeginChangeCheck();
            DrawBuildParameters();
            if (EditorGUI.EndChangeCheck())
            {
                Config.SaveConfig();
            }

            EditorGUILayout.Space(12f);
            if (GUILayout.Button(mIsBuilding ? "正在打包..." : "开始打包", GUILayout.Height(36f)))
            {
                StartBuild();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制BuildParameters中的全部属性配置项。
    /// </summary>
    private void DrawBuildParameters()
    {
        // 临时扩大标签区域，绘制完成后恢复全局宽度，避免影响其他Editor窗口。
        var originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = ParameterLabelWidth;
        try
        {
            Config.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(
                new GUIContent("BuildTarget（打包平台）", "打包平台"), Config.BuildTarget);
            Config.Channel = (Channel)EditorGUILayout.EnumPopup(
                new GUIContent("Channel（打包渠道）", "打包渠道"), Config.Channel);
            Config.IsDevelopment = EditorGUILayout.Toggle(
                new GUIContent("IsDevelopment（是否打开发版本）", "是否打开发版本"), Config.IsDevelopment);
            Config.VersionCode = EditorGUILayout.DoubleField(
                new GUIContent("VersionCode（版本号，格式：*.**且不小于1）", "例如1.00、2.35"), Config.VersionCode);
            Config.ResourceVersionCode = EditorGUILayout.IntField(
                new GUIContent($"ResourceVersionCode（资源版本号，范围：1~{VersionEditorUtilities.MaxResourceVersionCode}）",
                    "资源版本号"), Config.ResourceVersionCode);
            Config.ForceRebuildAB = EditorGUILayout.Toggle(
                new GUIContent("ForceRebuildAB（是否完全重新打包AB）", "是否完全重新打包AssetBundle"),
                Config.ForceRebuildAB);
        }
        finally
        {
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }
    }

    /// <summary>
    /// 校验参数、显示二次确认窗口，并在用户确认后执行本地打包。
    /// </summary>
    private void StartBuild()
    {
        // 点击打包时再次保存，保证此次尝试使用的参数能够在下次打开窗口时恢复。
        Config.SaveConfig();
        if (!TryCreateBuildParameters(out var buildParameters, out var errorMessage))
        {
            EditorUtility.DisplayDialog("打包参数格式错误", errorMessage, "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认打包参数", BuildConfirmationMessage(buildParameters), "确认打包", "取消"))
        {
            return;
        }

        mIsBuilding = true;
        Repaint();
        try
        {
            var buildResult = BuildTool.DoBuild(buildParameters);
            var resultMessage = buildResult == BuildResult.Success
                ? "打包成功！"
                : $"打包失败！\n\n打包结果：{buildResult}";
            EditorUtility.DisplayDialog("打包结果", resultMessage, "确定");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("打包结果", $"打包过程中发生异常：\n\n{exception.Message}", "确定");
        }
        finally
        {
            mIsBuilding = false;
            Repaint();
        }
    }

    /// <summary>
    /// 校验持久化配置并创建BuildParameters对象。
    /// </summary>
    /// <param name="buildParameters">校验成功后创建的打包参数。</param>
    /// <param name="errorMessage">校验失败时的详细错误信息。</param>
    /// <returns>所有参数有效时返回true，否则返回false。</returns>
    private bool TryCreateBuildParameters(out BuildParameters buildParameters, out string errorMessage)
    {
        buildParameters = null;
        var errors = new StringBuilder();

        if (!IsSupportedBuildTarget(Config.BuildTarget))
        {
            errors.AppendLine($"- 打包平台不受支持：{Config.BuildTarget}。仅支持Android、iOS、StandaloneWindows和StandaloneWindows64。");
        }

        // 统一复用版本工具中的格式化规则，并使用其返回值判断版本格式是否有效。
        (var versionCodeFormatResult, var finalVersionCode) = VersionEditorUtilities.FormatVersionCode(Config.VersionCode);
        if (!versionCodeFormatResult)
        {
            errors.AppendLine($"- 版本号“{Config.VersionCode}”格式错误，要求格式为*.**，例如1.00、2.35。");
        }
        else if (!VersionEditorUtilities.IsValideVersionCode(finalVersionCode))
        {
            errors.AppendLine($"- 版本号“{Config.VersionCode}”无效，数值必须大于等于1。");
        }

        if (!VersionEditorUtilities.IsValideResourceVersionCode(Config.ResourceVersionCode))
        {
            errors.AppendLine($"- 资源版本号“{Config.ResourceVersionCode}”无效，要求范围为1~{VersionEditorUtilities.MaxResourceVersionCode}。");
        }

        if (errors.Length > 0)
        {
            errorMessage = errors.ToString().TrimEnd();
            return false;
        }

        buildParameters = new BuildParameters(Config.BuildTarget, Config.Channel, Config.IsDevelopment,
                                              finalVersionCode, Config.ResourceVersionCode, Config.ForceRebuildAB);
        errorMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// 根据打包参数创建二次确认窗口的详细文本。
    /// </summary>
    /// <param name="buildParameters">即将执行的打包参数。</param>
    /// <returns>包含全部打包参数的确认文本。</returns>
    private static string BuildConfirmationMessage(BuildParameters buildParameters)
    {
        var message = new StringBuilder();
        message.AppendLine("请确认以下打包参数：");
        message.AppendLine();
        message.AppendLine($"打包平台：{buildParameters.BuildTarget}");
        message.AppendLine($"打包渠道：{buildParameters.Channel}");
        message.AppendLine($"开发版本：{buildParameters.IsDevelopment}");
        message.AppendLine($"版本号：{buildParameters.VersionCode.ToString("F2", CultureInfo.InvariantCulture)}");
        message.AppendLine($"资源版本号：{buildParameters.ResourceVersionCode}");
        message.AppendLine($"完全重建AB：{buildParameters.ForceRebuildAB}");
        message.AppendLine();
        message.Append("确认后将立即开始本地打包。");
        return message.ToString();
    }

    /// <summary>
    /// 判断BuildTool是否支持指定打包平台。
    /// </summary>
    /// <param name="buildTarget">待检查的打包平台。</param>
    /// <returns>平台受支持时返回true。</returns>
    private static bool IsSupportedBuildTarget(BuildTarget buildTarget)
    {
        return buildTarget == BuildTarget.Android ||
               buildTarget == BuildTarget.iOS ||
               buildTarget == BuildTarget.StandaloneWindows ||
               buildTarget == BuildTarget.StandaloneWindows64;
    }
}
