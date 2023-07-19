using System;
using UnityEngine;

namespace Core.Framework
{
    public class ColorName
    {
        public static ColorName Red = new ColorName(Color.red);
        public static ColorName Yellow = new ColorName(Color.yellow);
        public static ColorName Green = new ColorName(Color.green);
        public static ColorName Cyan = new ColorName(Color.cyan);

        public static ColorName colorByHex;

        private readonly string _prefix;
        private const string _subffix = "</color>";

        public ColorName()
        { }

        public ColorName(Color color)
        {
            _prefix = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
        }

        public ColorName(string hexColor)
        {
            _prefix = $"<color={hexColor}>";
        }

        public static string operator %(string strContent, ColorName color)
        {
            return color._prefix + strContent + _subffix;
        }
    }

    public class UnityDebugLogger : Business.ILogger
    {
        private readonly Type forType;
        private readonly string forTypeString;
        private readonly bool errorToWarn = true; // default is true(gRPC internal log show to warn)

        public UnityDebugLogger()
            : this(null)
        {
        }

        public UnityDebugLogger(bool errorToWarn)
            : this(null)
        {
            this.errorToWarn = errorToWarn;
        }

        protected UnityDebugLogger(Type forType)
        {
            this.forType = forType;
            if (forType != null)
            {
                var namespaceStr = forType.Namespace ?? "";
                if (namespaceStr.Length > 0)
                {
                    namespaceStr += ".";
                }
                this.forTypeString = namespaceStr + forType.Name + " ";
            }
            else
            {
                this.forTypeString = "";
            }
        }

        /// <summary>
        /// Returns a logger associated with the specified type.
        /// </summary>
        public virtual Business.ILogger ForType<T>()
        {
            if (typeof(T) == forType)
            {
                return this;
            }
            return new UnityDebugLogger(typeof(T));
        }

        /// <summary>
        /// Show Debug with Hex Color
        /// </summary>
        /// <param name="strContent">Debug Text</param>
        /// <param name="hexColor">hex color with #</param>
        public void Log(string strContent, string hexColor = "#ccccff")
        {
            var color = new ColorName(hexColor);
            Debug.Log(strContent % color);
        }

        public void Warning(string message, string hexColor = "#ccccff")
        {
            var color = new ColorName(hexColor);
            Debug.LogWarning(message % color);
        }

        public void Warning(Exception exception, string message, string hexColor = "#ccccff")
        {
            var color = new ColorName(hexColor);
            Debug.LogWarning(exception.Message % color);
        }

        public void Error(string message, string hexColor = "#ccccff")
        {
            var color = new ColorName(hexColor);
            Debug.LogError(message % color);
        }

        public void Error(Exception exception, string message, string hexColor = "#ccccff")
        {
            var color = new ColorName(hexColor);
            Debug.LogError(exception.Message % color);
        }
    }
}