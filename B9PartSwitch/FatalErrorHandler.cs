using System;
using System.Collections.Generic;
using UnityEngine;

namespace B9PartSwitch
{
    public static class FatalErrorHandler
    {
        private const int MAX_MESSAGE_COUNT = 10;
        private static PopupDialog dialog;
        private static List<string> allMessages = new List<string>();

        public static void HandleFatalError(string message)
        {
            try
            {
                if (allMessages.Contains(message))
                {
                    return;
                }
                else if (allMessages.Count < MAX_MESSAGE_COUNT)
                {
                    allMessages.Add(message);
                }
                else if (allMessages.Count == MAX_MESSAGE_COUNT)
                {
                    Debug.LogError("[FatalExceptionHandler] Not displaying fatal error because too many errors have already been added:");
                    Debug.LogError(message);
                    allMessages.Add("(too many error messages to display)");
                }
                else
                {
                    Debug.LogError("[FatalExceptionHandler] Not displaying fatal error because too many errors have already been added:");
                    Debug.LogError(message);
                    return;
                }

                if (dialog != null) dialog.Dismiss();

                dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new MultiOptionDialog(
                        "B9PartSwitchFatalError: " + message,
                        $"B9PartSwitch has encountered a fatal error and KSP needs to close.\n\n{string.Join("\n\n", allMessages.ToArray())}\n\nPlease see KSP's log for addtional details",
                        "B9PartSwitch - Fatal Error",
                        HighLogic.UISkin,
                        new Rect(0.5f, 0.5f, 500f, 60f),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIHorizontalLayout(
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton("Quit", Application.Quit, 140.0f, 30.0f, true),
                            new DialogGUIFlexibleSpace()
                        )
                    ),
                    true,
                    HighLogic.UISkin);
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception while trying to create the fatal exception dialog");
                Debug.LogException(ex);
                Application.Quit();
            }
        }
    }
}
