using System;
using System.Collections.Generic;
using UnityEngine;

namespace B9PartSwitch
{
    class SeriousWarningHandler
    {
        private const int MAX_MESSAGE_COUNT = 10;
        private static PopupDialog dialog;
        private static List<string> allMessages = new List<string>();

        public static void DisplaySeriousWarning(string message)
        {
            try
            {
                UpsertDialog(message);
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception while trying to create the serious warning dialog");
                Debug.LogException(ex);
                Application.Quit();
            }
        }

        private static void UpsertDialog(string message)
        {
            if (string.IsNullOrEmpty(message) || allMessages.Contains(message)) return;

            if (allMessages.Count < MAX_MESSAGE_COUNT)
            {
                allMessages.Add(message);
            }
            else if (allMessages.Count == MAX_MESSAGE_COUNT)
            {
                Debug.LogError("[SeriousWarningHandler] Not displaying serious warning because too many warnings have already been added:");
                Debug.LogError(message);
                allMessages.Add("(too many warning messages to display)");
            }
            else
            {
                Debug.LogError("[SeriousWarningHandler] Not displaying serious warning because too many warnings have already been added:");
                Debug.LogError(message);
                return;
            }

            if (dialog != null) dialog.Dismiss();

            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new MultiOptionDialog(
                    "B9PartSwitchSeriousWarning",
                    $"B9PartSwitch has encountered a serious warning. The game will continue to run but this should be fixed ASAP.\n\n{string.Join("\n\n", allMessages.ToArray())}\n\nPlease see KSP's log for additional details",
                    "B9PartSwitch - Serious Warning",
                    HighLogic.UISkin,
                    new Rect(0.5f, 0.5f, 500f, 60f),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIHorizontalLayout(
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIButton("OK", delegate () { }, 140.0f, 30.0f, true),
                        new DialogGUIFlexibleSpace()
                    )
                ),
                true,
                HighLogic.UISkin);
        }
    }
}
