using System;
using UnityEngine;

namespace Pumkin.AvatarTools
{
    public static class PumkinToolsLogger
    {
        /// <summary>
        /// Logs a message to console with a blue PumkinsAvatarTools: prefix.
        /// </summary>
        /// <param name="logFormat">Same as string.format</param>
        public static void Log(string message, LogType logType = LogType.Log, params string[] logFormat)
        {
            string msg = message;
            try
            {
                if(logFormat.Length > 0)
                    message = string.Format(message, logFormat);
                message = "<color=blue>PumkinsAvatarTools</color>: " + message;
            }
            catch
            {
                message = msg;
                logType = LogType.Warning;
            }
            switch(logType)
            {
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Exception:
                    Debug.LogException(new Exception(message));
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }
        
        /// <summary>
        /// Logs a message to console with a red PumkinsAvatarTools: prefix. Only if verbose logging is enabled.
        /// </summary>
        /// <param name="logFormat">Same as string.format()</param>
        public static void LogVerbose(string message, LogType logType = LogType.Log, params string[] logFormat)
        {
            if(!PumkinsAvatarTools.Settings.verboseLoggingEnabled)
                return;

            if(logFormat.Length > 0)
                message = string.Format(message, logFormat);
            message = "<color=red>PumkinsAvatarTools</color>: " + message;

            switch(logType)
            {
                case LogType.Error:
                    Debug.Log(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Exception:
                    Debug.LogException(new Exception(message));
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }
    }
}