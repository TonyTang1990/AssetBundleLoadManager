/*
 * Description:             预制件辅助工具类
 * Author:                  tanghuan
 * Create Date:             2018/03/27
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 预制件辅助工具类
/// </summary>
public class PrefabUtilitiesExtension : SingletonTemplate<PrefabUtilitiesExtension> {

    /// <summary>
    /// 如果GameObject绑定了预制件则Apply最新操作
    /// </summary>
    /// <param name="go"></param>
    public void applyGameObjectToPrefab(GameObject go)
    {
        if(go != null)
        {
            var prefab = PrefabUtility.GetPrefabParent(go);
            if (prefab != null)
            {
                PrefabUtility.ReplacePrefab(go, prefab);
                PrefabUtility.ConnectGameObjectToPrefab(go, prefab as GameObject);
            }
            else
            {
                Debug.LogError(string.Format("GameObject.name : {0}没有绑定预制件，无法Apply!", go.name));
            }
        }
        else
        {
            Debug.LogError("applyGameObjectToPrefab(null)");
        }
    }
}
