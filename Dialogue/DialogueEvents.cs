using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueEvents : MonoBehaviour
{
    public static void HandleEvent(string _event, ChapterLineManager.Line.Segment segment)
    {
        if(_event.Contains("("))
        {
            string[] actions = _event.Split(' ');
            for(int i=0; i<actions.Length;i++)
            {
                NovelController.instance.HandleAction(actions[i]);
            }
            return;
        }
        string[] eventData = _event.Split(' ');
        switch (eventData[0])
        {
            case "txtSpd":
                Event_TxtSpd(eventData[1], segment);

                break;

            case "/txtSpd":
                segment.architect.speed = 1;
                segment.architect.charactersPerFrame = 1;

                break;

        }
    }

    static void Event_TxtSpd(string data, ChapterLineManager.Line.Segment segment)
    {
        string[] parts = data.Split(',');
        float delay = float.Parse(parts[0]);
        int charactersPerFrame = int.Parse(parts[1]);

        segment.architect.speed = delay;
        segment.architect.charactersPerFrame = charactersPerFrame;
    }
}
