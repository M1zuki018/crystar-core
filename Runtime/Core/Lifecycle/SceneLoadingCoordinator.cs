using System;
using System.Threading;
using CryStar.Utility;
using CryStar.Utility.Enum;
using Cysharp.Threading.Tasks;

namespace CryStar.Core
{
    /// <summary>
    /// シーンロードとライフサイクルの協調を管理するクラス
    /// </summary>
    public static class SceneLoadingCoordinator
    {
        private static UniTaskCompletionSource _sceneInitializationCompletion;
        private static readonly object _lockObject = new object();
        
        /// <summary>
        /// シーンの初期化開始を通知
        /// </summary>
        public static void NotifySceneInitializationStarted()
        {
            lock (_lockObject)
            {
                _sceneInitializationCompletion?.TrySetCanceled();
                _sceneInitializationCompletion = new UniTaskCompletionSource();
                LogUtility.Verbose("シーン初期化開始を通知しました", LogCategory.System);
            }
        }
        
        /// <summary>
        /// シーンの初期化完了を通知
        /// </summary>
        public static void NotifySceneInitializationCompleted()
        {
            lock (_lockObject)
            {
                _sceneInitializationCompletion?.TrySetResult();
                LogUtility.Info("✅ シーン初期化完了を通知しました", LogCategory.System);
            }
        }
        
        /// <summary>
        /// シーンの初期化完了を待機
        /// </summary>
        public static async UniTask WaitForSceneInitializationAsync(CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource completionSource;
            lock (_lockObject)
            {
                completionSource = _sceneInitializationCompletion;
            }
            
            if (completionSource == null)
            {
                LogUtility.Warning("シーン初期化の完了源が設定されていません", LogCategory.System);
                return;
            }
            
            try
            {
                await completionSource.Task.AttachExternalCancellation(cancellationToken);
                LogUtility.Verbose("シーン初期化の完了を確認しました", LogCategory.System);
            }
            catch (OperationCanceledException)
            {
                LogUtility.Info("シーン初期化の待機がキャンセルされました", LogCategory.System);
                throw;
            }
        }
        
        /// <summary>
        /// 初期化エラーを通知
        /// </summary>
        public static void NotifySceneInitializationError(Exception error)
        {
            lock (_lockObject)
            {
                _sceneInitializationCompletion?.TrySetException(error);
                LogUtility.Error($"シーン初期化エラーを通知しました: {error.Message}", LogCategory.System);
            }
        }
        
        /// <summary>
        /// リソースをクリーンアップ
        /// </summary>
        public static void Cleanup()
        {
            lock (_lockObject)
            {
                _sceneInitializationCompletion?.TrySetCanceled();
                _sceneInitializationCompletion = null;
            }
        }
    }
}