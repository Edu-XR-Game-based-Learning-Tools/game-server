using UnityEngine;

public class GameSingleton : MonoBehaviour
{
    private static GameSingleton _instance;
    public static GameSingleton Instance
    {
        get { return _instance; }
        set
        {
            if (_instance == null)
                _instance = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }
}
