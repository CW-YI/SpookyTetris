using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ghost : MonoBehaviourPun
{
    private PvPLineController pvpLineController;
    private TextMeshProUGUI passiveText;

    /// <summary>
    /// 플레이어 1이 마지막으로 지운 줄의 갯수
    /// </summary>
    [SerializeField] private int player1NumberOfRows = 0; 
    /// <summary>
    /// 플레이어 2가 마지막으로 지운 줄의 갯수
    /// </summary>
    [SerializeField] private int player2NumberOfRows = 0;

    private bool player1_hasUsedPassive = false;
    private bool player2_hasUsedPassive = false;

    public GameCharacter gameCharacter;
    public Animator animator_p1;
    public Animator animator_p2;
    private void Awake()
    {
        gameCharacter.player2_currentSkillGauge = 0f;
        gameCharacter.player1_currentSkillGauge = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log("P1:" +  gameCharacter.player1_currentSkillGauge);
        Debug.Log("P2:" + gameCharacter.player2_currentSkillGauge);
    }
    void Start()
    {
        Initialize();

        pvpLineController = FindAnyObjectByType<PvPLineController>();

        Player1_TetrisBlock.OnSendGarbageLinesToOpponent += SetNumberOfLinesForP1;
        Player2_TetrisBlock.OnSendGarbageLinesToOpponent += SetNumberOfLinesForP2;
    }

    private void OnDestroy()
    {
        Player1_TetrisBlock.OnSendGarbageLinesToOpponent -= SetNumberOfLinesForP1;
        Player2_TetrisBlock.OnSendGarbageLinesToOpponent -= SetNumberOfLinesForP2;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        gameCharacter.player1_skill_6 = false;
        gameCharacter.player2_skill_6 = false;
        gameCharacter.player1_currentSkillGauge = 0f;
        gameCharacter.player2_currentSkillGauge = 0f;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "OnlineTetris") Initialize();
    }

    private void Initialize()
    {
        
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.NickName == "Ghost")
            {
                gameCharacter.player1_maxSkillGauge = 6.0f;
                gameCharacter.player1_currentSkillGauge = 0f;
                gameCharacter.player1_skill_6 = true;
                photonView.RPC("SetBoolAsTrue_P1", RpcTarget.Others);
            }
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.NickName == "Ghost")
            {
                gameCharacter.player2_maxSkillGauge = 6.0f;
                gameCharacter.player2_currentSkillGauge = 0f;
                gameCharacter.player2_skill_6 = true;
                photonView.RPC("SetBoolAsTrue_P2", RpcTarget.MasterClient);
            }
        }
        else enabled = false;

        pvpLineController = FindAnyObjectByType<PvPLineController>();
    }
    
    [PunRPC]
    void SetBoolAsTrue_P1()
    {
        gameCharacter.player1_skill_4 = false;
        gameCharacter.player1_skill_6 = true;
        gameCharacter.player1_maxSkillGauge = 6.0f;
        gameCharacter.player1_currentSkillGauge = 0f;
    }
    [PunRPC]
    void SetBoolAsTrue_P2()
    {
        gameCharacter.player2_skill_4 = false; 
        gameCharacter.player2_skill_6 = true;
        gameCharacter.player2_maxSkillGauge = 6.0f;
        gameCharacter.player2_currentSkillGauge = 0f;
    }

    void Update()
    {
        Ghost_PassiveAbility();
        Ghost_ActiveAbility();
    }

    private void Ghost_PassiveAbility()
    {
        if (player1NumberOfRows == player2NumberOfRows && (player1NumberOfRows > 0 && player2NumberOfRows > 0))
        {
            if (PhotonNetwork.IsMasterClient && !player1_hasUsedPassive)
            {
                if (PhotonNetwork.LocalPlayer.NickName == "Ghost")
                {
                    photonView.RPC("AddRowToPlayer2", RpcTarget.All);
                }
            }
            else if (!PhotonNetwork.IsMasterClient && !player2_hasUsedPassive)
            {
                if (PhotonNetwork.LocalPlayer.NickName == "Ghost")
                {
                    photonView.RPC("AddRowToPlayer1", RpcTarget.All);
                }
            }
        }
    }


    private void Ghost_ActiveAbility()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.LocalPlayer.NickName == "Ghost")
                {
                    if ((int)gameCharacter.player1_currentSkillGauge < (int)gameCharacter.player1_maxSkillGauge)
                    {
                        Debug.Log("Player1: Not enough skill gauge");
                        Debug.Log("current: " + gameCharacter.player1_currentSkillGauge + "max : " + gameCharacter.player1_maxSkillGauge);
                    }
                    else if (player2NumberOfRows == 0)
                    {
                        Debug.Log("Player1: Cannot use skill now");
                        Debug.Log("rows:" + player2NumberOfRows);
                    }
                    else
                    {
                        Player1_TetrisBlock.numberOfActiveSkillUsed += 1;
                        photonView.RPC("UseSkillOnPlayer2", RpcTarget.AllBuffered, player2NumberOfRows);
                        animator_p1.SetTrigger("Attack");
                    }
                } 
            }
            else if (!PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.LocalPlayer.NickName == "Ghost")
                {
                    if (gameCharacter.player2_currentSkillGauge < gameCharacter.player2_maxSkillGauge)
                    {
                        Debug.Log("Player2: Not enough skill gauge");
                    }
                    else if (player1NumberOfRows == 0)
                    {
                        Debug.Log("Player2: Cannot use skill now");
                    }
                    else
                    {
                        Player2_TetrisBlock.numberOfActiveSkillUsed += 1;
                        photonView.RPC("UseSkillOnPlayer1", RpcTarget.AllBuffered, player1NumberOfRows);
                        animator_p2.SetTrigger("Attack");
                    }
                }       
            }
        }
    }

    #region player 1
    /// <summary>
    /// 플레이어 1이 마지막으로 지운 줄의 갯수를 설정하는 함수
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="player"></param>
    void SetNumberOfLinesForP1(int rows, string player)
    {
        photonView.RPC("SetNumberOfLines_P1", RpcTarget.All, rows);
        player2_hasUsedPassive = false;
    }
    [PunRPC]
    void SetNumberOfLines_P1(int rows)
    {
        player1NumberOfRows = rows;
    }

    [PunRPC]
    void AddRowToPlayer1()
    {
        pvpLineController.rowsToAddPlayer1 += 1;
        player1_hasUsedPassive = true;
        player1NumberOfRows = 0;
        player2NumberOfRows = 0;
    }
    [PunRPC]
    void UseSkillOnPlayer2(int rows)
    {
        pvpLineController.rowsToAddPlayer2 += rows;
        gameCharacter.player1_currentSkillGauge = 0f;

        if (gameCharacter.player1_skill_4) gameCharacter.player1_skillGauge_4.fillAmount = 0;
        else if (gameCharacter.player1_skill_6) gameCharacter.player1_skillGauge_6.fillAmount = 0;

    }
    #endregion

    #region player 2
    /// <summary>
    /// 플레이어 2가 마지막으로 지운 줄의 갯수를 설정하는 함수
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="player"></param>
    void SetNumberOfLinesForP2(int rows, string player)
    {
        photonView.RPC("SetNumberOfLines_P2", RpcTarget.All, rows);
    }
    [PunRPC]
    void SetNumberOfLines_P2(int rows)
    {
        player2NumberOfRows = rows;
        player1_hasUsedPassive = false;
    }
    [PunRPC]
    void AddRowToPlayer2()
    {
        pvpLineController.rowsToAddPlayer2 += 1;
        player2_hasUsedPassive = false;
        player1NumberOfRows = 0;
        player2NumberOfRows = 0;
    }

    [PunRPC]
    void UseSkillOnPlayer1(int rows)
    {
        pvpLineController.rowsToAddPlayer1 += rows;
        gameCharacter.player2_currentSkillGauge = 0f;
        if (gameCharacter.player2_skill_4) gameCharacter.player2_skillGauge_4.fillAmount = 0;
        else if (gameCharacter.player2_skill_6) gameCharacter.player2_skillGauge_6.fillAmount = 0;
    }
    #endregion

    //#region inheritance
    //[PunRPC]
    //protected override void UpdateGaugePlayer1(float gauge)
    //{
    //    base.UpdateGaugePlayer1(gauge);
    //}
    //[PunRPC]
    //protected override void UpdateGaugePlayer2(float gauge)
    //{
    //    base.UpdateGaugePlayer2(gauge);
    //}
    //#endregion
}
