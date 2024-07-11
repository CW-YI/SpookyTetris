using EasyTransition;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectCharacter : MonoBehaviourPunCallbacks
{
    #region before
    //private TextMeshPro Player1Character;
    //private TextMeshPro Player2Character;
    [SerializeField] private GameObject player1Ready;
    [SerializeField] private GameObject player2Ready;
    private bool player1_ready = false;
    private bool player2_ready = false;
    #endregion

    private bool isGameStartReady = false;
    private bool sceneMove = false;
    private Coroutine startCoroutine;

    [SerializeField] private TextMeshProUGUI TimerText;
    private float reaminTime = 60f;

    private string Player1Character = "Random";
    private string Player2Character = "Random";
    [SerializeField] private Button[] characterButtons;
    private int selectedP1 = -1;
    private int selectedP2 = -1;

    private string[] characterNames = { "Witch", "Puppeteer", "Mummy", "Ghost", "Goblin", "Dracula", "Zombie", "Random" };
    private int[] characterGauges = { 2, 6, 4, 6, 6, 8, 2, 0 };

    #region skillInfo
    private string[] characterActiveInfo = { "쓰레기 줄을 1줄 제거할 때마다 방어 게이지가 0.5 상승합니다. 방어 게이지는 최대 6까지 저장 가능합니다. 액티브 스킬 사용 시 방어 게이지를 모두 소모하여 방어 게이지 1당 피격 게이지를 1만큼 상쇄합니다.",
                                            "현재 선택된 블록을 분해하여 게임 판 가장 아래에서부터 빈 공간 4칸에 각각 채워 넣습니다.",
                                            "상대의 게임 판에 제거할 수 없는 방해 블록을 생성합니다. 방해 블록은 2초 후에 사라집니다.",
                                            "상대방이 마지막으로 한 번에 제거한 줄 수만큼 상대에게 쓰레기 줄을 보냅니다." , 
                                            "Comming Soon",
                                            "Comming Soon",
                                            "Comming Soon", 
                                            "랜덤으로 선택된 캐릭터의 액티브 스킬을 가집니다."
                                            };

    private string[] characterPassiveInfo = { "쓰레기 줄 10줄이 쌓인 상태로 게임을 시작합니다. 쓰레기 줄을 모두 제거하면 라운드에서 즉시 승리합니다.",
                                             "한 번에 모든 색 블록을 포함하여 줄을 제거할 경우 상대에게 보내는 쓰레기 줄을 1줄 추가합니다.",
                                             "게임 판의 크기가 가로 8줄 x 세로 16줄 크기로 작아집니다.",
                                             "상대방이 마지막으로 한 번에 제거한 줄 수와 같은 수의 줄을 제거할 경우 상대에게 보내는 쓰레기 줄을 1줄 추가합니다.",
                                             "Comming Soon",
                                             "Comming Soon",
                                             "Comming Soon",
                                             "랜덤으로 선택된 캐릭터의 패시브 스킬을 가집니다."
                                             };
    #endregion
    private bool OpenInfo = false;
    

    #region playerImages
    [SerializeField] private Sprite[] characterNameImages;
    [SerializeField] private Sprite[] characterImages;
    [SerializeField] private Image P1Image;
    [SerializeField] private Image P2Image;
    [SerializeField] private Image P1NameImage;
    [SerializeField] private Image P2NameImage;
    #endregion

    [SerializeField] private TextMeshProUGUI readyButtonText;

    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private GameObject CountDown;
    [SerializeField] private Image CountDownImage;
    private bool timerOn = false;
    [SerializeField] private Sprite[] CountDownImages;
    private bool hasSelectedChar = false;

    #region info UI
    [SerializeField] private GameObject P1_infoUI;
    [SerializeField] TextMeshProUGUI P1_info_char_name;
    [SerializeField] TextMeshProUGUI P1_info_gauge;
    [SerializeField] TextMeshProUGUI P1_info_active;
    [SerializeField] TextMeshProUGUI P1_info_passive;

    [SerializeField] private GameObject P2_infoUI;
    [SerializeField] TextMeshProUGUI P2_info_char_name;
    [SerializeField] TextMeshProUGUI P2_info_gauge;
    [SerializeField] TextMeshProUGUI P2_info_active;
    [SerializeField] TextMeshProUGUI P2_info_passive;
    #endregion info UI

    public TransitionSettings transition;
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient && !hasSelectedChar) photonView.RPC("StartCharacterSelect_RPC", RpcTarget.All);
    }

    [PunRPC]
    void StartCharacterSelect_RPC()
    {
        Invoke(nameof(StartCharacterSelect), 2f);
    }


    void StartCharacterSelect()
    {
        timerOn = true;
        LoadingScreen.SetActive(false);
        CharacterButtonArray();
    }
    private void CharacterButtonArray()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    void OnButtonClicked(int buttonIndex)
    {
        if (PhotonNetwork.IsMasterClient && !sceneMove)
        {
            photonView.RPC("Player1Selected", RpcTarget.All, buttonIndex);
            photonView.RPC("Player1NameChange", RpcTarget.All, buttonIndex);
            Player1Info(buttonIndex);
        }
        else if (!PhotonNetwork.IsMasterClient && !sceneMove)
        {
            photonView.RPC("Player2Selected", RpcTarget.All, buttonIndex);
            photonView.RPC("Player2NameChange", RpcTarget.All, buttonIndex);
            Player2Info(buttonIndex);
        }
    }

    void Player1Info(int buttonIndex)
    {
        P1_info_char_name.text = Player1Character;
        P1_info_gauge.text = characterGauges[buttonIndex].ToString();
        P1_info_active.text = characterActiveInfo[buttonIndex];
        P1_info_passive.text = characterPassiveInfo[buttonIndex];
    }
    void Player2Info(int buttonIndex)
    {
        P2_info_char_name.text = Player2Character;
        P2_info_gauge.text = characterGauges[buttonIndex].ToString();
        P2_info_active.text = characterActiveInfo[buttonIndex];
        P2_info_passive.text = characterPassiveInfo[buttonIndex];
    }

    [PunRPC]
    void Player1Selected(int buttonIndex)
    {
        if (selectedP1 != -1)
        {
            Outline[] previousOutlines = characterButtons[selectedP1].GetComponents<Outline>();
            previousOutlines[0].enabled = false;
        }

        Outline[] outlines = characterButtons[buttonIndex].GetComponents<Outline>();
        outlines[0].enabled = true;

        selectedP1 = buttonIndex;
    }

    [PunRPC]
    void Player2Selected(int buttonIndex)
    {
        if (selectedP2 != -1)
        {
            Outline[] previousOutlines = characterButtons[selectedP2].GetComponents<Outline>();
            previousOutlines[1].enabled = false;
        }

        Outline[] outlines = characterButtons[buttonIndex].GetComponents<Outline>();
        outlines[1].enabled = true;

        selectedP2 = buttonIndex;
    }

    [PunRPC]
    void Player1NameChange(int buttonIndex)
    {
        if (player1_ready)
        {
            player1Ready.SetActive(false);
            player1_ready = false;

            if (PhotonNetwork.IsMasterClient)
            {
                if (player1_ready) readyButtonText.text = "Unready";
                else readyButtonText.text = "Ready";
            }
        }

        Player1Character = characterNames[buttonIndex];
        P1Image.sprite = characterImages[buttonIndex];
        P1NameImage.sprite = characterNameImages[buttonIndex];
    }

    [PunRPC]
    void Player2NameChange(int buttonIndex)
    {
        if (player2_ready)
        {
            player2Ready.SetActive(false);
            player2_ready = false;

            if (!PhotonNetwork.IsMasterClient)
            {
                if (player1_ready) readyButtonText.text = "Unready";
                else readyButtonText.text = "Ready";
            }
        }

        Player2Character = characterNames[buttonIndex];
        P2Image.sprite = characterImages[buttonIndex];
        P2NameImage.sprite = characterNameImages[buttonIndex];
    }
    void Update()
    {
        if (player1_ready && player2_ready && !isGameStartReady && !hasSelectedChar)
        {
            startCoroutine = StartCoroutine(StartGameMatch());
        }
        else if ((!player1_ready || !player2_ready) && isGameStartReady)
        {
            Debug.Log("CancelReady");
            StopCoroutine(startCoroutine);
            isGameStartReady = false;
            CountDown.SetActive(false);
        }

        if (timerOn)
        {
            if (reaminTime > 0)
            {
                reaminTime -= Time.deltaTime;
                TimerText.text = Mathf.CeilToInt(reaminTime).ToString();
            }
            else
            {
                TimerText.text = "0";
                ChoiceRandomCharacter();
            }
        }

        if (PhotonNetwork.IsConnected)
        {
            if (Input.GetKey(KeyCode.Q))OpenInfo = true;   
            else OpenInfo = false;

            if (PhotonNetwork.IsMasterClient) P1_infoUI.SetActive(OpenInfo);
            if (!PhotonNetwork.IsMasterClient) P2_infoUI.SetActive(OpenInfo);
        }
    }

    public void ExitMatch()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // ��Ʈ��ũ�� ���� �� ȣ��Ǵ� �Լ�
        Debug.Log(cause);
    }


    public void readyButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!player1_ready) readyButtonText.text = "Unready";
            else readyButtonText.text = "Ready";
            photonView.RPC("Player1ToReady", RpcTarget.All);
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            if (!player2_ready) readyButtonText.text = "Unready";
            else readyButtonText.text = "Ready";
            photonView.RPC("Player2ToReady", RpcTarget.All);
        }
    }

    [PunRPC]
    void Player1ToReady()
    {
        player1_ready = !player1_ready;
        player1Ready.SetActive(player1_ready);
    }

    [PunRPC]
    void Player2ToReady()
    {
        player2_ready = !player2_ready;
        player2Ready.SetActive(player2_ready);
    }

    private IEnumerator StartGameMatch()
    {
        isGameStartReady = true;
        CountDown.SetActive(true);
        CountDownImage.sprite = CountDownImages[2];
        Debug.Log("3");
        yield return new WaitForSeconds(1f);
        CountDownImage.sprite = CountDownImages[1];
        Debug.Log("2");
        yield return new WaitForSeconds(1f);
        CountDownImage.sprite = CountDownImages[0];
        Debug.Log("1");
        yield return new WaitForSeconds(1f);
        Debug.Log("moveScene");
        CountDown.SetActive(false);
        if (player1_ready && player2_ready)
        {
            ChoiceRandomCharacter();
        }
        isGameStartReady = false;
    }

    
    private void MovetoNextScene()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        TransitionManager.Instance().Transition("OnlineTetris", transition, 0);
        //PhotonNetwork.LoadLevel("OnlineTetris");
        hasSelectedChar = true;
    }

    private void ChoiceRandomCharacter()
    {
        sceneMove = true;
        string[] characters = { "Ghost", "Mummy", "Puppeteer", "Witch" };

        if (Player1Character == "Random" || Player1Character == "Zombie" || Player1Character == "Goblin" || Player1Character == "Dracula")
        {
            int index = Random.Range(0, characters.Length);
            Player1Character = characters[index];
            Debug.Log(Player1Character);
        }
        if (Player2Character == "Random" || Player2Character == "Zombie" || Player2Character == "Goblin" || Player2Character == "Dracula")
        {
            int index = Random.Range(0, characters.Length);
            Player2Character = characters[index];
            Debug.Log(Player2Character);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LocalPlayer.NickName = Player1Character;
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LocalPlayer.NickName = Player2Character;
        }
        MovetoNextScene();
    }
}
