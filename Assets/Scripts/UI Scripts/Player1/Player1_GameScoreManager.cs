using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player1_GameScoreManager : MonoBehaviourPun
{
    private TextMeshPro online_scoreText;
    private TextMeshProUGUI scoreText;
    private Player1_TetrisBlock tetrisBlock;
    private int score = 0;
    private int comboIncreaseScore = 0;
    private bool isFallTimeIncreased = false;

    private void Awake()
    {
        if (PhotonNetwork.IsConnected) online_scoreText = GetComponent<TextMeshPro>();
        else scoreText = GetComponent<TextMeshProUGUI>();
    }
    void Start()
    {
        Player1_TetrisBlock.OnUpdateComboFunctionCalled += UpdateScore;
        Player1_TetrisBlock.OnUpdateComboFunctionCalled += DecreaseFallTime;
        if (PhotonNetwork.IsConnected && online_scoreText != null) online_scoreText.text = "" + score;
        else if (scoreText != null) scoreText.text = "" + score;
    }

    private void UpdateScore()
    {
        score += 10 * Player1_TetrisBlock.comboCounter;
        if (PhotonNetwork.IsConnected && online_scoreText != null) online_scoreText.text = "" + score;
        else if (scoreText != null) scoreText.text = "" + score;
    }

    private void DecreaseFallTime()
    {
        tetrisBlock = FindAnyObjectByType<Player1_TetrisBlock>();

        if (score % 500 == 0 && tetrisBlock.fallTime > 0.1f && !isFallTimeIncreased)
        {
            tetrisBlock.fallTime -= 0.1f;
            comboIncreaseScore = score;
            isFallTimeIncreased = true;
        }
        else if (isFallTimeIncreased)
        {
            if (comboIncreaseScore != score) isFallTimeIncreased = false;
        }
    }
}
