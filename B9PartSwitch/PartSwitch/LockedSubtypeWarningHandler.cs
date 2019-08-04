using System;
using System.Collections.Generic;
using UnityEngine;

namespace B9PartSwitch
{
    public class LockedSubtypeWarningHandler
    {
        private const int MAX_MESSAGE_COUNT = 10;
        private static PopupDialog dialog;
        private static List<string> allMessages = new List<string>();

        public static void WarnSubtypeLocked(string message)
        {
            try
            {
                UpsertDialog(message);
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception while trying to create locked subtype warning dialog");
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
                Debug.LogError("[LockedSubtypeWarningHandler] Not displaying locked subtype warning because too many warnings have already been added:");
                Debug.LogError(message);
                allMessages.Add("(too many locked subtype messages to display)");
            }
            else
            {
                Debug.LogError("[LockedSubtypeWarningHandler] Not displaying locked subtype warning because too many warnings have already been added:");
                Debug.LogError(message);
                return;
            }

            if (dialog != null) dialog.Dismiss();

            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new MultiOptionDialog(
                    "B9PartSwitchLockedSubtypeWarning",
                    $"Some parts have locked subtypes selected.  They have been replaced with the highest priority unlocked subtype\n\n{string.Join("\n\n", allMessages.ToArray())}",
                    "B9PartSwitch - Locked Subtypes",
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
