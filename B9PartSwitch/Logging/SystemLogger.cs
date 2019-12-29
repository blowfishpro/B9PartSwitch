using System;
using UnityEngine;

namespace B9PartSwitch.Logging
{
    public static class SystemLogger
    {
        public static ILogger Logger { get; }

        static SystemLogger()
        {
            UnityLogger unityLogger = new UnityLogger(Debug.unityLogger);
            Logger = new AlertInterceptor(unityLogger);
        }
    }
}
