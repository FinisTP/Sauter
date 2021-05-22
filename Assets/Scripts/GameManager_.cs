using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager_ : MonoBehaviour
{

    public GameObject Player;
    public Camera MainCamera;
    public bool GameOver = false;

    public Animator transition;
    public float transitionTime = 1f;

    private static GameManager_ instance = null;
    public static GameManager_ Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager_>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = "GameManager_";
                    instance = go.AddComponent<GameManager_>();

                    DontDestroyOnLoad(go);
                }

            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Player = GameObject.Find("Player");
        MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

    }

    private void Update()
    {
        if (!GameOver && (Player == null || MainCamera == null))
        {
            Player = GameObject.Find("Player");
            MainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        }
    }

    public void LoadNextScene()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
    }

}
