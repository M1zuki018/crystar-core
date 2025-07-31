using Cysharp.Threading.Tasks;

namespace CryStar.Core
{
    /// <summary>
    /// ライフサイクル管理対象のインターフェース
    /// メソッドを増やしたい場合、このインターフェース内でメソッドを増やしたあと
    /// LifecycleController の ExecuteLifecycle() に実行処理を追加してください
    /// </summary>
    public interface ILifecycleTarget
    {
        /// <summary>
        /// 他クラスに干渉しない初期化処理
        /// </summary>
        UniTask OnAwake();
        
        /// <summary>
        /// UI表示の初期化処理
        /// </summary>
        UniTask OnUIInitialize();
        
        /// <summary>
        /// event Actionの登録など他のクラスと干渉する処理
        /// </summary>
        UniTask OnBind();
        
        /// <summary>
        /// ゲーム開始前最後に実行される処理
        /// </summary>
        UniTask OnStart();
    }
}