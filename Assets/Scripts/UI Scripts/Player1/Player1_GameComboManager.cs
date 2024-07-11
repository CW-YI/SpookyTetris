using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player1_GameComboManager : MonoBehaviour
{
    private TextMeshPro online_comboText;
    private TextMeshProUGUI comboText;
    Player1_TetrisBlock game_TetrisBlock;

    private void Awake()
    {
        if (PhotonNetwork.IsConnected) online_comboText = GetComponent<TextMeshPro>();
        else comboText = GetComponent<TextMeshProUGUI>();
        game_TetrisBlock = FindAnyObjectByType<Player1_TetrisBlock>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void Start()
    {
        Initialize();
        Player1_TetrisBlock.OnUpdateComboFunctionCalled += TriggerComboUpdate;
        if (PhotonNetwork.IsConnected) online_comboText.alpha = 0;
        //else comboText.alpha = 0;

    }

    private void OnDestroy()
    {
        Player1_TetrisBlock.OnUpdateComboFunctionCalled -= TriggerComboUpdate;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "OnlineTetris") Initialize();
    }

    void Initialize()
    {
        if (online_comboText != null) return;
        online_comboText = GetComponent<TextMeshPro>();
        game_TetrisBlock = FindAnyObjectByType<Player1_TetrisBlock>();

        Player1_TetrisBlock.OnUpdateComboFunctionCalled += TriggerComboUpdate;
        online_comboText.alpha = 0;
    }
    
 
    private void TriggerComboUpdate()
    {
        if (Player1_TetrisBlock.comboCounter > 0 && PhotonNetwork.IsConnected) StartCoroutine(Online_UpdateComboNumber());
    }
    IEnumerator Online_UpdateComboNumber()
    {
        online_comboText.alpha = 1;
        if (Player1_TetrisBlock.rowsDeleted < 4) online_comboText.text = Player1_TetrisBlock.comboCounter + " Combo!";
        else if (Player1_TetrisBlock.rowsDeleted == 4) online_comboText.text = "TETRIS!";

        yield return new WaitForSeconds(1);
        online_comboText.alpha = 0;
    }

    IEnumerator UpdateComboNumber()
    {
        comboText.alpha = 1;
        if (Player1_TetrisBlock.rowsDeleted < 4) comboText.text = Player1_TetrisBlock.comboCounter + " Combo!";
        else if (Player1_TetrisBlock.rowsDeleted == 4) comboText.text = "TETRIS!";

        yield return new WaitForSeconds(1);
        comboText.alpha = 0;
    }
}
