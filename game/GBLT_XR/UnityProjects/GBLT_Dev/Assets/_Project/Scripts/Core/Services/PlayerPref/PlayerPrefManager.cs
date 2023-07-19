using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Core.Framework
{
    public class PlayerPrefManager
    {
        public const string KEY_PREFIX = "ENC-";

        public const string VALUE_FLOAT_PREFIX = "0-";
        public const string VALUE_INT_PREFIX = "1-";
        public const string VALUE_STRING_PREFIX = "2-";
        public const string VALUE_BOOL_PREFIX = "3-";

        private static GameStore.Setting _gameSetting;

        public PlayerPrefManager(
            GameStore.Setting gameSetting)
        {
            _gameSetting = gameSetting;
        }

        public static bool IsSerializedKey(string key)
        {
            if (key.StartsWith(KEY_PREFIX))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string DeserializeKey(string serializedKey)
        {
            if (serializedKey.StartsWith(KEY_PREFIX))
            {
                // Remove the key prefix from the encrypted key
                string strippedKey = serializedKey.Substring(KEY_PREFIX.Length);
                // Return the decrypted key
                return PlayerPrefSerialization.DeserializeString(strippedKey);
            }
            else
            {
                throw new InvalidOperationException("Could not decrypt item, no match found in known encrypted key prefixes");
            }
        }

        #region SET SERIALIZE
        public static void SetSerializedFloat(string key, float value)
        {
            string serializedKey = PlayerPrefSerialization.SerializeString(key);
            string serializedValue = PlayerPrefSerialization.SerializeFloat(value);

            // Store the encrypted key and value (with relevant identifying prefixes) in PlayerPrefs
            PlayerPrefs.SetString(KEY_PREFIX + serializedKey, VALUE_FLOAT_PREFIX + serializedValue);
        }

        public static void SetSerializedInt(string key, int value)
        {
            string serializedKey = PlayerPrefSerialization.SerializeString(key);
            string serializedValue = PlayerPrefSerialization.SerializeInt(value);

            // Store the encrypted key and value (with relevant identifying prefixes) in PlayerPrefs
            PlayerPrefs.SetString(KEY_PREFIX + serializedKey, VALUE_INT_PREFIX + serializedValue);
        }

        public static void SetSerializedString(string key, string value)
        {
            string serializedKey = PlayerPrefSerialization.SerializeString(key);
            string serializedValue = PlayerPrefSerialization.SerializeString(value);

            // Store the encrypted key and value (with relevant identifying prefixes) in PlayerPrefs
            PlayerPrefs.SetString(KEY_PREFIX + serializedKey, VALUE_STRING_PREFIX + serializedValue);
        }

        public static void SetSerializedBool(string key, bool value)
        {
            string serializedKey = PlayerPrefSerialization.SerializeString(key);
            string serializedValue = PlayerPrefSerialization.SerializeBool(value);

            // Store the encrypted key and value (with relevant identifying prefixes) in PlayerPrefs
            PlayerPrefs.SetString(KEY_PREFIX + serializedKey, VALUE_BOOL_PREFIX + serializedValue);
        }
        #endregion

        #region GET SERIALIZE
        public static object GetSerializedValue(string serializedKey, string serializedValue)
        {
            if (serializedValue.StartsWith(VALUE_FLOAT_PREFIX))
            {
                // It's a float, so deserialize it as a float and return the value
                return GetSerializedFloat(PlayerPrefSerialization.DeserializeString(serializedKey.Substring(KEY_PREFIX.Length)));
            }
            else if (serializedValue.StartsWith(VALUE_INT_PREFIX))
            {
                // It's an int, so deserialize it as an int and return the value
                return GetSerializedInt(PlayerPrefSerialization.DeserializeString(serializedKey.Substring(KEY_PREFIX.Length)));
            }
            else if (serializedValue.StartsWith(VALUE_STRING_PREFIX))
            {
                // It's a string, so deserialize it as a string and return the value
                return GetSerializedString(PlayerPrefSerialization.DeserializeString(serializedKey.Substring(KEY_PREFIX.Length)));
            }
            else if (serializedValue.StartsWith(VALUE_BOOL_PREFIX))
            {
                // It's a string, so deserialize it as a string and return the value
                return GetSerializedBool(PlayerPrefSerialization.DeserializeString(serializedKey.Substring(KEY_PREFIX.Length)));
            }
            else
            {
                throw new InvalidOperationException("Could not deserialize item, no match found in known encrypted key prefixes");
            }
        }

        public static float GetSerializedFloat(string key, float defaultValue = 0.0f)
        {
            // Serialize and prefix the key so we can look it up from player prefs
            string serializedKey = KEY_PREFIX + PlayerPrefSerialization.SerializeString(key);

            // Look up the encrypted value
            string fetchedString = PlayerPrefs.GetString(serializedKey);
            return !string.IsNullOrEmpty(fetchedString) ?
                PlayerPrefSerialization.DeserializeFloat(fetchedString.Substring(VALUE_FLOAT_PREFIX.Length)) :
                defaultValue;
        }

        public static int GetSerializedInt(string key, int defaultValue = 0)
        {
            // Serialize and prefix the key so we can look it up from player prefs
            string serializedKey = KEY_PREFIX + PlayerPrefSerialization.SerializeString(key);

            // Look up the encrypted value
            string fetchedString = PlayerPrefs.GetString(serializedKey);
            return !string.IsNullOrEmpty(fetchedString) ?
                PlayerPrefSerialization.DeserializeInt(fetchedString.Substring(VALUE_INT_PREFIX.Length)) :
                defaultValue;
        }

        public static string GetSerializedString(string key, string defaultValue = "")
        {
            // Serialize and prefix the key so we can look it up from player prefs
            string serializedKey = KEY_PREFIX + PlayerPrefSerialization.SerializeString(key);

            // Look up the encrypted value
            string fetchedString = PlayerPrefs.GetString(serializedKey);
            return !string.IsNullOrEmpty(fetchedString) ?
                PlayerPrefSerialization.DeserializeString(fetchedString.Substring(VALUE_STRING_PREFIX.Length)) :
                defaultValue;
        }

        public static bool GetSerializedBool(string key, bool defaultValue = false)
        {
            // Serialize and prefix the key so we can look it up from player prefs
            string serializedKey = KEY_PREFIX + PlayerPrefSerialization.SerializeString(key);

            // Look up the encrypted value
            string fetchedString = PlayerPrefs.GetString(serializedKey);
            return !string.IsNullOrEmpty(fetchedString) ?
                PlayerPrefSerialization.DeserializeBool(fetchedString.Substring(VALUE_BOOL_PREFIX.Length)) :
                defaultValue;
        }
        #endregion

        #region PlayerPref Extension
        public static void SetBool(string key, bool value)
        {
            if (_gameSetting.IsSerializedPlayerPrefs) SetSerializedBool(key, value);
            else PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (_gameSetting.IsSerializedPlayerPrefs) return GetSerializedBool(key, defaultValue);
            else
            {
                // Use HasKey to check if the bool has been stored (as int defaults to 0 which is ambiguous with a stored False)
                if (PlayerPrefs.HasKey(key))
                {
                    int value = PlayerPrefs.GetInt(key);
                    return value != 0;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        public static void SetEnum(string key, Enum value)
        {
            // In-development for serialized enum
            // Convert the enum value to its string name (as opposed to integer index) and store it in a string PlayerPref
            PlayerPrefs.SetString(key, value.ToString());
        }

        public static T GetEnum<T>(string key, T defaultValue = default(T)) where T : struct
        {
            // Fetch the string value from PlayerPrefs
            string stringValue = PlayerPrefs.GetString(key);

            if (!string.IsNullOrEmpty(stringValue))
            {
                // Existing value, so parse it using the supplied generic type and cast before returning it
                return (T)Enum.Parse(typeof(T), stringValue);
            }
            else
            {
                // No player pref for this, just return default. If no default is supplied this will be the enum's default
                return defaultValue;
            }
        }

        public static object GetEnum(string key, Type enumType, object defaultValue)
        {
            // Fetch the string value from PlayerPrefs
            string value = PlayerPrefs.GetString(key);

            if (!string.IsNullOrEmpty(value))
            {
                // Existing value, parse it using the supplied type, then return the result as an object
                return Enum.Parse(enumType, value);
            }
            else
            {
                // No player pref for this key, so just return supplied default. It's required to supply a default value,
                // you can just pass null, but you would then need to do a null check where you call non-generic GetEnum().
                // Consider using GetEnum<T>() which doesn't require a default to be passed (supplying default(T) instead)
                return defaultValue;
            }
        }

        public static void SetDateTime(string key, DateTime value)
        {
            // In-development for serialized datetime
            // Convert to an ISO 8601 compliant string ("o"), so that it's fully qualified, then store in PlayerPrefs
            PlayerPrefs.SetString(key, value.ToString("o", CultureInfo.InvariantCulture));
        }

        public static DateTime GetDateTime(string key, DateTime defaultValue = new DateTime())
        {
            // Fetch the string value from PlayerPrefs
            string stringValue = PlayerPrefs.GetString(key);

            if (!string.IsNullOrEmpty(stringValue))
            {
                // Make sure to parse it using Roundtrip Kind otherwise a local time would come out as UTC
                return DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
            else
            {
                // No existing player pref value, so return defaultValue instead
                return defaultValue;
            }
        }

        public static void SetTimeSpan(string key, TimeSpan value)
        {
            // Use the TimeSpan's ToString() method to encode it as a string which is then stored in PlayerPrefs
            PlayerPrefs.SetString(key, value.ToString());
        }

        public static TimeSpan GetTimeSpan(string key, TimeSpan defaultValue = new TimeSpan())
        {
            // Fetch the string value from PlayerPrefs
            string stringValue = PlayerPrefs.GetString(key);

            if (!string.IsNullOrEmpty(stringValue))
            {
                // Parse the string and return the TimeSpan
                return TimeSpan.Parse(stringValue);
            }
            else
            {
                // No existing player pref value, so return defaultValue instead
                return defaultValue;
            }
        }
        #endregion

        #region Mirror PlayerPref
        public static void SetInt(string key, int value)
        {
            if (_gameSetting.IsSerializedPlayerPrefs) SetSerializedInt(key, value);
            else PlayerPrefs.SetInt(key, value);
        }
        public static int GetInt(string key, int defaultValue = 0)
        {
            return _gameSetting.IsSerializedPlayerPrefs ? GetSerializedInt(key, defaultValue) :
                PlayerPrefs.GetInt(key, defaultValue);
        }

        public static void SetFloat(string key, float value)
        {
            if (_gameSetting.IsSerializedPlayerPrefs) SetSerializedFloat(key, value);
            else PlayerPrefs.SetFloat(key, value);
        }
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            return _gameSetting.IsSerializedPlayerPrefs ? GetSerializedFloat(key, defaultValue) :
                PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static void SetString(string key, string value)
        {
            if (_gameSetting.IsSerializedPlayerPrefs) SetSerializedString(key, value);
            else PlayerPrefs.SetString(key, value);
        }
        public static string GetString(string key, string defaultValue = "")
        {
            return _gameSetting.IsSerializedPlayerPrefs ? GetSerializedString(key, defaultValue) :
                PlayerPrefs.GetString(key, defaultValue);
        }
        #endregion

        public static void SetEncryptedString(string key, string value)
        {
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(value);

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] keyArray = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
            md5.Clear();

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();

            SetString(key, Convert.ToBase64String(resultArray, 0, resultArray.Length));
        }
        public static string GetEncryptedString(string key, string defaultValue = "")
        {
            string value = GetString(key, defaultValue);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            byte[] toEncryptArray = Convert.FromBase64String(value);
            byte[] resultArray;

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] keyArray = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
            md5.Clear();

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;


            ICryptoTransform cTransform = tdes.CreateDecryptor();
            resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();

            return Encoding.UTF8.GetString(resultArray);
        }
    }
}