using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Core.Framework
{
    public static class FileUtil
    {
        private static bool _isUnityEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static string GetWritablePath(string relativeFilePath)
        {
            return GetPersistentDataPath() + Path.DirectorySeparatorChar + relativeFilePath;
        }

        public static string GetLuaFilePath(string relativeFilePath)
        {
            return GetPersistentDataPath() + Path.DirectorySeparatorChar + "LuaScripts" + Path.DirectorySeparatorChar + relativeFilePath;
        }

        public static string GetJsonFilePath(string relativeFilePath)
        {
            return GetPersistentDataPath() + Path.DirectorySeparatorChar + "Entities" + Path.DirectorySeparatorChar + relativeFilePath;
        }

        private static string GetPersistentDataPath()
        {
            if (_isUnityEditor)
                return Application.dataPath.Replace("Assets", "ExternalData");

            return Application.persistentDataPath;
        }

        public static byte[] LoadFile(string absolutePath)
        {
            if (absolutePath == null || absolutePath.Length == 0)
            {
                return null;
            }
            if (File.Exists(absolutePath))
            {
                return File.ReadAllBytes(absolutePath);
            }
            else
            {
                return null;
            }
        }

        public static bool CheckFileExist(string filePath)
        {
            return File.Exists(filePath);
        }

        public static bool CreateNewFile(string content, string path)
        {
            try
            {
                using (FileStream fs = File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(content);
                    fs.Write(info, 0, info.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                return false;
            }
        }
    }
}