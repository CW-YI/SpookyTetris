using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Witch : MonoBehaviourPun
{
    private int player1_defense_gauge = 0;
    private int player2_defense_gauge = 0;
    private int max_defense_gauge = 5;

    public PvPLineController pvpLineController;
    public StartGameMatch startGameMatch;
    public GameCharacter gameCharacter;

    public GameObject garbageBlockPrefab;
    public Animator animator_p1;
    public Animator animator_p2;

    public GameObject p1_defense_UI;
    public TextMeshProUGUI p1_defense_text;

    public GameObject p2_defense_UI;
    public TextMeshProUGUI p2_defense_text;

    bool hasInit = false;
    // Start is called before the first frame update

    private void Awake()
    {
        Player1_TetrisBlock.OnSendGarbageLinesToOpponent += P1_CheckGarbageLine;
        Player2_TetrisBlock.OnSendGarbageLinesToOpponent += P2_CheckGarbageLine;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (startGameMatch.gameStarted == true && Player1_TetrisBlock.grid_1 != null && Player2_TetrisBlock.grid_2 != null && !hasInit) Init();
        if (startGameMatch.gameStarted == true && PhotonNetwork.LocalPlayer.NickName == "Witch" && hasInit)
        {
            if (PhotonNetwork.IsMasterClient && CheckPassiveWin())
                pvpLineController.ShowResult_P1Win();


            if (!PhotonNetwork.IsMasterClient && CheckPassiveWin())
                pvpLineController.ShowReuslt_P2Win();

        }

        if (Input.GetKeyDown(KeyCode.A)) UseActive();
    }

    #region initialize

    private void Init()
    {
        if (PhotonNetwork.LocalPlayer.NickName != "Witch") return;
        if (PhotonNetwork.IsMasterClient)
        {
            Player1_BlockGrid();
            gameCharacter.player1_maxSkillGauge = 2.0f;

            gameCharacter.player1_skill_2 = true;
            photonView.RPC("SetBoolAsTrue_P1", RpcTarget.All);
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            Player2_BlockGrid();
            gameCharacter.player2_maxSkillGauge = 2.0f;

            gameCharacter.player2_skill_2 = true;
            photonView.RPC("SetBoolAsTrue_P2", RpcTarget.All);
        }

        hasInit = true;
        Debug.Log(hasInit);
    }

    [PunRPC]
    void SetBoolAsTrue_P1()
    {
        p1_defense_UI.SetActive(true);
        gameCharacter.player1_skill_2 = true;
        gameCharacter.player1_maxSkillGauge = 2.0f;
    }
    [PunRPC]
    void SetBoolAsTrue_P2()
    {
        p2_defense_UI.SetActive(true);
        gameCharacter.player2_skill_2 = true;
        gameCharacter.player2_maxSkillGauge = 2.0f;
    }

    void Player1_BlockGrid()
    {
        int emptyXAxis = Random.Range(Player1_TetrisBlock.leftMostXAxis, Player1_TetrisBlock.width + 1);
        float randomChance;
        for (int y = Player1_TetrisBlock.bottomHeight; y < Player1_TetrisBlock.bottomHeight + 10; ++y)
        {
            randomChance = Random.Range(0f, 1f);

            if (randomChance > 0.7f) emptyXAxis = Random.Range(Player1_TetrisBlock.leftMostXAxis, Player1_TetrisBlock.width);

            for (int x = Player1_TetrisBlock.leftMostXAxis; x < Player1_TetrisBlock.width + 1; ++x)
            {
                if (x != emptyXAxis && (PhotonNetwork.IsConnected && PhotonNetwork.InRoom))
                {
                    photonView.RPC("Player1InputOnGrid", RpcTarget.All, x, y);
                }
            }
        }
    }

    void Player2_BlockGrid()
    {
        int emptyXAxis = Random.Range(Player2_TetrisBlock.leftMostXAxis, Player2_TetrisBlock.width);
        float randomChance;

        for (int y = Player2_TetrisBlock.bottomHeight; y < Player2_TetrisBlock.bottomHeight + 10; ++y)
        {
            randomChance = Random.Range(0f, 1f);
            if (randomChance > 0.7f) emptyXAxis = Random.Range(Player2_TetrisBlock.leftMostXAxis, Player2_TetrisBlock.width);

            for (int x = Player2_TetrisBlock.leftMostXAxis; x < Player2_TetrisBlock.width + 1; ++x)
                if (x != emptyXAxis && (PhotonNetwork.IsConnected && PhotonNetwork.InRoom))
                    photonView.RPC("Player2InputOnGrid", RpcTarget.All, x, y);
        }
    }
    [PunRPC]
    void Player1InputOnGrid(int x, int y)
    {
        GameObject garbageBlock = Instantiate(garbageBlockPrefab, new Vector3(x, y, 0), Quaternion.identity);
        Player1_TetrisBlock.grid_1[x, y] = garbageBlock.transform;
        Player1_TetrisBlock.highestHeight = y;
    }

    [PunRPC]
    void Player2InputOnGrid(int x, int y)
    {
        GameObject garbageBlock = Instantiate(garbageBlockPrefab, new Vector3(x, y, 0), Quaternion.identity);
        Player2_TetrisBlock.grid_2[x, y] = garbageBlock.transform;
        Player2_TetrisBlock.highestHeight = y;
    }

    #endregion

    #region active

    void P1_CheckGarbageLine(int rows, string player)
    {
        photonView.RPC("RPC_P1_CheckGarbageLine", RpcTarget.All, rows);
    }
    [PunRPC]
    void RPC_P1_CheckGarbageLine(int rows)
    {
        if (player1_defense_gauge + rows < max_defense_gauge) player1_defense_gauge += rows;
        else player1_defense_gauge = max_defense_gauge;

        p1_defense_text.text = player1_defense_gauge.ToString();
    }
    void P2_CheckGarbageLine(int rows, string player)
    {
        photonView.RPC("RPC_P2_CheckGarbageLine", RpcTarget.All, rows);
    }
    [PunRPC]
    void RPC_P2_CheckGarbageLine(int rows)
    {
        if (player2_defense_gauge + rows < max_defense_gauge) player2_defense_gauge += rows;
        else player2_defense_gauge = max_defense_gauge;

        p2_defense_text.text = player2_defense_gauge.ToString();
    }

    void UseActive()
    {
        if (PhotonNetwork.LocalPlayer.NickName != "Witch") return;

        if (PhotonNetwork.IsMasterClient && gameCharacter.player1_currentSkillGauge == gameCharacter.player1_maxSkillGauge)
        {
            animator_p1.SetTrigger("Attack");
            if (player1_defense_gauge == 0) return;

            if (pvpLineController.rowsToAddPlayer1 < player1_defense_gauge)
            {
                int remainder = player1_defense_gauge - pvpLineController.rowsToAddPlayer1;
                pvpLineController.rowsToAddPlayer1 -= remainder;
                player1_defense_gauge = remainder;
            }
            else
            {
                pvpLineController.rowsToAddPlayer1 -= player1_defense_gauge;
                player1_defense_gauge = 0;
            }
            gameCharacter.player1_currentSkillGauge = 0f;
            photonView.RPC("RPC_P1_CheckGarbageLine", RpcTarget.All, 0);

        }
        else if (!PhotonNetwork.IsMasterClient && gameCharacter.player2_currentSkillGauge == gameCharacter.player2_maxSkillGauge)
        {
            animator_p2.SetTrigger("Attack");
            if (player2_defense_gauge == 0) return;

            if (pvpLineController.rowsToAddPlayer2 < player2_defense_gauge)
            {
                int remainder = player2_defense_gauge - pvpLineController.rowsToAddPlayer2;
                pvpLineController.rowsToAddPlayer2 -= remainder;
                player2_defense_gauge = remainder;
            }
            else
            {
                pvpLineController.rowsToAddPlayer2 -= player2_defense_gauge;
                player2_defense_gauge = 0;
            }

            gameCharacter.player1_currentSkillGauge = 0f;
            photonView.RPC("RPC_P2_CheckGarbageLine", RpcTarget.All, 0);
        }
    }
    #endregion

    #region passive
    bool CheckPassiveWin()
    {
        if (PhotonNetwork.LocalPlayer.NickName != "Witch") return false;

        if (PhotonNetwork.IsMasterClient)
        {
            if (Player1_TetrisBlock.grid_1 == null) return false;

            for (int y = Player1_TetrisBlock.bottomHeight; y < Player1_TetrisBlock.height; ++y)
                for (int x = Player1_TetrisBlock.leftMostXAxis; x < Player1_TetrisBlock.width + 1; ++x)
                    if (Player1_TetrisBlock.grid_1[x, y] != null)
                        if (Player1_TetrisBlock.grid_1[x, y].gameObject.CompareTag("Garbage")) return false;
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            if (Player2_TetrisBlock.grid_2 == null) return false;

            for (int y = Player2_TetrisBlock.bottomHeight; y < Player2_TetrisBlock.height; ++y)
                for (int x = Player2_TetrisBlock.leftMostXAxis; x < Player2_TetrisBlock.width + 1; ++x)
                    if (Player2_TetrisBlock.grid_2[x, y] != null)
                        if (Player2_TetrisBlock.grid_2[x, y].gameObject.CompareTag("Garbage")) return false;
        }

        return true;
    }
    #endregion
}
