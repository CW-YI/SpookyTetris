using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameCharacter : MonoBehaviourPun
{
    #region 플레이어 1 스킬 게이지 관련 변수 
    public float player1_currentSkillGauge = 0f;
    public float player1_maxSkillGauge = 6f;
    protected float player1_base_skillGaugeIncrementRate = 0.1f;
    protected float player1_lineDelete_skillGaugeIncrementRate = 1f;
    protected float player1_attacked_skillGaugeIncrementRate = 0.5f;

    protected bool player1_hasUsedSpecialRecovery = false;
    public bool player1_skill_4 = false;
    public bool player1_skill_6 = false;
    public bool player1_skill_2 = false;

    #region skill gauge
    public Image player1_skillGauge_2;
    public Image player1_skillGauge_4;
    public Image player1_skillGauge_6;
    #endregion

    #endregion

    #region 플레이어 2 스킬 게이지 관련 변수 
    public float player2_currentSkillGauge = 0f;
    public float player2_maxSkillGauge = 6f;
    protected float player2_base_skillGaugeIncrementRate = 0.1f;
    protected float player2_lineDelete_skillGaugeIncrementRate = 1f;
    protected float player2_attacked_skillGaugeIncrementRate = 0.5f;

    protected bool player2_hasUsedSpecialRecovery = false;
    public bool player2_skill_4 = false;
    public bool player2_skill_6 = false;
    public bool player2_skill_2 = false;

    #region skill gauge
    public Image player2_skillGauge_2;
    public Image player2_skillGauge_4;
    public Image player2_skillGauge_6;
    #endregion

    #endregion

    bool isfindImage = false;

    private void Awake()
    {
        player2_currentSkillGauge = 0f;
        player1_currentSkillGauge = 0f;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Start()
    {
        Initialize();
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "OnlineTetris") return;
        player1_currentSkillGauge = 0f;

        photonView.RPC("UpdateGaugePlayer1", RpcTarget.All, 0f);

        player2_currentSkillGauge = 0f;

        photonView.RPC("UpdateGaugePlayer1", RpcTarget.All, 0f);
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && !player1_hasUsedSpecialRecovery && (Player1_TetrisBlock.highestHeight > Player1_TetrisBlock.height * (2f / 3)))
        {
            if (player1_skillGauge_4 == null && player1_skillGauge_6 == null && player1_skillGauge_2 == null) return;
            Player1_On2Over3Filled_IncreaseSkillGauge();
            player1_hasUsedSpecialRecovery = true;
        }
        if (!PhotonNetwork.IsMasterClient && !player2_hasUsedSpecialRecovery && (Player2_TetrisBlock.highestHeight > Player2_TetrisBlock.height * (2f / 3)))
        {
            if (player2_skillGauge_4 == null && player2_skillGauge_6 == null && player2_skillGauge_2 == null) return;
            Player2_On2Over3Filled_IncreaseSkillGauge();
            player2_hasUsedSpecialRecovery = true;
            Debug.Log("HERE WE GO");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        Player2_TetrisBlock.OnAddToGridFunctionCalled -= Player2_BlockPlacement_IncreaseSkillGauge; //블록 1개 놓을 때마다 +0.1 
        PvPLineController.OnGarbageLinesAddedToPlayer1 -= Player2_OnSendGarbageLines_IncreaseSkillGauge; //적에게 보내는 쓰레기 줄 1줄 당 +1
        PvPLineController.OnGarbageLinesAddedToPlayer2 -= Player2_OnReceiveGarbageLines_IncreaseSkillGauge; //공격받은 쓰레기 줄 1줄 당 +0.5

        Player1_TetrisBlock.OnAddToGridFunctionCalled -= Player1_BlockPlacement_IncreaseSkillGauge;
        PvPLineController.OnGarbageLinesAddedToPlayer2 -= Player1_OnSendGarbageLines_IncreaseSkillGauge;
        PvPLineController.OnGarbageLinesAddedToPlayer1 -= Player1_OnReceiveGarbageLines_IncreaseSkillGauge;
        player2_currentSkillGauge = 0f;
        player1_currentSkillGauge = 0f;
    }

    private void Initialize()
    {
        #region player 2
        if (!PhotonNetwork.IsMasterClient)
        {
            Player2_TetrisBlock.OnAddToGridFunctionCalled += Player2_BlockPlacement_IncreaseSkillGauge; //블록 1개 놓을 때마다 +0.1 
            PvPLineController.OnGarbageLinesAddedToPlayer1 += Player2_OnSendGarbageLines_IncreaseSkillGauge; //적에게 보내는 쓰레기 줄 1줄 당 +1
            PvPLineController.OnGarbageLinesAddedToPlayer2 += Player2_OnReceiveGarbageLines_IncreaseSkillGauge; //공격받은 쓰레기 줄 1줄 당 +0.5

            player2_skillGauge_2.fillAmount = 0;
            player2_skillGauge_4.fillAmount = 0;
            player2_skillGauge_6.fillAmount = 0;
            player2_currentSkillGauge = 0f;
        }
        #endregion

        #region player1
        if (PhotonNetwork.IsMasterClient)
        {
            Player1_TetrisBlock.OnAddToGridFunctionCalled += Player1_BlockPlacement_IncreaseSkillGauge;
            PvPLineController.OnGarbageLinesAddedToPlayer2 += Player1_OnSendGarbageLines_IncreaseSkillGauge;
            PvPLineController.OnGarbageLinesAddedToPlayer1 += Player1_OnReceiveGarbageLines_IncreaseSkillGauge;

            player1_skillGauge_2.fillAmount = 0;
            player1_skillGauge_4.fillAmount = 0;
            player1_skillGauge_6.fillAmount = 0;
            player1_currentSkillGauge = 0f;
        }
        #endregion

        if (PhotonNetwork.LocalPlayer.NickName != "Mummy")
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (Player1_TetrisBlock.leftMostXAxis != 9) Player1_TetrisBlock.leftMostXAxis = 9;
                if (Player1_TetrisBlock.width != 18) Player1_TetrisBlock.width = 18;
                if (Player1_TetrisBlock.bottomHeight != 3) Player1_TetrisBlock.bottomHeight = 3;
            }

            else if (!PhotonNetwork.IsMasterClient)
            {
                if (Player2_TetrisBlock.leftMostXAxis != 32) Player2_TetrisBlock.leftMostXAxis = 32;
                if (Player2_TetrisBlock.width != 41) Player2_TetrisBlock.width = 41;
                if (Player2_TetrisBlock.bottomHeight != 3) Player2_TetrisBlock.bottomHeight = 3;
            }
        }
    }

    #region player 1
    protected void Player1_BlockPlacement_IncreaseSkillGauge() //블록 1개 놓을 때마다 +0.1 
    {
        photonView.RPC("UpdateGaugePlayer1", RpcTarget.All, 0.1f);
    }
    protected void Player1_OnSendGarbageLines_IncreaseSkillGauge(int numberOfRows) //적에게 보내는 쓰레기 줄 1줄 당 +1
    {
        photonView.RPC("UpdateGaugePlayer1", RpcTarget.All, 1.0f * (float)numberOfRows);
    }
    protected void Player1_OnReceiveGarbageLines_IncreaseSkillGauge(int numberOfRows) //공격받은 쓰레기 줄 1줄 당 +0.5
    {
        photonView.RPC("UpdateGaugePlayer1", RpcTarget.All, 0.5f * (float)numberOfRows);
    }
    protected void Player1_On2Over3Filled_IncreaseSkillGauge() // 2/3 채웠을때
    {
        photonView.RPC("UpdateGaugePlayer1", RpcTarget.All, player1_maxSkillGauge);
    }

    [PunRPC]
    protected virtual void UpdateGaugePlayer1(float gauge)
    {
        if (player1_currentSkillGauge < player1_maxSkillGauge)
        {
            player1_currentSkillGauge += gauge;
            if (player1_currentSkillGauge > player1_maxSkillGauge) player1_currentSkillGauge = player1_maxSkillGauge;
        }

        if (player1_skill_4 && player1_skillGauge_4 != null)
        {
            player1_skillGauge_4.fillAmount = Mathf.Clamp01(player1_currentSkillGauge / player1_maxSkillGauge);
        }
        else if (player1_skill_6 && player1_skillGauge_6 != null)
        {
            player1_skillGauge_6.fillAmount = Mathf.Clamp01(player1_currentSkillGauge / player1_maxSkillGauge);
        }
        else if (player1_skill_2 && player1_skillGauge_2 != null)
        {
            player1_skillGauge_2.fillAmount = Mathf.Clamp01(player1_currentSkillGauge / player1_maxSkillGauge);

        }

    }

    #endregion

    #region player 2
    protected void Player2_BlockPlacement_IncreaseSkillGauge() //블록 1개 놓을 때마다 +0.1 
    {
        photonView.RPC("UpdateGaugePlayer2", RpcTarget.All, 0.1f);
    }

    protected void Player2_OnSendGarbageLines_IncreaseSkillGauge(int numberOfRows) //적에게 보내는 쓰레기 줄 1줄 당 +1
    {
        photonView.RPC("UpdateGaugePlayer2", RpcTarget.All, 1.0f * (float)numberOfRows);
    }

    protected void Player2_OnReceiveGarbageLines_IncreaseSkillGauge(int numberOfRows) //공격받은 쓰레기 줄 1줄 당 +0.5
    {
        photonView.RPC("UpdateGaugePlayer2", RpcTarget.All, 0.5f * (float)numberOfRows);
    }

    protected void Player2_On2Over3Filled_IncreaseSkillGauge()
    {
        photonView.RPC("UpdateGaugePlayer2", RpcTarget.All, player2_maxSkillGauge);
    }

    [PunRPC]
    protected virtual void UpdateGaugePlayer2(float gauge)
    {
        if (player2_currentSkillGauge < player2_maxSkillGauge)
        {
            player2_currentSkillGauge += gauge;
            if (player2_currentSkillGauge > player2_maxSkillGauge) player2_currentSkillGauge = player2_maxSkillGauge;
        }


        if (player2_skill_4 && player2_skillGauge_4 != null)
        {
            player2_skillGauge_4.fillAmount = Mathf.Clamp01(player2_currentSkillGauge / player2_maxSkillGauge);
        }
        else if (player2_skill_6 && player2_skillGauge_6 != null)
        {
            player2_skillGauge_6.fillAmount = Mathf.Clamp01(player2_currentSkillGauge / player2_maxSkillGauge);
        }

        else if (player2_skill_2 && player2_skillGauge_2 != null)
        {
            player2_skillGauge_2.fillAmount = Mathf.Clamp01(player2_currentSkillGauge / player2_maxSkillGauge);
        }
    }

    #endregion
}
