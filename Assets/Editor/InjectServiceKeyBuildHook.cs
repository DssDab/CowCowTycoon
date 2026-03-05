#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 포트폴리오 제출용: "PC 빌드에서만" 서비스키를 주입하고, 빌드 후 원복.
/// - 모바일(Android/iOS) 빌드는 주입을 완전히 스킵(=키 없이 fallback 정책)
/// - 레포에는 NetworkConfig.asset의 serviceKey를 항상 ""로 유지 권장
/// - 키는 빌드 머신의 환경변수로만 주입: COWCOW_EKAPE_KEY
/// </summary>
[InitializeOnLoad]
public sealed class InjectServiceKeyBuildHook : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    private const string ENV_NAME = "COWCOW_EKAPE_KEY";

    // SessionState(휘발성) — EditorPrefs(영구 저장) 사용 안 함
    private const string SS_INJECTED_FLAG = "COWCOW__INJECTED";
    private const string SS_BACKUP_VALUE = "COWCOW__BACKUP";

    // NetworkConfig 로딩: GUID 고정(권장). 아래 GUID를 네 NetworkConfig.asset의 GUID로 교체.
    private const string NETWORK_CONFIG_GUID = "c3c31a4f14b3bdf4fb2283af5dc5f554";

    static InjectServiceKeyBuildHook()
    {
        // 에디터 재시작/도메인 리로드 시 “남아있는 주입”이 있으면 정리
        CleanupIfLeftInjected();
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        // PC(Standalone) 빌드에서만 주입
        if (!IsPcStandalone(report.summary.platform))
        {
            // 모바일/그 외 플랫폼: 주입 금지. (혹시 남아있으면 정리)
            CleanupIfLeftInjected();
            return;
        }

        var cfg = LoadNetworkConfigOrThrow();

        // (권장) 빌드 전 serviceKey가 남아있으면 사고 방지 차원에서 막아버리기
        if (!string.IsNullOrEmpty(cfg.serviceKey))
            throw new BuildFailedException("NetworkConfig.serviceKey must be EMPTY in repo. Clear it and inject via ENV only.");

        var envKey = Environment.GetEnvironmentVariable(ENV_NAME);
        if (string.IsNullOrEmpty(envKey))
        {
            Debug.LogWarning($"[{nameof(InjectServiceKeyBuildHook)}] ENV {ENV_NAME} is empty. PC build proceeds with NO key (fallback).");
            return;
        }

        // 이전 잔재 정리 후 주입
        CleanupIfLeftInjected();

        SessionState.SetString(SS_BACKUP_VALUE, cfg.serviceKey ?? "");
        SessionState.SetBool(SS_INJECTED_FLAG, true);

        cfg.serviceKey = envKey;
        EditorUtility.SetDirty(cfg);
        AssetDatabase.SaveAssets();

        Debug.Log($"[{nameof(InjectServiceKeyBuildHook)}] Injected serviceKey for PC build.");
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        // PC 빌드에서만 원복(주입도 PC에서만 하니까, 사실상 여기서만 돌아도 됨)
        if (IsPcStandalone(report.summary.platform))
            CleanupIfLeftInjected();
    }

    private static bool IsPcStandalone(BuildTarget target)
    {
        // “PC 빌드” 범위를 어디까지로 볼지 선택:
        // - Windows/macOS/Linux Standalone 전부 포함하는 게 일반적인 PC 범위.
        return target == BuildTarget.StandaloneWindows
            || target == BuildTarget.StandaloneWindows64
            || target == BuildTarget.StandaloneOSX
            || target == BuildTarget.StandaloneLinux64;
    }

    private static void CleanupIfLeftInjected()
    {
        if (!SessionState.GetBool(SS_INJECTED_FLAG, false))
            return;

        var cfg = TryLoadNetworkConfig();
        if (cfg != null)
        {
            var backup = SessionState.GetString(SS_BACKUP_VALUE, "");
            cfg.serviceKey = backup ?? "";

            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();

            Debug.Log($"[{nameof(InjectServiceKeyBuildHook)}] Reverted serviceKey after build/cleanup.");
        }

        SessionState.EraseBool(SS_INJECTED_FLAG);
        SessionState.EraseString(SS_BACKUP_VALUE);
    }

    private static NetworkConfig LoadNetworkConfigOrThrow()
    {
        var cfg = TryLoadNetworkConfig();
        if (cfg == null)
            throw new BuildFailedException("NetworkConfig asset not found. Check NETWORK_CONFIG_GUID.");
        return cfg;
    }

    private static NetworkConfig TryLoadNetworkConfig()
    {
        if (string.IsNullOrEmpty(NETWORK_CONFIG_GUID) ||
            NETWORK_CONFIG_GUID == "PUT_YOUR_NETWORKCONFIG_ASSET_GUID_HERE")
            return null;

        var path = AssetDatabase.GUIDToAssetPath(NETWORK_CONFIG_GUID);
        if (string.IsNullOrEmpty(path))
            return null;

        return AssetDatabase.LoadAssetAtPath<NetworkConfig>(path);
    }
}
#endif