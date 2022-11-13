using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // singleton variables
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    // Game configuration
    [SerializeField] public Color[] chestColorByQuality;

    // unity components
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] float[] audioSourceStartTime;

    // Game Assets
    [SerializeField] public GameObject[] chestMeshes; // chest mesh is ordered by ENUM.

    // Laser stuff

    // Awake used to enforce singleton pattern. this means the game must start from the main menu, but will then always contain an instance of ScoreManager
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;

            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    // this should act as an awake method for the script since it will be called each time a new scene is loaded.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        audioSourceStartTime = new float[audioSources.Length];
        for (int i = 0; i < audioSourceStartTime.Length; i++)
            audioSourceStartTime[i] = 0;
    }

    /// <summary>
    ///     Plays the sound passed. By default will play from the first audio player, but others can be specified.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="playerIndex">Which audio player should play the sound</param>
    public void PlaySound(AudioClip clip, Vector3 soundOrigin, int playerIndex = -1)
    {
        int index = playerIndex;
        if (playerIndex == -1)
        {
            // when index is set to -1, use the 'oldest' source to play a sound instead.
            float min = Mathf.Min(audioSourceStartTime);
            for(int i = 0; i < audioSourceStartTime.Length; i++)
            {
                if (audioSourceStartTime[i] == min)
                {
                    index = i;
                    break;
                }
            }
        }

        audioSources[index].transform.position = soundOrigin;
        audioSources[index].PlayOneShot(clip);
        audioSourceStartTime[index] = Time.time;
    }

    public void DrawLine(Vector3 start, Vector3 end, Color color, float width = 0.125f, float lineLife = 0.33f)
    {
        GameObject go = new GameObject();
        go.transform.position = Vector3.zero;

        LaserController lc = go.AddComponent(typeof(LaserController)) as LaserController;

        // set it goin
        lc.CreateLine(color, start, end, width, lineLife);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
