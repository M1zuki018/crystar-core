using System;
using System.Collections.Generic;
using System.Linq;
using CryStar.Attribute;
using CryStar.Utility;
using CryStar.Utility.Enum;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CryStar.Core
{
    /// <summary>
    /// シーンのライフサイクル管理を行うコントローラー
    /// ILifecycleTargetを実装したコンポーネントの初期化順序を制御する
    /// </summary>
    public class LifecycleController : MonoBehaviour
    {
        [Header("環境設定")] 
        [SerializeField, Comment("デバッグモード：本番環境にスキップしない")]
        private bool _debugMode = true;
        [SerializeField, Comment("強制遷移する本番シーン名")]
        private string _productionSceneName = "Title";

        [Header("ログ設定")] 
        [SerializeField, Comment("インスタンス生成ログの表示")]
        private bool _isLogFormatEnabled = true;
        [SerializeField, Comment("ライフサイクル実行ログの表示")]
        private bool _isLoggingEnabled = true;

        [Header("自動インスタンス化するプレハブリスト")] 
        [SerializeField]
        private List<GameObject> _prefabsToInstantiate = new List<GameObject>();

        private readonly List<ILifecycleTarget> _lifecycleTargets = new List<ILifecycleTarget>();
        private readonly Dictionary<Type, List<ILifecycleTarget>> _instanceCache = new Dictionary<Type, List<ILifecycleTarget>>();
        private readonly HashSet<ILifecycleTarget> _registeredInstances = new HashSet<ILifecycleTarget>();

        /// <summary>
        /// Awake
        /// </summary>
        private async void Awake()
        {
            try
            {
                // 本番環境へのリダイレクトが必要かチェック
                if (ShouldRedirectToProduction())
                {
                    RedirectToProduction();
                    return;
                }

                // ライフサイクル実行
                await ExecuteLifecycle();

                LogUtility.Info("\u2705 全てのオブジェクトの初期化が完了しました", LogCategory.System);
            }
            catch (Exception ex)
            {
                LogUtility.Error($"ライフサイクル実行中にエラーが発生しました: {ex.Message}", LogCategory.System);
            }
        }

        #region Redirect

        /// <summary>
        /// 本番環境へのリダイレクトが必要かチェック
        /// </summary>
        private bool ShouldRedirectToProduction()
        {
            return !_debugMode &&
                   !string.IsNullOrEmpty(_productionSceneName) &&
                   SceneManager.GetActiveScene().name != _productionSceneName;
        }

        /// <summary>
        /// 本番環境へのリダイレクト実行
        /// </summary>
        private void RedirectToProduction()
        {
            LogUtility.Info($"\ud83d\udd34 本番環境: Titleシーンに強制遷移します", LogCategory.System);
            SceneManager.LoadScene(_productionSceneName);
        }

        #endregion

        /// <summary>
        /// ライフサイクルを実行する
        /// </summary>
        private async UniTask ExecuteLifecycle()
        {
            await DiscoverAndInstantiateTargets(); // インスタンス化

            await ExecuteLifecyclePhase(view => view.OnAwake());
            await ExecuteLifecyclePhase(view => view.OnUIInitialize());
            await ExecuteLifecyclePhase(view => view.OnBind());
            await ExecuteLifecyclePhase(view => view.OnStart());
        }

        /// <summary>
        /// ターゲットの発見とインスタンス化
        /// </summary>
        private async UniTask DiscoverAndInstantiateTargets()
        {
            // 既存のターゲットを発見
            DiscoverExistingTargets();

            // プレハブからインスタンス化
            await InstantiatePrefabTargets();

            LogUtility.Verbose($"発見・生成されたライフサイクルターゲット数: {_prefabsToInstantiate.Count}");
        }

        /// <summary>
        /// シーン内の既存ターゲットを発見
        /// </summary>
        private void DiscoverExistingTargets()
        {
            var existingTargets = FindObjectsOfType<MonoBehaviour>()
                .OfType<ILifecycleTarget>()
                .ToList();

            foreach (var target in existingTargets)
            {
                RegisterTarget(target);
            }
        }

        /// <summary>
        /// プレハブからターゲットをインスタンス化
        /// </summary>
        private async UniTask InstantiatePrefabTargets()
        {
            foreach (var prefab in _prefabsToInstantiate.Where(p => p != null))
            {
                try
                {
                    var targetComponents = GetAllLifecycleTargetComponents(prefab);
                    if (!targetComponents.Any())
                    {
                        LogUtility.Warning($"{prefab.name}: ILifecycleTargetを実装したコンポーネントが見つかりません", LogCategory.System);
                        continue;
                    }

                    // 各コンポーネント型について処理
                    foreach (var targetComponent in targetComponents)
                    {
                        var targetType = targetComponent.GetType();

                        // 既存インスタンスチェック（同じGameObjectに複数ある場合を考慮）
                        if (HasExistingInstanceOfType(targetType, prefab))
                        {
                            LogUtility.Verbose($"{prefab.name} ({targetType.Name}): 既存インスタンスが存在するためスキップ",
                                LogCategory.System);
                            continue;
                        }

                        // インスタンス化
                        var instance = await InstantiatePrefab(prefab, targetType);
                        if (instance != null)
                        {
                            RegisterTarget(instance);
                            LogUtility.Verbose($"{prefab.name} ({targetType.Name})", LogCategory.System);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogUtility.Error($"{prefab.name}: インスタンス化エラー - {ex.Message}", LogCategory.System);
                }
            }
        }

        /// <summary>
        /// 各ライフサイクルメソッドを全ビューに適用
        /// </summary>
        private async UniTask ExecuteLifecyclePhase(Func<ILifecycleTarget, UniTask> lifecycleMethod)
        {
            var tasks = _lifecycleTargets
                .Where(target => target != null)
                .Select(async target =>
                {
                    try
                    {
                        await lifecycleMethod(target);
                    }
                    catch (Exception ex)
                    {
                        LogUtility.Error($"実行エラー - ({target.GetType().Name}): {ex.Message}", LogCategory.System);
                    }
                });

            await UniTask.WhenAll(tasks);
        }


        #region Register Target

        /// <summary>
        /// ターゲットの登録（再帰的に子オブジェクトも処理）
        /// </summary>
        private void RegisterTarget(ILifecycleTarget target)
        {
            if (target == null || _registeredInstances.Contains(target)) return;

            var targetType = target.GetType();

            // ライフサイクルターゲットリストに追加
            _lifecycleTargets.Add(target);
            _registeredInstances.Add(target);

            // 型別キャッシュに追加
            if (!_instanceCache.ContainsKey(targetType))
            {
                _instanceCache[targetType] = new List<ILifecycleTarget>();
            }

            _instanceCache[targetType].Add(target);

            // 子オブジェクトの処理（MonoBehaviourの場合のみ）
            if (target is MonoBehaviour monoBehaviour)
            {
                RegisterChildTargets(monoBehaviour.transform);
            }
        }

        /// <summary>
        /// 子オブジェクトのターゲット登録
        /// </summary>
        private void RegisterChildTargets(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (child.TryGetComponent<ILifecycleTarget>(out var childTarget))
                {
                    RegisterTarget(childTarget);
                    RegisterChildTargets(child); // 再帰処理
                }
            }
        }

        #endregion

        /// <summary>
        /// プレハブからすべてのILifecycleTargetコンポーネントを取得
        /// </summary>
        private ILifecycleTarget[] GetAllLifecycleTargetComponents(GameObject prefab)
        {
            return prefab.GetComponents<Component>()
                .OfType<ILifecycleTarget>()
                .ToArray();
        }

        /// <summary>
        /// 指定型の既存インスタンスが存在するかチェック
        /// </summary>
        private bool HasExistingInstanceOfType(Type targetType, GameObject sourcePrefab)
        {
            if (!_instanceCache.TryGetValue(targetType, out var instances))
                return false;

            // 同じプレハブから生成されたかどうかもチェック可能
            return instances.Any(instance =>
                instance is MonoBehaviour mb &&
                mb.gameObject.name.Contains(sourcePrefab.name));
        }

        /// <summary>
        /// プレハブのインスタンス化（リフレクション使用）
        /// </summary>
        private async UniTask<ILifecycleTarget> InstantiatePrefab(GameObject prefab, Type targetType)
        {
            try
            {
                var instantiateMethod = typeof(GameObjectUtility)
                    .GetMethod(nameof(GameObjectUtility.Instantiate))
                    ?.MakeGenericMethod(targetType);

                if (instantiateMethod == null)
                {
                    LogUtility.Error($"GameObjectUtility.Instantiate<{targetType.Name}>メソッドが見つかりません");
                    return null;
                }

                var result = instantiateMethod.Invoke(null, new object[] { prefab });
                return result as ILifecycleTarget;
            }
            catch (Exception ex)
            {
                LogUtility.Error($"リフレクション実行エラー: {ex.Message}", LogCategory.System);
                return null;
            }
        }
    }
}