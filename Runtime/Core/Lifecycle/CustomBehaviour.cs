using CryStar.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CryStar.Core
{
    /// <summary>
    /// 独自のライフサイクルを定義した基底クラス
    ///
    /// NOTE:
    /// Unity標準のライフサイクル（Awake, Start等）の実行順序は保証されないため、
    /// 確実な初期化順序が必要な場合にこのクラスを継承してください
    /// 
    /// 各メソッドをオーバーライドする際は、必ず base メソッドを呼び出してください
    /// 重い処理を行う場合は適切に await を使用してください
    ///
    /// UnityのLifecycleのAwakeのタイミングで実行されます
    /// </summary>
    public abstract class CustomBehaviour : MonoBehaviour, ILifecycleTarget
    {
        /// <summary>
        /// 他クラスに干渉しない処理
        /// </summary>
        public virtual UniTask OnAwake()
        {
            LogUtility.Verbose($"[CustomBehaviour] {gameObject.name} の Awake 実行");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// UI表示の初期化
        /// </summary>
        public virtual UniTask OnUIInitialize()
        {
            LogUtility.Verbose($"[CustomBehaviour] {gameObject.name} の UIInitialize 実行");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// event Actionの登録など他のクラスと干渉する処理
        /// </summary>
        public virtual UniTask OnBind()
        {
            LogUtility.Verbose($"[CustomBehaviour] {gameObject.name} の Bind 実行");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// ゲーム開始前最後に実行される処理
        /// </summary>
        public virtual UniTask OnStart()
        {
            LogUtility.Verbose($"[CustomBehaviour] {gameObject.name} の Start 実行");
            return UniTask.CompletedTask;
        }
    }
}