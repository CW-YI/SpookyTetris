using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Player1_TetrisBlock;
//using static Unity.VisualScripting.Metadata;

public class Player1_TetrisBlock : TetrisBlock
{
    #region 블록 움직일때 쓰이는 변수들
    private bool firstInput = true;
    public static bool hasPlacedBlock = false;

    private bool isLastMoveRotation = false;
    private bool isValidPlacement = false;
    #endregion

    #region 화면 경계선 확인할때 쓰이는 변수들
    Vector3 objectPosition;
    Vector3 objectScale;

    public static int leftMostXAxis = 9;
    public static int width = 18;

    public static int bottomHeight = 3;
    public static int height = 23;

    public static Transform[,] grid_1;
    public static bool hasGrid = false;
    #endregion

    #region 고스트 피스
    private GameObject ghostPiece;
    #endregion

    #region 콤보 관련 변수들
    public static int comboCounter = 0;
    public static bool isCombo = false;
    public static int rowsDeleted = 0;
    #endregion

    #region 스킬 게이지 관련 변수들
    public static int highestHeight = 3;
    #endregion

    #region 다른 스크립트 참조
    private Player1_TetrominoSpawner tetrominoSpawner;
    #endregion

    AudioSource audioSource;

    #region 이벤트 함수
    public delegate void AddToGridFunctionCalled();
    public static event AddToGridFunctionCalled OnAddToGridFunctionCalled;

    public delegate void DeleteLineFunctionCalled();
    public static event DeleteLineFunctionCalled OnDeleteLineFunctionCalled;

    public delegate void UpdateComboFunctionCalled();
    public static event UpdateComboFunctionCalled OnUpdateComboFunctionCalled;

    public delegate void SendGarbageLinesToOpponent(int rows, string allBlock);
    public static event SendGarbageLinesToOpponent OnSendGarbageLinesToOpponent;

    public delegate void P1_GameOver();
    public static event P1_GameOver OnGameOver;
    #endregion

    #region 결과창 변수
    public static int numberOfBlocksPlaced = 0;
    public static int numberOfLinesDeleted = 0;
    public static int numberOfActiveSkillUsed = 0;
    public static float piecesPerSecond = 0f;
    public static float linesPerMinute = 0f;
    //public static float timeElapsedInSeconds = 0f;

    private static bool isPlaying = true;
    public static bool isGameOver = false;
    #endregion

    private void Awake()
    {
        tetrominoSpawner = FindAnyObjectByType<Player1_TetrominoSpawner>();
        audioSource = GetComponent<AudioSource>();
    }
    void Start()
    { 
        if (!hasGrid)
        {
            grid_1 = new Transform[width + 3, height + 1];
            hasGrid = true;
        }

        isLastMoveRotation = false;
        isValidPlacement = false;

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient) InstantiateGhostPiece(); // 온라인
    }

    void Update()
    {
        if ((PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient) || !PhotonNetwork.IsConnected)
        {
            if (!IsGameOver())
            {
                MoveTetrominoes_Player1();
                UpdateGhostPiecePosition();
            }
            else
            {
                OnGameOver?.Invoke();
            }
        }

        if (isPlaying)
        {
            //timeElapsedInSeconds += Time.deltaTime;
        }
    }

    void MoveTetrominoes_Player1() //블록 조종 함수
    {
        if (Input.GetKey(KeyCode.LeftArrow)) // 블록 좌로 이동
        {
            if (firstInput == true)
            {
                transform.position += new Vector3(-1, 0, 0);
                firstInput = false;
            }

            if (firstInput == false) inputTimer += Time.deltaTime;

            if (inputTimer > inputDelay)
            {
                transform.position += new Vector3(-1, 0, 0);
                inputTimer = 0;
            }

            if (!ValidMove(gameObject)) transform.position -= new Vector3(-1, 0, 0);
            else placementDelay_timer = 0;

            isLastMoveRotation = false;
            previousRotation = Mathf.RoundToInt(transform.rotation.eulerAngles.z);
        }
        else if (Input.GetKey(KeyCode.RightArrow)) //블록 우로 이동
        {
            if (firstInput == true)
            {
                transform.position += new Vector3(1, 0, 0);
                firstInput = false;
            }

            if (firstInput == false) inputTimer += Time.deltaTime;

            if (inputTimer > inputDelay)
            {
                transform.position += new Vector3(1, 0, 0);
                inputTimer = 0;
            }
            if (!ValidMove(gameObject)) transform.position -= new Vector3(1, 0, 0);
            else placementDelay_timer = 0;

            isLastMoveRotation = false;
            previousRotation = Mathf.RoundToInt(transform.rotation.eulerAngles.z);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) //블록 회전
        {
            placementDelay_timer = 0;
            isValidPlacement = false;

            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
            currentRotation = Mathf.RoundToInt(transform.rotation.eulerAngles.z);

            //0 --> -90, -90 --> 180, 180 --> 90, 90 --> 0

            if ((!ValidMove(gameObject) || GroundCheck()) && !gameObject.CompareTag("I"))
            {
                for (int i = 0; i < 5; ++i)
                {
                    if (previousRotation == 0 && currentRotation == 270)
                        transform.position += new Vector3(WallKick_Clockwise_x[0, i], WallKick_Clockwise_y[0, i], 0);
                    else if (previousRotation == 270 && currentRotation == 180)
                        transform.position += new Vector3(WallKick_Clockwise_x[1, i], WallKick_Clockwise_y[1, i], 0);
                    else if (previousRotation == 180 && currentRotation == 90)
                        transform.position += new Vector3(WallKick_Clockwise_x[2, i], WallKick_Clockwise_y[2, i], 0);
                    else if (previousRotation == 90 && currentRotation == 0)
                        transform.position += new Vector3(WallKick_Clockwise_x[3, i], WallKick_Clockwise_y[3, i], 0);

                    if (ValidMove(gameObject))
                    {
                        isValidPlacement = true;
                        break;
                    }
                }
            }
            if ((!ValidMove(gameObject) || GroundCheck()) && gameObject.CompareTag("I"))
            {
                for (int i = 0; i < 5; ++i)
                {
                    if (previousRotation == 0 && currentRotation == 270)
                    {
                        transform.position += new Vector3(I_WallKick_Clockwise_x[0, i], I_WallKick_Clockwise_y[0, i], 0);
                    }
                    else if (previousRotation == 270 && currentRotation == 180)
                    {
                        transform.position += new Vector3(I_WallKick_Clockwise_x[1, i], I_WallKick_Clockwise_y[1, i], 0);
                    }
                    else if (previousRotation == 180 && currentRotation == 90)
                    {
                        transform.position += new Vector3(I_WallKick_Clockwise_x[2, i], I_WallKick_Clockwise_y[2, i], 0);
                    }
                    else if (previousRotation == 90 && currentRotation == 0)
                    {
                        transform.position += new Vector3(I_WallKick_Clockwise_x[3, i], I_WallKick_Clockwise_y[3, i], 0);
                    }

                    if (ValidMove(gameObject))
                    {
                        isValidPlacement = true;
                        break;
                    }
                }
            }
            else isValidPlacement = true;

            if (!isValidPlacement) transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), +90);
            isLastMoveRotation = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            inputTimer = 0;
            firstInput = true;
        }

        if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10 : fallTime)) //아래 방향키 눌러서 빨리 떨구기
        {
            transform.position += new Vector3(0, -1, 0);
            if (!ValidMove(gameObject))
            {


                transform.position -= new Vector3(0, -1, 0);

                AddToGrid();

                StartCoroutine(AddHighlight(gameObject));

                if (isLastMoveRotation && TSpinCheck()) Debug.Log("T SPIN!");
                CheckForLines();
                Destroy(ghostPiece);

                this.enabled = false;
                FindObjectOfType<Player1_TetrominoSpawner>().NewTetromino();
                tetrominoSpawner.usedHold = false;
                rowsDeleted = 0;



            }
            previousTime = Time.time;
            isLastMoveRotation = false;
        }
        if (Time.time - previousTime > (Input.GetKeyDown(KeyCode.Space) ? fallTime / 10 : fallTime))
        {
            while (ValidMove(gameObject))
            {
                transform.position += new Vector3(0, -1, 0);

                if (!ValidMove(gameObject))
                {
                        transform.position -= new Vector3(0, -1, 0);
                        AddToGrid();
                        StartCoroutine(AddHighlight(gameObject));
                        if (isLastMoveRotation && TSpinCheck()) Debug.Log("T SPIN!");
                        CheckForLines();
                        Destroy(ghostPiece);
                        this.enabled = false;
                        FindObjectOfType<Player1_TetrominoSpawner>().NewTetromino();
                        tetrominoSpawner.usedHold = false;
                        rowsDeleted = 0;
                        break;
                }
            }
            previousRotation = 0;
        }
        if (transform.hierarchyCount == 0) Destroy(gameObject);
        if (GroundCheck()) placementDelay_timer += Time.deltaTime;
    }

    public void DeleteBlock()
    {
        CheckForLines();
        Destroy(ghostPiece);
        this.enabled = false;
        FindObjectOfType<Player1_TetrominoSpawner>().NewTetromino();
        tetrominoSpawner.usedHold = false;
        rowsDeleted = 0;
    }
    bool GroundCheck()
    {
        int lowestY = height, lowestX = 0;
        foreach (Transform children in transform)
        {
            int x = Mathf.RoundToInt(children.position.x), y = Mathf.RoundToInt(children.position.y);
            if (y < lowestY)
            {
                lowestX = x;
                lowestY = y;
            }
        }

        if (lowestY == 0) return true;
        if (grid_1[lowestX, lowestY - 1] != null) return true;
        return false;
    }

    void AddToGrid()
    {
        List<int> positionsX = new List<int>();
        List<int> positionsY = new List<int>();
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            positionsX.Add(roundedX); positionsY.Add(roundedY);
        }
        //audioSource.Play();
        photonView.RPC("RPC_AddToGrid", RpcTarget.All, positionsX.ToArray(), positionsY.ToArray());
        OnAddToGridFunctionCalled?.Invoke();
        highestHeight = GetHighestY();
    }

    public int GetHighestY()
    {
        int highestY = 0;

        for (int x = leftMostXAxis; x <= width; x++)
        {
            for (int y = height; y >= bottomHeight; y--)
            {
                if (grid_1[x, y] != null)
                {
                    if (!grid_1[x, y].gameObject.CompareTag("Unremoveable"))
                    {
                        if (y > highestY)
                        {
                            highestY = y;
                        }
                    }
                    // Continue to next x if a block is found, regardless of its tag
                    break;
                }
            }
        }
        return highestY;
    }


    [PunRPC]
    void RPC_AddToGrid(int[] positionsX, int[] positionsY)
    {
        numberOfBlocksPlaced += 1;

        int index = 0;
        foreach (Transform child in transform)
        {
            if (index < positionsX.Length)
            {
                Vector2Int pos = new Vector2Int(positionsX[index], positionsY[index]);
                index++;
                grid_1[pos.x, pos.y] = child;
            }
        }
    }

    bool TSpinCheck()
    {
        //rotation order (z-axis) = 0, -90, 180, 90
        //iteration order = center, right, left, top when rotation = 0
        int rotation = Mathf.RoundToInt(transform.rotation.eulerAngles.z), count = 0; //코너 갯수 저장

        for (int i = 1; i < transform.childCount - 1; ++i)
        {
            Transform child = transform.GetChild(i);
            int x = Mathf.RoundToInt(child.position.x), y = Mathf.RoundToInt(child.position.y);
            if (rotation == 0 || rotation == 180)
            {
                if (grid_1[x, y + 1] != null) count++;
                if (grid_1[x, y - 1] != null || y - 1 == 0) count++;
            }
            else if (rotation == 90 || rotation == 270)
            {
                if (grid_1[x + 1, y] != null) count++;
                if (grid_1[x - 1, y] != null || x < leftMostXAxis) count++;
            }
        }


        if (count >= 3) return true;
        return false;
    }

    IEnumerator AddHighlight(GameObject piece)
    {
        foreach (Transform child in piece.transform)
        {
            child.gameObject.layer = 6;
            child.gameObject.GetComponent<PostProcessVolume>().profile.TryGetSettings(out Bloom bloom);
            bloom.intensity.value = 20f;

        }

        yield return new WaitForSeconds(0.1f);

        foreach (Transform child in piece.transform)
        {
            child.gameObject.layer = 7;
            child.gameObject.GetComponent<PostProcessVolume>().profile.TryGetSettings(out Bloom bloom);
            bloom.intensity.value = 0f;
        }
    }

    #region 줄 삭제 관련 함수
    public void CheckForLines()
    {
        rowsDeleted = 0;
        int garbageCount = 0;
        for (int i = height - 1; i >= 2; i--)
        {
            
            if (HasLine(i))
            {
                if (HasGarbageLine(i)) garbageCount++;
                rowsDeleted++;
                StartCoroutine(DeleteLine(i));
            }
        }
        UpdateCombo(rowsDeleted);
        if (rowsDeleted > 0)
        {
            OnSendGarbageLinesToOpponent?.Invoke(rowsDeleted, "Player1");
        }
    }
    bool HasLine(int i)
    {
        for (int j = leftMostXAxis; j <= width; ++j)
        {
            if (grid_1[j, i] == null || (grid_1[j, i] != null && grid_1[j, i].gameObject.CompareTag("Unremoveable"))) return false;
        }

        return true;
    }

    bool HasGarbageLine(int i)
    {
        for (int j = leftMostXAxis; j <= width; ++j)
        {
            if (grid_1[j, i].gameObject.CompareTag("Garbage")) return true;
        }

        return false;
    }

    IEnumerator DeleteLine(int i)
    {
        OnDeleteLineFunctionCalled?.Invoke();
        for (int j = leftMostXAxis; j <= width; ++j)
        {
            grid_1[j, i].gameObject.layer = 6;
            grid_1[j, i].gameObject.GetComponent<PostProcessVolume>().profile.TryGetSettings(out Bloom bloom);
            bloom.intensity.value = 20f;
        }

        numberOfLinesDeleted += 1;
        yield return new WaitForSeconds(0.2f);

        photonView.RPC("deleteLine", RpcTarget.All, i);
    }


    [PunRPC]
    void deleteLine(int i)
    {
        for (int j = leftMostXAxis; j <= width; ++j)
        {
            if (grid_1[j, i] != null)
            {
                Destroy(grid_1[j, i].gameObject);
                grid_1[j, i] = null;
            }
            //else Debug.Log(grid_1[j, i].name);
        }

        for (int y = i; y < height; ++y)
        {
            for (int j = leftMostXAxis; j <= width; ++j)
            {
                if (grid_1[j, y] != null)
                {
                    grid_1[j, y - 1] = grid_1[j, y];
                    grid_1[j, y] = null;
                    grid_1[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }

    void UpdateCombo(int rows)
    {
        if (rows > 0 && rows < 4)
        {
            if (comboCounter == 0 && isCombo == false)
            {
                comboCounter += 1;
                isCombo = true;
            }
            else if (comboCounter > 0 && isCombo)
            {
                comboCounter += 1;
                isCombo = true;
            }
        }
        else if (comboCounter > 0 && rows == 0 && isCombo)
        {
            comboCounter = 0;
            isCombo = false;
        }
        else if (rows == 4) //4줄 지우기 보너스 구현하는 곳
        {
            if (comboCounter == 0 && isCombo == false)
            {
                comboCounter += 1;
                isCombo = true;
            }
            else if (comboCounter > 0 && isCombo)
            {
                comboCounter += 1;
                isCombo = true;
            }
        }
        OnUpdateComboFunctionCalled?.Invoke();
    }
    #endregion

    bool ValidMove(GameObject piece)
    {
        foreach (Transform children in piece.transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if (roundedX < leftMostXAxis)
            {
                piece.transform.position += new Vector3(1, 0, 0);
            }
            else if (roundedX > width)
            {
                piece.transform.position += new Vector3(-1, 0, 0);
            }
            else if (roundedY < bottomHeight || roundedY >= height) return false; 

            if (grid_1[roundedX, roundedY] != null) return false;

            //if (grid_1[roundedX, roundedY] != null && grid_1[roundedX, roundedY].gameObject.CompareTag("Garbage")) return false;
        }
        return true;
    }

    
    #region 고스트 피스 관련 함수
    public void InstantiateGhostPiece()
    {
        ghostPiece = Instantiate(ghostPrefab, transform.position, Quaternion.identity);
    }

    public void DestroyGhostPiece()
    {
        Destroy(ghostPiece);
    }
    
    void UpdateGhostPiecePosition()
    {
        if(ghostPiece != null) ghostPiece.transform.SetPositionAndRotation(transform.position, transform.rotation);

        while (ValidMove(ghostPiece))
        {
            ghostPiece.transform.position += new Vector3(0, -1, 0);
        }

        ghostPiece.transform.position -= new Vector3(0, -1, 0);
    }
    #endregion

    #region 게임 오버 관련 함수
    public void RestartGame()
    {
        OnGameOver?.Invoke();

        if (PvPLineController.P1_roundWon == 2)
        {
            tetrominoSpawner.enabled = false;
            if (ghostPiece != null) ghostPiece.SetActive(false);
            gameObject.SetActive(false);
            SceneManager.LoadScene("Game_GameOver");
        }
        else
        {
            tetrominoSpawner.enabled = false;
            PvPLineController.P1_roundWon++;

            photonView.RPC("ResetGrid", RpcTarget.All);
            StartCoroutine("Reinstantiate");
        }
    }

    [PunRPC]
    void ResetGrid()
    {
        for (int x = leftMostXAxis; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (grid_1[x, y] != null)
                {
                    Destroy(grid_1[x, y].gameObject);
                    grid_1[x, y] = null;
                }
            }
        }
    }

    IEnumerator Reinstantiate()
    {
        yield return new WaitForSeconds(3f);
        tetrominoSpawner.enabled = true;
        Debug.Log("REINSTANTIATE");
        this.enabled = false;
        FindObjectOfType<Player1_TetrominoSpawner>().NewTetromino();
        Destroy(gameObject);
    }
    bool IsGameOver()
    {
        for (int i = leftMostXAxis; i < width; i++)
        {
            if (grid_1[i, height] != null) return true;
        }
        return false;
    }
    #endregion
}
