using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreen : MonoBehaviourPunCallbacks
{
    #region p1
    public TextMeshProUGUI p1_score_UI;

    public TextMeshProUGUI p1_piecesPlaced_UI;

    public TextMeshProUGUI p1_piecesPerSecond_UI;

    public TextMeshProUGUI p1_lines_UI;

    public TextMeshProUGUI p1_linesPerMinute_UI;

    public TextMeshProUGUI p1_activeSkillUsed_UI;

    public GameObject p1Win;
    public GameObject p1Lose;
    float p1_timeElapsed;
    #endregion

    #region p2
    public TextMeshProUGUI p2_score_UI;
    int p2_score;

    public TextMeshProUGUI p2_piecesPlaced_UI;
    int p2_piecesPlaced;

    public TextMeshProUGUI p2_piecesPerSecond_UI;
    int p2_piecesPerSecond;

    public TextMeshProUGUI p2_lines_UI;
    int p2_liens;

    public TextMeshProUGUI p2_linesPerMinute_UI;
    int p2_linesPerMinute;

    public TextMeshProUGUI p2_activeSkillUsed_UI;
    int p2_activeSkillUsed;

    public GameObject p2Win;
    public GameObject p2Lose;
    float p2_timeElapsed;
    #endregion


    public override void OnEnable()
    {
        if (PvPLineController.P1_roundWon > PvPLineController.P2_roundWon)
        {
            p1Win.SetActive(true); p2Lose.SetActive(true);
            Debug.Log("player1win");
        }
        else
        {
            p2Win.SetActive(true); p1Lose.SetActive(true);
            Debug.Log("player2win");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // Master client calculates P1 values
            CalculateAndUpdateP1Values();
        }
        else
        {
            // Other clients calculate P2 values
            CalculateAndUpdateP2Values();
        }
    }
    void CalculateAndUpdateP1Values()
    {
        Player1_TetrisBlock.piecesPerSecond = (float)Player1_TetrisBlock.numberOfBlocksPlaced / StartGameMatch.timeElapsed;
        Player1_TetrisBlock.linesPerMinute = ((float)Player1_TetrisBlock.numberOfLinesDeleted / StartGameMatch.timeElapsed) * 60f;

        photonView.RPC("SyncP1Values", RpcTarget.All, Player1_TetrisBlock.piecesPerSecond, Player1_TetrisBlock.linesPerMinute);
    }

    void CalculateAndUpdateP2Values()
    {
        Player2_TetrisBlock.piecesPerSecond = (float)Player2_TetrisBlock.numberOfBlocksPlaced / StartGameMatch.timeElapsed;

        Player2_TetrisBlock.linesPerMinute = ((float)Player2_TetrisBlock.numberOfLinesDeleted / StartGameMatch.timeElapsed) * 60f;

        photonView.RPC("SyncP2Values", RpcTarget.All, Player2_TetrisBlock.piecesPerSecond, Player2_TetrisBlock.linesPerMinute);
    }

    [PunRPC]
    private void SyncP1Values(float piecesPerSecond, float linesPerMinute)
    {
        p1_score_UI.text = PvPLineController.P1_roundWon.ToString();
        p1_piecesPlaced_UI.text = Player1_TetrisBlock.numberOfBlocksPlaced.ToString();
        p1_piecesPerSecond_UI.text = float.IsNaN(piecesPerSecond) ? "0.00" : piecesPerSecond.ToString("F2");
        p1_lines_UI.text = Player1_TetrisBlock.numberOfLinesDeleted.ToString();
        p1_linesPerMinute_UI.text = float.IsNaN(linesPerMinute) ? "0.00" : linesPerMinute.ToString("F2");
        p1_activeSkillUsed_UI.text = Player1_TetrisBlock.numberOfActiveSkillUsed.ToString();
    }

    [PunRPC]
    private void SyncP2Values(float piecesPerSecond, float linesPerMinute)
    {
        p2_score_UI.text = PvPLineController.P2_roundWon.ToString();
        p2_piecesPlaced_UI.text = Player2_TetrisBlock.numberOfBlocksPlaced.ToString();
        p2_piecesPerSecond_UI.text = float.IsNaN(piecesPerSecond) ? "0.00" : piecesPerSecond.ToString("F2");
        p2_lines_UI.text = Player2_TetrisBlock.numberOfLinesDeleted.ToString();
        p2_linesPerMinute_UI.text = float.IsNaN(linesPerMinute) ? "0.00" : linesPerMinute.ToString("F2");
        p2_activeSkillUsed_UI.text = Player2_TetrisBlock.numberOfActiveSkillUsed.ToString();
    }

    public void ReturnToLobby()
    {
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    PhotonNetwork.DestroyAll();
        //}

        StartCoroutine(LeaveRoomAndLoadLobby());
    }

    private IEnumerator LeaveRoomAndLoadLobby()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Leaving room.");
            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom) yield return null; // Wait until leaving the room is complete
        }

        if (PhotonNetwork.InLobby)
        {
            Debug.Log("Leaving lobby.");
            PhotonNetwork.LeaveLobby();
            while (PhotonNetwork.InLobby) yield return null; // Wait until leaving the lobby is complete
        }

        Debug.Log("Disconnecting.");
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected) yield return null; // Wait until disconnecting is complete

        PhotonNetwork.LoadLevel("OnlineLobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        // Handle disconnection, maybe return to the main menu or handle state clean up
        Debug.Log("Disconnected from Photon. Cause: " + cause);
        SceneManager.LoadScene("MainMenu");
    }

}
