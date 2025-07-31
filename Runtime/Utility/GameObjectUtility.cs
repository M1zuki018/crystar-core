using System;
using CryStar.Core;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// GameObjectに関するUtilityクラス
/// </summary>
public static class GameObjectUtility
{
    /// <summary>
    /// プレハブをインスタンス化し、CustomBehaviour の派生クラスを返す
    /// </summary>
    public static T Instantiate<T>(GameObject prefab) where T : CustomBehaviour
    {
        if (prefab == null)
        {
            Debug.LogException(new ArgumentNullException(nameof(prefab), "⛔ Prefabがnullです"));
            return null;
        }

        try
        {
            var instance= Object.Instantiate(prefab); // インスタンス生成
            var viewBase = instance.GetComponent<T>(); // T型のコンポーネント取得
        
            if (viewBase == null)
            {
                throw new InvalidOperationException($"⛔ {prefab.name} に {typeof(T).Name} のコンポーネントがアタッチされていません");
            }
            
            return viewBase;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return null;
        }
    } 
}