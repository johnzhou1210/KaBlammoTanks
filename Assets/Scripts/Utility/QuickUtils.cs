using System;
using System.Collections.Generic;
using UnityEngine;

public class QuickUtils
{
    public static T Choice<T>(IList<T> list) {
        if (list == null || list.Count == 0) {
            throw new ArgumentException("List is null or empty!");
        }
        int randomIndex = UnityEngine.Random.Range(0, list.Count);
        return list[randomIndex];
    }
}
