using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NetworkObjectDictionnary
{
    /// <summary>
    /// Dictionnary that contains all gameobjects spawnable
    /// </summary>
    private static readonly Dictionary<int, string> dictionary = new Dictionary<int, string>
    {
        {1, "BouncyBall" },
        {2, "Player" }
    };

    /// <summary>
    /// Returns the specified object name 
    /// </summary>
    /// <param name="pID"></param>
    /// <returns></returns>
    public static string GetResourcePathFor(int pID)
    {
        string objectName;
        dictionary.TryGetValue(pID, out objectName);
 //       Debug.Log(objectName);
        return objectName;
    }
}