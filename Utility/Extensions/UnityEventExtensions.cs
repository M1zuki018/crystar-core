using UnityEngine.Events;

namespace CryStar.Utility
{
    /// <summary>
    /// UnityEventの拡張メソッド
    /// </summary>
    public static class UnityEventExtensions
    {
        /// <summary>
        /// 安全にリスナーを追加
        /// </summary>
        public static void SafeAddListener(this UnityEvent unityEvent, UnityAction action)
        {
            if (unityEvent != null && action != null)
            {
                unityEvent.AddListener(action);
            }
        }
    
        /// <summary>
        /// 安全にリスナーを削除
        /// </summary>
        public static void SafeRemoveListener(this UnityEvent unityEvent, UnityAction action)
        {
            if (unityEvent != null && action != null)
            {
                unityEvent.RemoveListener(action);
            }
        }
    
        /// <summary>
        /// 安全に全てのリスナーを削除
        /// </summary>
        public static void SafeRemoveAllListeners(this UnityEvent unityEvent)
        {
            unityEvent?.RemoveAllListeners();
        }

        /// <summary>
        /// 登録されているリスナーを削除してから登録
        /// </summary>
        public static void SafeReplaceListener(this UnityEvent unityEvent, UnityAction action)
        {
            if (unityEvent != null && action != null)
            {
                unityEvent.RemoveAllListeners();
                unityEvent.AddListener(action);
            }
        }
    }
}
