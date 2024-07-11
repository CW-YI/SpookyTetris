using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player1_TetrominoSpawner : TetrominoSpawner
{
    public GameObject[] Tetrominoes;
    public GameObject nextTetrominoLocation_1;
    public GameObject nextTetrominoLocation_2;
    public GameObject nextTetrominoLocation_3;
    public GameObject nextTetrominoLocation_4;
    public GameObject holdTetrominoLocation;

    #region Hold Variables
    private GameObject currentTetromino;
    private List<GameObject> nextTetrominoes = new List<GameObject>();
    private GameObject holdTetromino = null;
    private GameObject tempTetromino = null;
    #endregion

    int[] tetrominoesArray = { 0, 1, 2, 3, 4, 5, 6 };
    string[] tetrominoesNames = { "Player_1/Player1_I-Tetromino", "Player_1/Player1_J-Tetromino", "Player_1/Player1_L-Tetromino",
        "Player_1/Player1_O-Tetromino", "Player_1/Player1_S-Tetromino", "Player_1/Player1_T-Tetromino", "Player_1/Player1_Z-Tetromino" };
    private int currentIndex = 0;
    private int spawnCount;

    private bool firstBlock = true;
    [HideInInspector] public bool usedHold = false;

    private bool startGame = false;

    private void Start()
    {
        // Uncomment this if you want to start the game automatically
        // if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient) StartGame();
        // else if (!PhotonNetwork.IsConnected) StartGame();
    }

    public void StartGame()
    {
        startGame = true;
        // Initialize the four next tetrominos
        for (int i = 0; i < 4; i++)
        {
            InstantiateNextTetromino();
        }
        // Spawn the first active tetromino
        NewTetromino();
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient && startGame) HoldTetromino_Player1();
        else if (!PhotonNetwork.IsConnected && startGame) HoldTetromino_Player1();
    }

    public void NewTetromino()
    {
        if (nextTetrominoes.Count > 0)
        {
            // Get the next tetromino from the queue
            currentTetromino = nextTetrominoes[0];
            currentTetromino.transform.position = transform.position;
            currentTetromino.transform.localScale = new Vector3(1f, 1f, 1f);
            currentTetromino.GetComponent<Player1_TetrisBlock>().enabled = true;
            nextTetrominoes.RemoveAt(0);

            // Instantiate new tetromino to maintain 4 next tetrominos
            InstantiateNextTetromino();
        }
        else
        {
            Debug.LogError("No tetrominoes available to spawn!");
        }
    }

    public void InstantiateNextTetromino()
    {
        currentIndex = Random.Range(0, Tetrominoes.Length);
        while (CheckIfBlockHasAlreadySpawned(currentIndex)) currentIndex = Random.Range(0, Tetrominoes.Length);

        GameObject nextTetromino;
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            string tetrominoName = tetrominoesNames[currentIndex];
            nextTetromino = PhotonNetwork.Instantiate(tetrominoName, Vector3.zero, Quaternion.identity);
        }
        else
        {
            nextTetromino = Instantiate(Tetrominoes[currentIndex], Vector3.zero, Quaternion.identity);
        }

        nextTetromino.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        nextTetromino.GetComponent<Player1_TetrisBlock>().enabled = false;
        nextTetrominoes.Add(nextTetromino);
        spawnCount++;

        // Update positions of the next tetrominos
        UpdateNextTetrominoPositions();
    }

    private void UpdateNextTetrominoPositions()
    {
        Vector3[] positions = new Vector3[]
        {
            nextTetrominoLocation_1.transform.position,
            nextTetrominoLocation_2.transform.position,
            nextTetrominoLocation_3.transform.position,
            nextTetrominoLocation_4.transform.position
        };

        for (int i = 0; i < nextTetrominoes.Count && i < positions.Length; i++)
        {
            nextTetrominoes[i].transform.position = positions[i];
        }
    }

    private bool CheckIfBlockHasAlreadySpawned(int index)
    {
        if (spawnCount == 7)
        {
            for (int i = 0; i < 7; ++i)
                tetrominoesArray[i] = i;

            spawnCount = 0;
        }
        for (int i = 0; i < 7; ++i)
        {
            if (currentIndex == tetrominoesArray[i])
            {
                tetrominoesArray[i] = -1;
                return false;
            }
        }

        return true;
    }

    private void HoldTetromino_Player1()
    {
        if (Input.GetKeyDown(KeyCode.C) && usedHold == false)
        {
            if (holdTetromino == null)
            {
                currentTetromino.transform.rotation = Quaternion.identity;

                holdTetromino = currentTetromino;
                holdTetromino.GetComponent<Player1_TetrisBlock>().DestroyGhostPiece();
                holdTetromino.GetComponent<Player1_TetrisBlock>().enabled = false;
                holdTetromino.transform.position = holdTetrominoLocation.transform.position;
                holdTetromino.transform.rotation = Quaternion.identity;
                holdTetromino.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                NewTetromino();
            }
            else
            {
                currentTetromino.GetComponent<Player1_TetrisBlock>().DestroyGhostPiece();
                currentTetromino.GetComponent<Player1_TetrisBlock>().enabled = false;
                currentTetromino.transform.rotation = Quaternion.identity;
                currentTetromino.transform.position = holdTetrominoLocation.transform.position;
                currentTetromino.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                holdTetromino.transform.position = transform.position;
                holdTetromino.transform.rotation = Quaternion.identity;
                holdTetromino.transform.localScale = new Vector3(1f, 1f, 1f);

                tempTetromino = currentTetromino;
                currentTetromino = holdTetromino;
                holdTetromino = tempTetromino;

                currentTetromino.GetComponent<Player1_TetrisBlock>().enabled = true;
                holdTetromino.GetComponent<Player1_TetrisBlock>().enabled = false;
                currentTetromino.GetComponent<Player1_TetrisBlock>().InstantiateGhostPiece();
                currentTetromino.transform.position = transform.position;
            }
            usedHold = true;
        }
    }
}
