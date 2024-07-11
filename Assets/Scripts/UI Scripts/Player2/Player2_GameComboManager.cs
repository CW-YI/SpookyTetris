using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player2_GameComboManager : MonoBehaviour
{
    private TextMeshPro online_comboText;
    private TextMeshProUGUI comboText;
    Player2_TetrisBlock game_TetrisBlock;

    private void Awake()
    {
        if (PhotonNetwork.IsConnected) online_comboText = GetComponent<TextMeshPro>();
        else comboText = GetComponent<TextMeshProUGUI>();
        game_TetrisBlock = FindAnyObjectByType<Player2_TetrisBlock>();

        SceneManager.sceneLoaded += OnSceneLoaded;

    }
    void Start()
    {
        Initialize();
        Player2_TetrisBlock.OnUpdateComboFunctionCalled += TriggerComboUpdate;
        if (PhotonNetwork.IsConnected) online_comboText.alpha = 0;
        //else comboText.alpha = 0;
    }

    private void OnDestroy()
    {
        Player2_TetrisBlock.OnUpdateComboFunctionCalled -= TriggerComboUpdate;
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
        game_TetrisBlock = FindAnyObjectByType<Player2_TetrisBlock>();

        Player2_TetrisBlock.OnUpdateComboFunctionCalled += TriggerComboUpdate;
        online_comboText.alpha = 0;
    
    }
    private void TriggerComboUpdate()
    {
        if (Player2_TetrisBlock.comboCounter > 0 && PhotonNetwork.IsConnected) StartCoroutine(Online_UpdateComboNumber());
        else if (Player2_TetrisBlock.comboCounter > 0) StartCoroutine(UpdateComboNumber());
    }
    IEnumerator Online_UpdateComboNumber()
    {
        online_comboText.alpha = 1;
        if (Player2_TetrisBlock.rowsDeleted < 4) online_comboText.text = Player2_TetrisBlock.comboCounter + " Combo!";
        else if (Player2_TetrisBlock.rowsDeleted == 4) online_comboText.text = "TETRIS!";

        yield return new WaitForSeconds(1);
        online_comboText.alpha = 0;
    }

    IEnumerator UpdateComboNumber()
    {
        comboText.alpha = 1;
        if (Player2_TetrisBlock.rowsDeleted < 4) comboText.text = Player2_TetrisBlock.comboCounter + " Combo!";
        else if (Player2_TetrisBlock.rowsDeleted == 4) comboText.text = "TETRIS!";

        yield return new WaitForSeconds(1);
        comboText.alpha = 0;
    }
}
