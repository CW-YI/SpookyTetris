using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puppeteer : MonoBehaviourPun
{
    // Start is called before the first frame update

    public GameCharacter gameCharacter;
    public StartGameMatch startGameMatch;
    public PvPLineController pvpLineController;

    bool P1_Passive_isTrue = false;
    bool P2_Passive_isTrue = false;

    public Animator animator_p1;
    public Animator animator_p2;

    private string[] requiredTags = { "L", "J", "Z", "S", "T", "O", "I" };

    void Start()
    {
        Player1_TetrisBlock.OnSendGarbageLinesToOpponent += P1_CheckPassive;
        Player2_TetrisBlock.OnSendGarbageLinesToOpponent += P2_CheckPassive;
        Initialize();
    }

    private void OnDestroy()
    {
        Player1_TetrisBlock.OnSendGarbageLinesToOpponent -= P1_CheckPassive;
        Player2_TetrisBlock.OnSendGarbageLinesToOpponent -= P2_CheckPassive;
    }
    void Update()
    {
        if(startGameMatch.gameStarted && Input.GetKeyDown(KeyCode.A)) UseActive();
    }

    #region initalize
    private void Initialize()
    {
        if (PhotonNetwork.LocalPlayer.NickName != "Puppeteer") return;

        if (PhotonNetwork.IsMasterClient)
        {
            gameCharacter.player1_maxSkillGauge = 6f;
            gameCharacter.player1_skill_6 = true;
            photonView.RPC("SetBoolAsTrue_P1", RpcTarget.Others);
        }
        else if (!PhotonNetwork.IsMasterClient) 
        {
            gameCharacter.player2_maxSkillGauge = 6.0f;
            gameCharacter.player2_skill_6 = true;
            photonView.RPC("SetBoolAsTrue_P2", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void SetBoolAsTrue_P1()
    {
        gameCharacter.player1_skill_6 = true;
        gameCharacter.player1_maxSkillGauge = 6.0f;
        gameCharacter.player1_currentSkillGauge = 0f;
    }
    [PunRPC]
    void SetBoolAsTrue_P2()
    {
        gameCharacter.player2_skill_6 = true;
        gameCharacter.player2_maxSkillGauge = 6.0f;
        gameCharacter.player2_currentSkillGauge = 0f;
    }
    #endregion

    #region active
    void UseActive()
    {
        if (PhotonNetwork.LocalPlayer.NickName != "Puppeteer") return;

        if (PhotonNetwork.IsMasterClient && gameCharacter.player1_currentSkillGauge == gameCharacter.player1_maxSkillGauge)
        {
            animator_p1.SetTrigger("Attack");
            gameCharacter.player1_currentSkillGauge = 0f;
            int count = 0;
            Player1_TetrisBlock tetrisBlock = FindAnyObjectByType<Player1_TetrisBlock>();

            Transform block1, block2, block3, block4;
            block1 = tetrisBlock.gameObject.transform.GetChild(0);
            block2 = tetrisBlock.gameObject.transform.GetChild(1);
            block3 = tetrisBlock.gameObject.transform.GetChild(2);
            block4 = tetrisBlock.gameObject.transform.GetChild(3);

            tetrisBlock.gameObject.transform.DetachChildren();

            for (int y = Player1_TetrisBlock.bottomHeight; y < Player1_TetrisBlock.height; ++y)
                for (int x = Player1_TetrisBlock.leftMostXAxis; x <= Player1_TetrisBlock.width; ++ x)
                    if (Player1_TetrisBlock.grid_1[x, y] == null)
                        switch (count)
                        {
                            case 0: block1.transform.position = new Vector3(x, y, 0); Player1_TetrisBlock.grid_1[x, y] = block1; count++; break;
                            case 1: block2.transform.position = new Vector3(x, y, 0); Player1_TetrisBlock.grid_1[x, y] = block2; count++; break;
                            case 2: block3.transform.position = new Vector3(x, y, 0); Player1_TetrisBlock.grid_1[x, y] = block3; count++; break;
                            case 3: block4.transform.position = new Vector3(x, y, 0); Player1_TetrisBlock.grid_1[x, y] = block4; count++; break;
                        }

            tetrisBlock.DeleteBlock();
        }
        else if (!PhotonNetwork.IsMasterClient && gameCharacter.player2_currentSkillGauge == gameCharacter.player2_maxSkillGauge)
        {
            animator_p2.SetTrigger("Attack");
            gameCharacter.player2_currentSkillGauge = 0f;
            int count = 0;
            Player2_TetrisBlock tetrisBlock = FindAnyObjectByType<Player2_TetrisBlock>();
            Transform block1, block2, block3, block4;
            block1 = tetrisBlock.gameObject.transform.GetChild(0); 
            block2 = tetrisBlock.gameObject.transform.GetChild(1);
            block3 = tetrisBlock.gameObject.transform.GetChild(2);
            block4 = tetrisBlock.gameObject.transform.GetChild(3);

            tetrisBlock.gameObject.transform.DetachChildren();

            for (int y = Player2_TetrisBlock.bottomHeight; y < Player2_TetrisBlock.height; ++y)
                for (int x = Player2_TetrisBlock.leftMostXAxis; x <= Player2_TetrisBlock.width; ++x)
                    if (Player2_TetrisBlock.grid_2[x, y] == null)
                        switch (count)
                        {
                            case 0: block1.transform.position = new Vector3(x, y, 0); Player2_TetrisBlock.grid_2[x, y] = block1; count++; break;
                            case 1: block2.transform.position = new Vector3(x, y, 0); Player2_TetrisBlock.grid_2[x, y] = block2; count++; break;
                            case 2: block3.transform.position = new Vector3(x, y, 0); Player2_TetrisBlock.grid_2[x, y] = block3; count++; break;
                            case 3: block4.transform.position = new Vector3(x, y, 0); Player2_TetrisBlock.grid_2[x, y] = block4; count++; break;
                        }

            tetrisBlock.DeleteBlock();
        }

    }
    #endregion

    #region passive

    void P1_CheckPassive(int rows, string player)
    {
        for (int y = Player1_TetrisBlock.bottomHeight; y < Player1_TetrisBlock.height; ++y)
        {
            if (CheckAllTagsInLine(Player1_TetrisBlock.grid_1, y))
            {
                pvpLineController.rowsToAddPlayer1 += 1;
                Debug.Log("Player 1: All tags are present in line " + y);
            }
        }
    }

    void P2_CheckPassive(int rows, string player)
    {
        for (int y = Player2_TetrisBlock.bottomHeight; y < Player2_TetrisBlock.height; ++y)
        {
            if (CheckAllTagsInLine(Player2_TetrisBlock.grid_2, y))
            {
                pvpLineController.rowsToAddPlayer1 += 1;
                Debug.Log("Player 2: All tags are present in line " + y);
            }
        }
    }
    bool CheckAllTagsInLine(Transform[,] grid, int y)
    {
        HashSet<string> foundTags = new HashSet<string>();

        for (int x = 0; x < grid.GetLength(0); ++x)
            if (grid[x, y] != null)
                foreach (string tag in requiredTags)
                    if (grid[x, y].CompareTag(tag))
                    {
                        foundTags.Add(tag);
                        break;
                    }

        return foundTags.Count == requiredTags.Length;
    }
    #endregion
}
