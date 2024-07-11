using EasyTransition;
using Photon.Pun;
//using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PvPLineController : MonoBehaviourPun
{
    public GameObject garbageBlockPrefab;
    private Player1_GameComboManager player1_GameComboManager;
    private Player2_GameComboManager player2_GameComboManager;

    public int rowsToAddPlayer1 = 0;
    public int numberOfRowsLastSentByPlayer1 = 0;

    public int rowsToAddPlayer2 = 0;
    public int numberOfRowsLastSentByPlayer2 = 0;

    private int maxNumberOfRowsAtOnce = 7;
    private int max = 17;

    #region 이벤트 함수
    public delegate void GarbageLinesAddedToPlayer1(int numberOfLines);
    public static event GarbageLinesAddedToPlayer1 OnGarbageLinesAddedToPlayer1;

    public delegate void GarbageLinesAddedToPlayer2(int numberOfLines);
    public static event GarbageLinesAddedToPlayer2 OnGarbageLinesAddedToPlayer2;
    #endregion

    #region 게임 진행 변수
    public static int P1_roundWon = 0;
    public static int P2_roundWon = 0;

    public GameObject roundWin;
    #endregion

    #region UI
    public Slider P1_trash_gauge;
    public Slider P2_trash_gauge;

    public GameObject resultScreen;
    public TransitionSettings transition;
    public GameObject player1Canvas;
    public GameObject player2Canvas;
    #endregion

    #region 게임 오버 연출
    private Vector3 targetPosition;
    private Vector3 maskTargetPosition;
    private Vector3 target1Position = new Vector3(9, 5, -10);
    private Vector3 target2Position = new Vector3(43, 5, -10);

    private float zoomspeed = 2.0f;
    private float duration = 2.0f;

    private float currentDistance;
    private Vector3 offset;
    public Camera zoomCamera;
    private float initialSize = 15f;
    private float targetSize = 5f;

    public Material circleMaskMaterial;
    private float maskInitialSize = 1.5f;
    private float maskFinalSize = 0.2f;
    public GameObject CircleWipe;

    public Animator[] Player1SDAnim;
    public Animator[] Player2SDAnim;
    public int player1Index = 0;
    public int player2Index = 0;
    #endregion

    bool isEnding = false;
    private void Awake()
    {

    }

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        Player1_TetrisBlock.OnSendGarbageLinesToOpponent -= Player2_AddGarbageLinesFromOpponent;
        Player2_TetrisBlock.OnSendGarbageLinesToOpponent -= Player1_AddGarbageLinesFromOpponent;

        Player1_TetrisBlock.OnSendGarbageLinesToOpponent -= P2_UpdateTrashGauge;
        Player2_TetrisBlock.OnSendGarbageLinesToOpponent -= P1_UpdateTrashGauge;

        Player1_TetrisBlock.OnAddToGridFunctionCalled -= Player1_CheckAndAddGarbageLines;
        Player2_TetrisBlock.OnAddToGridFunctionCalled -= Player2_CheckAndAddGarbageLines;

        Player1_TetrisBlock.OnGameOver -= ShowReuslt_P2Win;
        Player2_TetrisBlock.OnGameOver -= ShowResult_P1Win;
    }

    void Initialize()
    {
        //StartCoroutine(Initialize(1f));
        Player1_TetrisBlock.OnSendGarbageLinesToOpponent += Player2_AddGarbageLinesFromOpponent;
        Player2_TetrisBlock.OnSendGarbageLinesToOpponent += Player1_AddGarbageLinesFromOpponent;

        Player1_TetrisBlock.OnSendGarbageLinesToOpponent += P2_UpdateTrashGauge;
        Player2_TetrisBlock.OnSendGarbageLinesToOpponent += P1_UpdateTrashGauge;

        Player1_TetrisBlock.OnAddToGridFunctionCalled += Player1_CheckAndAddGarbageLines;
        Player2_TetrisBlock.OnAddToGridFunctionCalled += Player2_CheckAndAddGarbageLines;

        Player1_TetrisBlock.OnGameOver += ShowReuslt_P2Win;
        Player2_TetrisBlock.OnGameOver += ShowResult_P1Win;
    }

    IEnumerator Initialize(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (PhotonNetwork.IsMasterClient)
        {
            P1_trash_gauge = GameObject.FindGameObjectWithTag("P1_Trash_Gauge").GetComponent<Slider>();
            PhotonView player1PhotonView = P1_trash_gauge.GetComponent<PhotonView>();
            Player1SDAnim[player1Index].SetTrigger("Damage");
            //photonView.RPC("SetP1TrashGauge", RpcTarget.All, player1PhotonView.ViewID);
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            P2_trash_gauge = GameObject.FindGameObjectWithTag("P2_Trash_Gauge").GetComponent<Slider>();
            PhotonView player2PhotonView = P2_trash_gauge.GetComponent<PhotonView>();
            Player2SDAnim[player2Index].SetTrigger("Damage");
           // photonView.RPC("SetP2TrashGauge", RpcTarget.All, player2PhotonView.ViewID);
        }

        //resultScreen = GameObject.FindWithTag("GameOverScreen");
    }
    #region trash gauge logic

    void Player1_AddGarbageLinesFromOpponent(int rows, string player)
    {
        int totalRows = CalculateTotalRows(rows, Player2_TetrisBlock.comboCounter);
        photonView.RPC("GarbageLinePlus_FromPlayer2", RpcTarget.All, totalRows);
    }

    void Player2_AddGarbageLinesFromOpponent(int rows, string player)
    {
        int totalRows = CalculateTotalRows(rows, Player1_TetrisBlock.comboCounter);
        photonView.RPC("GarbageLinePlus_FromPlayer1", RpcTarget.All, totalRows);
    }

    int CalculateTotalRows(int rows, int combo)
    {
        int rowsToAdd = 0;
        switch (rows)
        {
            case 1: rowsToAdd += 0; break;
            case 2: rowsToAdd += 1; break;
            case 3: rowsToAdd += 2; break;
            case 4: rowsToAdd += 4; break;
        }

        switch (combo)
        {
            case 0: case 1: rowsToAdd += 0; break;
            case 2: case 3: rowsToAdd += 1; break;
            case 4: case 5: rowsToAdd += 2; break;
            case 6: case 7: rowsToAdd += 3; break;
            case 8: case 9: case 10: rowsToAdd += 4; break;
            default: rowsToAdd += 5; break;
        }

        return rowsToAdd;
    }

    [PunRPC]
    void GarbageLinePlus_FromPlayer2(int rows)
    {
        if (rowsToAddPlayer1 + rows < max) rowsToAddPlayer1 += rows;
        else rowsToAddPlayer1 = max;

        P1_UpdateTrashGauge(rowsToAddPlayer1, " ");
    }

    [PunRPC]
    void GarbageLinePlus_FromPlayer1(int rows)
    {
        if (rowsToAddPlayer2 + rows < max) rowsToAddPlayer2 += rows;
        else rowsToAddPlayer2 = max;

        P2_UpdateTrashGauge(rowsToAddPlayer2, " ");
    }

    void Player1_CheckAndAddGarbageLines()
    {
        if (rowsToAddPlayer1 > 0)
        {
            if (rowsToAddPlayer1 > maxNumberOfRowsAtOnce)
            {
                photonView.RPC("Player1_RowUp", RpcTarget.All, maxNumberOfRowsAtOnce);
                Player1_InsertGarbageLines(maxNumberOfRowsAtOnce);
                numberOfRowsLastSentByPlayer1 = maxNumberOfRowsAtOnce;
                rowsToAddPlayer1 -= maxNumberOfRowsAtOnce;
                OnGarbageLinesAddedToPlayer1?.Invoke(maxNumberOfRowsAtOnce);
            }
            else
            {
                photonView.RPC("Player1_RowUp", RpcTarget.All, rowsToAddPlayer1);
                Player1_InsertGarbageLines(rowsToAddPlayer1);
                numberOfRowsLastSentByPlayer1 = rowsToAddPlayer1;

                OnGarbageLinesAddedToPlayer1?.Invoke(rowsToAddPlayer1);
                rowsToAddPlayer1 = 0;
            }

            photonView.RPC("P1_UpdateTrashGauge_RPC", RpcTarget.All, rowsToAddPlayer1);
        }
    }
    void Player2_CheckAndAddGarbageLines()
    {
        if (rowsToAddPlayer2 > 0)
        {
            if (rowsToAddPlayer2 > maxNumberOfRowsAtOnce)
            {
                photonView.RPC("Player2_RowUp", RpcTarget.All, maxNumberOfRowsAtOnce);
                Player2_InsertGarbageLines(maxNumberOfRowsAtOnce);
                numberOfRowsLastSentByPlayer1 = maxNumberOfRowsAtOnce;
                rowsToAddPlayer2 -= maxNumberOfRowsAtOnce;
                OnGarbageLinesAddedToPlayer2?.Invoke(maxNumberOfRowsAtOnce);

            }
            else
            {
                photonView.RPC("Player2_RowUp", RpcTarget.All, rowsToAddPlayer2);
                Player2_InsertGarbageLines(rowsToAddPlayer2);
                numberOfRowsLastSentByPlayer1 = rowsToAddPlayer2;

                OnGarbageLinesAddedToPlayer2?.Invoke(rowsToAddPlayer2);
                rowsToAddPlayer2 = 0;
            }

            photonView.RPC("P2_UpdateTrashGauge_RPC", RpcTarget.All, rowsToAddPlayer2);
        }
    }

    [PunRPC]
    void Player1_RowUp(int rows)
    {
        for (int  i = 0; i < rows; ++i)
        {
            for (int y = Player1_TetrisBlock.height; y >= Player1_TetrisBlock.bottomHeight; --y)
            {
                for (int x = Player1_TetrisBlock.leftMostXAxis; x <= Player1_TetrisBlock.width; ++x)
                {
                    if (Player1_TetrisBlock.grid_1[x, y] != null)
                    {
                        Player1_TetrisBlock.grid_1[x, y + 1] = Player1_TetrisBlock.grid_1[x, y];
                        Player1_TetrisBlock.grid_1[x, y].transform.position += new Vector3(0, 1, 0);
                        Player1_TetrisBlock.grid_1[x, y] = null;
                    }
                }
            }
        }
    }

    [PunRPC]
    void Player2_RowUp(int rows)
    {
        for (int i = 0; i < rows; ++i)
        {
            for (int y = Player2_TetrisBlock.height; y >= Player2_TetrisBlock.bottomHeight; --y)
                for (int x = Player2_TetrisBlock.leftMostXAxis; x <= Player2_TetrisBlock.width; ++x)
                {
                    if (Player2_TetrisBlock.grid_2[x, y] != null)
                    {
                        Player2_TetrisBlock.grid_2[x, y + 1] = Player2_TetrisBlock.grid_2[x, y];
                        Player2_TetrisBlock.grid_2[x, y].transform.position += new Vector3(0, 1, 0);
                        Player2_TetrisBlock.grid_2[x, y] = null;
                    }
                }

        }
    }

    void Player1_InsertGarbageLines(int rows)
    {
        int emptyXAxis = Random.Range(Player1_TetrisBlock.leftMostXAxis, Player1_TetrisBlock.width + 1);
        float randomChance;
        for (int y = Player1_TetrisBlock.bottomHeight; y < Player1_TetrisBlock.bottomHeight + rows; ++y)
        {
            randomChance = Random.Range(0f, 1f);

            if (randomChance > 0.7f) emptyXAxis = Random.Range(Player1_TetrisBlock.leftMostXAxis, Player1_TetrisBlock.width);

            for (int x = Player1_TetrisBlock.leftMostXAxis; x <= Player1_TetrisBlock.width; ++x)
            {
                if (x != emptyXAxis && (PhotonNetwork.IsConnected && PhotonNetwork.InRoom))
                {
                    photonView.RPC("Player1InputOnGrid", RpcTarget.All, x, y);
                }
            }
        }
    }

    [PunRPC]
    void Player1InputOnGrid(int x, int y)
    {
        GameObject garbageBlock = Instantiate(garbageBlockPrefab, new Vector3(x, y, 0), Quaternion.identity);
        Player1_TetrisBlock.grid_1[x, y] = garbageBlock.transform;
    }

    void Player2_InsertGarbageLines(int rows)
    {
       int emptyXAxis = Random.Range(Player2_TetrisBlock.leftMostXAxis, Player2_TetrisBlock.width);
        float randomChance;

        for (int y = Player2_TetrisBlock.bottomHeight; y < Player2_TetrisBlock.bottomHeight + rows; ++y)
        {
            randomChance = Random.Range(0f, 1f);
            if (randomChance > 0.7f) emptyXAxis = Random.Range(Player2_TetrisBlock.leftMostXAxis, Player2_TetrisBlock.width);

            for (int x = Player2_TetrisBlock.leftMostXAxis; x <= Player2_TetrisBlock.width; ++x) 
                if (x != emptyXAxis && (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)) 
                    photonView.RPC("Player2InputOnGrid", RpcTarget.All, x, y);
        } 
    }

    [PunRPC]
    void Player2InputOnGrid(int x, int y)
    {
        GameObject garbageBlock = Instantiate(garbageBlockPrefab, new Vector3(x, y, 0), Quaternion.identity);
        Player2_TetrisBlock.grid_2[x, y] = garbageBlock.transform;
    }

    #endregion

    #region UI LOGIC

    public void ShowReuslt_P2Win()
    {
        photonView.RPC("ShowResult_RPC", RpcTarget.All, "P2");
    }
    public void ShowResult_P1Win()
    {
        photonView.RPC("ShowResult_RPC", RpcTarget.All, "P1");
    }
    
    [PunRPC]
    void ShowResult_RPC(string player)
    {
        if (player == "P1") P1_roundWon = 1;
        else P2_roundWon = 1;

        player1Canvas.SetActive(false); player2Canvas.SetActive(false);
        CircleWipe.SetActive(true);
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.DestroyAll();
        //DeleteAllGarbageBlock();
        if (!isEnding) StartCoroutine(ZoomInToObject());
    }

    void DeleteAllGarbageBlock()
    {
        //GameObject[] garbageObjects = GameObject.FindGameObjectsWithTag("Garbage");

        //if (garbageObjects != null)
        //{
        //    foreach (GameObject garbageObject in garbageObjects)
        //    {
        //        Destroy(garbageObject);
        //    }
        //}

        //GameObject[] muumyObjects = GameObject.FindGameObjectsWithTag("Unremoveable");

        //if (muumyObjects != null)
        //{
        //    foreach (GameObject muumyObject in muumyObjects)
        //    {
        //        Destroy(muumyObject);
        //    }
        //}
        
    }

    IEnumerator ZoomInToObject()
    {
        isEnding = true;
        Vector3 initialPosition = zoomCamera.transform.position;
        if (P1_roundWon == 1)
        {
            targetPosition = target1Position;
            //maskTargetPosition = target1Position; 2, 8
            maskTargetPosition = new Vector3(target1Position.x, target1Position.y + 8, target1Position.z);

            Player1SDAnim[player1Index].SetTrigger("Win");
        }
        else
        {
            targetPosition = target2Position;
            //maskTargetPosition = target2Position; -10, 10
            maskTargetPosition = new Vector3(target2Position.x - 6, target2Position.y + 10, target2Position.z);
            Player2SDAnim[player2Index].SetTrigger("Win");
        }
        float elapsedTime = 0;

        zoomCamera.orthographicSize = initialSize;
        circleMaskMaterial.SetFloat("_Cutoff", maskInitialSize);
        circleMaskMaterial.SetVector("_Center", zoomCamera.WorldToViewportPoint(maskTargetPosition));

        while (elapsedTime < duration)
        {
            zoomCamera.transform.position = Vector3.Lerp(initialPosition, targetPosition, (elapsedTime / duration));
            zoomCamera.orthographicSize = Mathf.Lerp(initialSize, targetSize, (elapsedTime / duration));

            circleMaskMaterial.SetFloat("_Cutoff", Mathf.Lerp(maskInitialSize, maskFinalSize, (elapsedTime / duration)));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        circleMaskMaterial.SetFloat("_Cutoff", maskFinalSize);
        zoomCamera.transform.position = targetPosition;
        zoomCamera.orthographicSize = targetSize;

        yield return new WaitForSeconds(3f);

        elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            // 마스크 크기 변경
            circleMaskMaterial.SetFloat("_Cutoff", Mathf.Lerp(maskFinalSize, 0f, elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        circleMaskMaterial.SetFloat("_Cutoff", 0);
        CircleWipe.SetActive(false);
        LoadScreenTransition("OnlineTetris");
    }

    void LoadScreenTransition(string _sceneName)
    {
        //TransitionManager.Instance().Transition(null, transition, 0);
        resultScreen.SetActive(true);

        Player1_TetrisBlock P1_TetrisBlock = FindAnyObjectByType<Player1_TetrisBlock>();
        if (P1_TetrisBlock != null)
        {
            P1_TetrisBlock.enabled = false;
        }

        Player2_TetrisBlock P2_TetrisBlock = FindAnyObjectByType<Player2_TetrisBlock>();
        if (P2_TetrisBlock != null)
        {
            P2_TetrisBlock.enabled = false;
        }

        this.enabled = false;

        //while (CircleWipe.activeSelf) CircleWipe.SetActive(false);
    }

    void P1_UpdateTrashGauge(int rows, string player)
    {
        photonView.RPC("P1_UpdateTrashGauge_RPC", RpcTarget.All, rows);
    }

    [PunRPC]
    void P1_UpdateTrashGauge_RPC(int rows)
    {
        rowsToAddPlayer1 = rows;
        P1_trash_gauge.value = Mathf.Clamp01((float)rowsToAddPlayer1 / max);
    }

    void P2_UpdateTrashGauge(int rows, string player)
    {
        photonView.RPC("P2_UpdateTrashGauge_RPC", RpcTarget.All, rows);
    }

    [PunRPC]
    void P2_UpdateTrashGauge_RPC(int rows)
    {
        rowsToAddPlayer2 = rows;
        P2_trash_gauge.value = Mathf.Clamp01((float)rowsToAddPlayer2 / max);

    }
    #endregion
}
