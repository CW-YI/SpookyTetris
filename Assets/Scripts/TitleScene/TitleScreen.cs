using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    bool isStart = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown && !isStart)
        {
            isStart = true;
            MoveScene();
        }
    }

    void MoveScene()
    {
        SceneManager.LoadScene("OnlineLobby");
    }

    public void GameStart()
    {
        Debug.Log("GameStart");
        SceneManager.LoadScene("OnlineLobby");
    }

    public void GameExit()
    {
        Debug.Log("GameExit");
        Application.Quit();
    }
}
