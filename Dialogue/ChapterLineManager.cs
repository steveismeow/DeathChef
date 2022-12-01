using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages each line in the dialogue file: Segmentation and interprets tags line by line at the behest of the NovelController
/// </summary>
public class ChapterLineManager : MonoBehaviour
{
    public static Line Interpret(string rawLine)
    {
        return new Line(rawLine);
    }

    public class Line
    {
        public string speaker = "";

        public List<Segment> segments = new List<Segment>();
        public List<string> actions = new List<string>();

        public string lastSegmentsWholeDialogue = "";

        public Line(string rawLine)
        {
            string[] dialogueAndActions = rawLine.Split('"');
            char actionSplitter = ' ';
            string[] actionsArray = dialogueAndActions.Length == 3 ? dialogueAndActions[2].Split(actionSplitter) : dialogueAndActions[0].Split(actionSplitter);

            if (dialogueAndActions.Length == 3)
            {
                speaker = dialogueAndActions[0] == "" ? NovelController.instance.cachedLastSpeaker : dialogueAndActions[0];
                if (speaker[speaker.Length - 1] == ' ')
                {
                    speaker = speaker.Remove(speaker.Length - 1);
                }

                NovelController.instance.cachedLastSpeaker = speaker;

                SegmentDialogue(dialogueAndActions[1]);

            }

            for (int i = 0; i < actionsArray.Length; i++)
            {
                actions.Add(actionsArray[i]);
            }

        }

        void SegmentDialogue(string dialogue)
        {
            segments.Clear();
            string[] parts = dialogue.Split('{', '}');

            for (int i = 0; i < parts.Length; i++)
            {
                Segment segment = new Segment();
                bool isOdd = i % 2 != 0;

                if (isOdd)
                {
                    string[] commandData = parts[i].Split(' ');
                    switch (commandData[0])
                    {
                        case "c":
                            segment.trigger = Segment.Trigger.waitClick;
                            break;
                        case "a":
                            segment.trigger = Segment.Trigger.waitClick;
                            segment.pretext = segments.Count > 0 ? segments[segments.Count - 1].dialogue : "";
                            break;
                        case "w":
                            segment.trigger = Segment.Trigger.autoDelay;
                            segment.autoDelay = float.Parse(commandData[1]);

                            break;
                        case "wa":
                            segment.trigger = Segment.Trigger.autoDelay;
                            segment.autoDelay = float.Parse(commandData[1]);
                            segment.pretext = segments.Count > 0 ? segments[segments.Count - 1].dialogue : "";
                            break;

                    }
                    i++;

                }

                segment.dialogue = parts[i];
                segment.line = this;

                segments.Add(segment);
            }
        }

        public class Segment
        {
            public Line line;
            public string dialogue = "";
            public string pretext = "";
            public enum Trigger
            {
                waitClick,
                autoDelay
            }
            public Trigger trigger = Trigger.waitClick;

            public float autoDelay = 0;

            public void Run()
            {
                if (running != null)
                {
                    NovelController.instance.StopCoroutine(running);
                }

                running = NovelController.instance.StartCoroutine(Running());

            }

            public bool isRunning { get { return running != null; } }
            Coroutine running = null;
            public TextArchitect architect = null;
            List<string> allCurrentExecutedEvents = new List<string>();
            IEnumerator Running()
            {
                allCurrentExecutedEvents.Clear();

                TagManager.Inject(ref dialogue);

                string[] parts = dialogue.Split('[', ']');

                for (int i = 0; i < parts.Length; i++)
                {
                    bool isOdd = i % 2 != 0;
                    if (isOdd)
                    {
                        DialogueEvents.HandleEvent(parts[i], this);
                        allCurrentExecutedEvents.Add(parts[i]);
                        i++;
                    }

                    string targetDialogue = parts[i];

                    //if (line.speaker != "player")
                    //{

                    //    DialogueManager.instance.Speak(targetDialogue, line.speaker, i > 0 ? true : pretext != "");
                    //}
                    //else
                    //{
                    //    //Tell the DialogueBox tmpro to set Italics
                    //    DialogueManager.instance.Speak(targetDialogue, line.speaker, i > 0 ? true : pretext != "");
                    //}

                    DialogueManager.instance.Speak(targetDialogue, line.speaker, i > 0 ? true : pretext != "");


                    architect = DialogueManager.instance.currentArchitect;
                    while (architect.isConstructing)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                }

                running = null;
            }

            public void ForceFinish()
            {
                if (running != null)
                {
                    NovelController.instance.StopCoroutine(running);
                }

                running = null;

                if (architect != null)
                {
                    architect.ForceFinish();

                    if (pretext == "")
                    {
                        line.lastSegmentsWholeDialogue = "";

                    }

                    string[] parts = dialogue.Split('[', ']');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        bool isOdd = i % 2 != 0;
                        if (isOdd)
                        {
                            string e = parts[i];
                            if (allCurrentExecutedEvents.Contains(e))
                            {
                                allCurrentExecutedEvents.Remove(e);
                            }
                            else
                            {
                                DialogueEvents.HandleEvent(e, this);
                            }
                            i++;
                        }

                        line.lastSegmentsWholeDialogue += parts[i];
                    }

                    architect.ShowText(line.lastSegmentsWholeDialogue);
                }
            }

        }
    }
}
