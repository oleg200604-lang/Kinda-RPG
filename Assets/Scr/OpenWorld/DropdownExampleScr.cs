using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[Serializable]
public class DropdownDataContainer
{
    private object target;
    private FieldInfo field;

    public void Initialize(object targetObject, string fieldName)
    {
        target = targetObject;
        field = targetObject.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (field == null || field.FieldType != typeof(int))
        {
            Debug.LogWarning($"[IntFieldReference] Field '{fieldName}' not found or not int.");
            field = null;
        }
    }

    public int Get()
    {
        if (field == null || target == null)
            return 0;
        return (int)field.GetValue(target);
    }

    public void Set(int value)
    {
        if (field == null || target == null)
            return;
        field.SetValue(target, value);
    }
}