using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagManager : MonoBehaviour
{
    public static void Inject(ref string s)
    {
        if(!s.Contains("["))
        {
            return;
        }

        //Replace second "Player" with a reference to the gamedata file's saved player name that they will select on [new game], or else we can just comment this out
        s = s.Replace("[player]", "Player");
    }

    public static string[] SplitByTags(string targetText)
    {
        return targetText.Split(new char[2] { '<', '>' });
    }
}
