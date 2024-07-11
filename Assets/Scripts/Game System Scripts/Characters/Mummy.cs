using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mummy : MonoBehaviourPun
{
    public static Mummy Instance { get; private set; }

    public GameObject unremoveable_Block;
    public GameObject warning_Block;

    public GameCharacter gameCharacter;
    public StartGameMatch startGameMatch;

    public Animator animator_p1;
    public Animator animator_p2;

    int P1_original_left = 9;
    int P1_original_width = 18;
    int P1_original_bottom = 3;

    int P2_original_left = 32;
    int P2_original_width = 41;
    int P2_original_bottom = 3;
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
    }

    private void Update()
    {
        if (startGameMatch.gameStarted) Initialize();
        Trigger_MummyActive();
    }

    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //if (scene.name == "OnlineTetris") Initialize();
    }

    private void Initialize()
    {
        if (PhotonNetwork.LocalPlayer.NickName != "Mummy")
        {
            enabled = false;
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("BlockGrid", RpcTarget.All, 1);
            gameCharacter.player1_maxSkillGauge = 4.0f;
            Player1_TetrisBlock.leftMostXAxis = 10;
            Player1_TetrisBlock.width = 17;
            Player1_TetrisBlock.bottomHeight = 7;
            
            gameCharacter.player1_skill_4 = true;
            photonView.RPC("SetBoolAsTrue_P1", RpcTarget.Others);
        }


        else if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("BlockGrid", RpcTarget.All, 2);
            gameCharacter.player2_maxSkillGauge = 4.0f;
            Player2_TetrisBlock.leftMostXAxis = 33;
            Player2_TetrisBlock.width = 40;
            Player2_TetrisBlock.bottomHeight = 7;
            
            gameCharacter.player2_skill_4 = true;
            photonView.RPC("SetBoolAsTrue_P2", RpcTarget.MasterClient);

        }
    }
    [PunRPC]
    private void Uninit()
    {
        if (PhotonNetwork.LocalPlayer.NickName != "Mummy") return;

        if (PhotonNetwork.IsMasterClient)
        {
            Player1_TetrisBlock.leftMostXAxis = 9;
            Player1_TetrisBlock.width = 18;
            Player1_TetrisBlock.bottomHeight = 3;

            gameCharacter.player1_skill_4 = false;
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("P2 reverted");
            Player2_TetrisBlock.leftMostXAxis = 32;
            Player2_TetrisBlock.width = 41;
            Player2_TetrisBlock.bottomHeight = 3;

            gameCharacter.player2_skill_4 = false;
        }

        gameCharacter.player1_currentSkillGauge = 0f;
        gameCharacter.player2_currentSkillGauge = 0f;
    }
   

    [PunRPC]
    void SetBoolAsTrue_P1()
    {
        gameCharacter.player1_skill_4 = true;
        gameCharacter.player1_skill_6 = false;
        gameCharacter.player1_maxSkillGauge = 4.0f;
        gameCharacter.player1_currentSkillGauge = 0f;
        Player1_TetrisBlock.highestHeight += 4;
    }
    [PunRPC]
    void SetBoolAsTrue_P2()
    {
        gameCharacter.player2_skill_4 = true;
        gameCharacter.player2_skill_6 = false;
        gameCharacter.player2_maxSkillGauge = 4.0f;
        gameCharacter.player2_currentSkillGauge = 0f;
        Player2_TetrisBlock.highestHeight += 4;
    }
    [PunRPC]
    void BlockGrid(int player)
    {
        if (player == 1)
        {
            for (int y = P1_original_bottom; y < Player1_TetrisBlock.height; ++y)
            {
                if (y >= P1_original_bottom + 4)
                {
                    Instantiate(unremoveable_Block, new Vector3(P1_original_left, y, 0f), Quaternion.identity);
                    Instantiate(unremoveable_Block, new Vector3(P1_original_width, y, 0f), Quaternion.identity);
                }
            }

            for (int y = P1_original_bottom; y < P1_original_bottom + 4; ++y)
            {
                for (int x = P1_original_left; x <= P1_original_width; ++x)
                {
                    Instantiate(unremoveable_Block, new Vector3(x, y, 0f), Quaternion.identity);
                }
            }
        }

        else if (player == 2)
        {
            for (int y = P2_original_bottom; y < Player2_TetrisBlock.height; ++y)
            {
                if (y >= P2_original_bottom + 4)
                {
                    Instantiate(unremoveable_Block, new Vector3(P2_original_left, y, 0f), Quaternion.identity);
                    Instantiate(unremoveable_Block, new Vector3(P2_original_width, y, 0f), Quaternion.identity);
                }
            }

            for (int y = P2_original_bottom; y < P2_original_bottom + 4; ++y)
            {
                for (int x = P2_original_left; x <= P2_original_width; ++x)
                {
                    Instantiate(unremoveable_Block, new Vector3(x, y, 0f), Quaternion.identity);
                }
            }
        }
    }
    private void Trigger_MummyActive()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (PhotonNetwork.IsMasterClient && gameCharacter.player1_currentSkillGauge == gameCharacter.player1_maxSkillGauge)
            {
                if (PhotonNetwork.LocalPlayer.NickName == "Mummy")
                {
                    photonView.RPC("Mummy_ActiveAbility", RpcTarget.All);
                    animator_p1.SetTrigger("Attack");
                }
            }
            else if (!PhotonNetwork.IsMasterClient && gameCharacter.player2_currentSkillGauge == gameCharacter.player2_maxSkillGauge)
            {
                if (PhotonNetwork.LocalPlayer.NickName == "Mummy")
                {
                    photonView.RPC("Mummy_ActiveAbility", RpcTarget.All);
                    animator_p2.SetTrigger("Attack");
                }
            }
        }
    }
    [PunRPC]
    private IEnumerator Mummy_ActiveAbility()
    {
        if (PhotonNetwork.IsMasterClient && gameCharacter.player1_currentSkillGauge == gameCharacter.player1_maxSkillGauge)
        {
            if (PhotonNetwork.LocalPlayer.NickName == "Mummy")
            {
                Player1_TetrisBlock.numberOfActiveSkillUsed += 1;
                gameCharacter.player1_currentSkillGauge = 0f;

                if (gameCharacter.player1_skill_4) gameCharacter.player1_skillGauge_4.fillAmount = 0;
                else if (gameCharacter.player1_skill_6) gameCharacter.player1_skillGauge_6.fillAmount = 0;

                Debug.Log("height:" + Player2_TetrisBlock.highestHeight);

                photonView.RPC("ShowWarning", RpcTarget.All, Player2_TetrisBlock.leftMostXAxis, Player2_TetrisBlock.highestHeight);
                yield return new WaitForSeconds(2);
                photonView.RPC("CreateBlocks", RpcTarget.All, Player2_TetrisBlock.leftMostXAxis, Player2_TetrisBlock.highestHeight);
                yield return new WaitForSeconds(5);
                photonView.RPC("DestroyBlocks", RpcTarget.All, Player2_TetrisBlock.leftMostXAxis, Player2_TetrisBlock.highestHeight);
            }
        }
        else if (!PhotonNetwork.IsMasterClient && gameCharacter.player2_currentSkillGauge == gameCharacter.player2_maxSkillGauge)
        {
            if (PhotonNetwork.LocalPlayer.NickName == "Mummy")
            {
                Player2_TetrisBlock.numberOfActiveSkillUsed += 1;

                gameCharacter.player2_currentSkillGauge = 0f;

                if (gameCharacter.player2_skill_4) gameCharacter.player2_skillGauge_4.fillAmount = 0f;
                else if (gameCharacter.player2_skill_6) gameCharacter.player2_skillGauge_6.fillAmount = 0f;

                photonView.RPC("ShowWarning", RpcTarget.All, Player1_TetrisBlock.leftMostXAxis, Player1_TetrisBlock.highestHeight);
                yield return new WaitForSeconds(2);
                photonView.RPC("CreateBlocks", RpcTarget.All, Player1_TetrisBlock.leftMostXAxis, Player1_TetrisBlock.highestHeight);
                yield return new WaitForSeconds(5);
                photonView.RPC("DestroyBlocks", RpcTarget.All, Player1_TetrisBlock.leftMostXAxis, Player1_TetrisBlock.highestHeight);
            }
        }
    }
    [PunRPC]
    IEnumerator ShowWarning(int leftMostX, int highestY)
    {
        GameObject block1 = Instantiate(warning_Block, new Vector3(leftMostX + 2, highestY + 3, 0), Quaternion.identity);
        GameObject block2 = Instantiate(warning_Block, new Vector3(leftMostX + 5, highestY + 3, 0), Quaternion.identity);
        SpriteRenderer r1 = block1.GetComponent<SpriteRenderer>();
        SpriteRenderer r2 = block2.GetComponent<SpriteRenderer>();


        for (int i = 0; i < 10; ++i)
        {
            r1.color = Color.red; r2.color = Color.red;
            yield return new WaitForSeconds(0.2f);

            r1.color = Color.grey; r2.color = Color.grey;
            yield return new WaitForSeconds(0.2f);
        }
        
        Destroy(block1);
        Destroy(block2);
    }

    [PunRPC]
    private void CreateBlocks(int leftMostX, int highestY)
    {
        GameObject block1 = Instantiate(unremoveable_Block, new Vector3(leftMostX + 2, highestY + 3, 0), Quaternion.identity);
        GameObject block2 = Instantiate(unremoveable_Block, new Vector3(leftMostX + 5, highestY + 3, 0), Quaternion.identity);

        if (leftMostX < Player1_TetrisBlock.width)
        {
            Player1_TetrisBlock.grid_1[leftMostX + 2, highestY + 3] = block1.transform;
            Player1_TetrisBlock.grid_1[leftMostX + 5, highestY + 3] = block2.transform;
        }
        else if (leftMostX > Player1_TetrisBlock.width)
        {
            Player2_TetrisBlock.grid_2[leftMostX + 2, highestY + 3] = block1.transform;
            Player2_TetrisBlock.grid_2[leftMostX + 5, highestY + 3] = block2.transform;
        }
    }

    [PunRPC]
    private void DestroyBlocks(int leftMostX, int highestY)
    {
        if (leftMostX < Player1_TetrisBlock.width)
        {
            Destroy(Player1_TetrisBlock.grid_1[leftMostX + 2, highestY + 3].gameObject);
            Destroy(Player1_TetrisBlock.grid_1[leftMostX + 5, highestY + 3].gameObject);

            Player1_TetrisBlock.grid_1[leftMostX + 5, highestY + 3] = null;
            Player1_TetrisBlock.grid_1[leftMostX + 5, highestY + 3] = null;
        }
        else if (leftMostX > Player1_TetrisBlock.width)
        {
            Destroy(Player2_TetrisBlock.grid_2[leftMostX + 2, highestY + 3].gameObject);
            Destroy(Player2_TetrisBlock.grid_2[leftMostX + 5, highestY + 3].gameObject);

            Player2_TetrisBlock.grid_2[leftMostX + 2, highestY + 3] = null;
            Player2_TetrisBlock.grid_2[leftMostX + 5, highestY + 3] = null;
        }
    }

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
