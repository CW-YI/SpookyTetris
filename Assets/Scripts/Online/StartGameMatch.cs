using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StartGameMatch : MonoBehaviourPunCallbacks
{
    private string[] characterNames = { "Witch", "Puppeteer", "Mummy", "Ghost", "Goblin", "Dracula", "Zombie", "Random" };
    private int[] characterGauges = { 2, 4, 4, 6, 6, 8, 2, 4 };

    #region ReadyForStart
    [SerializeField] private Image player1CharacterImage;
    [SerializeField] private Image player2CharacterImage;
    [SerializeField] private Image player1CharacterName;
    [SerializeField] private Image player2CharacterName;
    [SerializeField] private Sprite[] characterfaceImages;
    [SerializeField] private Sprite[] characterImages;
    [SerializeField] private Sprite[] characterTextImages;
    [SerializeField] private GameObject[] player1characterSD;
    [SerializeField] private GameObject[] player2characterSD;
    [SerializeField] private GameObject[] player1Gauge;
    [SerializeField] private GameObject[] player2Gauge;

    [SerializeField] private Player1_TetrominoSpawner player1startGame;
    [SerializeField] private Player2_TetrominoSpawner player2startGame;

    [SerializeField] private GameObject CountDown;
    [SerializeField] private Image CountDownImage;
    [SerializeField] private Sprite[] CountDownImages;
    #endregion

    [SerializeField] private Image GameOverPlayer1Name;
    [SerializeField] private Image GameOverPlayer2Name;
    [SerializeField] private Image GameOverPlayer1Image;
    [SerializeField] private Image GameOverPlayer2Image;
    int player1characterIndex = 0;
    int player2characterIndex = 0;
    string player1Name;
    string player2Name;

    [SerializeField] private GameObject LoadingScreen;
    private static List<int> scenenLoadedPlayer = new List<int>();
    #region 타 스크립트 참조
    GameCharacter gameCharacter;
    public PvPLineController lineController;
    #endregion

    public bool gameStarted = false;
    private float startTime;
    public static float timeElapsed = 0f;

    public Image VsPlayer1;
    public Image VsPlayer2;
    public GameObject VsPlayer1object;
    public GameObject VsPlayer2object;
    public GameObject Vs;
    public Animator Vsanimator;
    public GameObject GamePlayer1;
    public GameObject GamePlayer2;
    public GameObject VsStart;

    private void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            CheckStartGame();
        }
    }

    [PunRPC]
    void FindCharacterIndex_Player1(string player1NickName)
    {
        player1Name = player1NickName;
        player1characterIndex = Array.IndexOf(characterNames, player1NickName);
        player1CharacterImage.sprite = characterfaceImages[player1characterIndex];
        player1CharacterName.sprite = characterTextImages[player1characterIndex];
        VsPlayer1.sprite = characterImages[player1characterIndex];

        lineController.player1Index = player1characterIndex;

        if (player1characterSD[player1characterIndex] != null) player1characterSD[player1characterIndex].SetActive(true);
        if (player1Gauge[player1characterIndex] != null) player1Gauge[player1characterIndex].SetActive(true);

        GameOverPlayer1Name.sprite = characterTextImages[player1characterIndex];
        GameOverPlayer1Image.sprite = characterImages[player1characterIndex];

        //Debug.Log("^^^^^^^^^^^" + player1NickName + PhotonNetwork.IsMasterClient + player1characterIndex);
    }
    [PunRPC]
    void FindCharacterIndex_Player2(string player2NickName)
    {
        player2Name = player2NickName;
        player2characterIndex = Array.IndexOf(characterNames, player2NickName);
        player2CharacterImage.sprite = characterfaceImages[player2characterIndex];
        player2CharacterName.sprite = characterTextImages[player2characterIndex];
        VsPlayer2.sprite = characterImages[player2characterIndex];

        lineController.player2Index = player2characterIndex;

        if (player2characterSD[player2characterIndex] != null) player2characterSD[player2characterIndex].SetActive(true);
        if (player2Gauge[player2characterIndex] != null) player2Gauge[player2characterIndex].SetActive(true);

        GameOverPlayer2Name.sprite = characterTextImages[player2characterIndex];
        GameOverPlayer2Image.sprite = characterImages[player2characterIndex];
    }

    void CheckStartGame()
    {
        if (PhotonNetwork.IsMasterClient) photonView.RPC("FindCharacterIndex_Player1", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        if (!PhotonNetwork.IsMasterClient) photonView.RPC("FindCharacterIndex_Player2", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);

        //PhotonNetwork.Instantiate("PvpScriptHolder", new Vector3(0f, 0f, 0f), Quaternion.identity);
        VsStart.SetActive(true);
        Invoke(nameof(WaitForStart), 2f);
    }

    void WaitForStart()
    {
        if (!PhotonNetwork.IsMasterClient) photonView.RPC("CreateGrid", RpcTarget.All);
    }

    [PunRPC]
    void CreateGrid()
    {
        //LoadingScreen.SetActive(false);
        StartCoroutine(CountDownToStartGame());
    }

    private IEnumerator CountDownToStartGame()
    {
        yield return new WaitForSeconds(2f);
        GamePlayer1.SetActive(true); GamePlayer2.SetActive(true);
        yield return new WaitForSeconds(1f);
        Vsanimator.SetTrigger("end");

        yield return new WaitForSeconds(1.5f);
        Vs.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        CountDown.SetActive(true);

        CountDownImage.sprite = CountDownImages[2];
        yield return new WaitForSeconds(1f);
        CountDownImage.sprite = CountDownImages[1];
        yield return new WaitForSeconds(1f);
        CountDownImage.sprite = CountDownImages[0];
        yield return new WaitForSeconds(1f);
        CountDown.SetActive(false);
        gameStarted = true;

        VsPlayer1object.SetActive(false); VsPlayer2object.SetActive(false); 
        if (PhotonNetwork.IsMasterClient) player1startGame.StartGame();
        if (!PhotonNetwork.IsMasterClient) player2startGame.StartGame();
        
    }

    private void Update()
    {
        if (gameStarted)
        {
            // Calculate time elapsed since the start time
            timeElapsed = (float)(PhotonNetwork.Time - startTime);
            //Debug.Log(player1CharacterImage.sprite.name + characterTextImages[player1characterIndex].name);
            //player1CharacterImage.sprite = characterfaceImages[player1characterIndex];
            //player1CharacterName.sprite = characterTextImages[player1characterIndex];
            //GameOverPlayer1Name.sprite = characterTextImages[player1characterIndex];
            //GameOverPlayer1Image.sprite = characterImages[player1characterIndex];
        }

        //if (player1CharacterImage.sprite.name != characterfaceImages[player1characterIndex].name)
        //{
        //    Debug.Log("왜 다름? " + characterfaceImages[player1characterIndex].name);
        //    player1CharacterImage.sprite = characterfaceImages[player1characterIndex];
        //}
    }
}