using EasyTransition;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyGameMatch : MonoBehaviourPunCallbacks
{
    #region button
    [SerializeField] private GameObject PauseScreen;
    [SerializeField] private GameObject MatchUI;
    [SerializeField] private GameObject CreateUI;
    [SerializeField] private GameObject EnterUI;
    [SerializeField] private GameObject CreateUI_P2;
    [SerializeField] private GameObject OptionUI;

    [SerializeField] private TextMeshProUGUI Player1_P2_ready;
    [SerializeField] private TextMeshProUGUI Player2_P2_ready;
    [SerializeField] private TextMeshProUGUI ReadyButtonText;
    [SerializeField] private TextMeshProUGUI Player2RoomName;

    [SerializeField] private GameObject WaitingText;
    [SerializeField] private GameObject MatchText;
    [SerializeField] private GameObject WrongCodeText;
    [SerializeField] private GameObject Player2UIForP1;
    [SerializeField] private TextMeshProUGUI RoomCodeText;
    private string roomNumberCode;
    #endregion

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    [SerializeField] TMP_InputField roomNameInput;
    private string roomNameCreate = null;
    private bool isRoomCreating = false;
    private int choice = 0;

    private bool isP2Ready = false;

    public TransitionSettings transition;
    // Start is called before the first frame update
    void Start()
    {
        //roomNameCreate = roomNameInput.GetComponent<TMP_InputField>().text;
    }

    public void RandomMatchButton()
    {
        choice = 3;
        PauseScreen.SetActive(true);
        MatchUI.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoomButton()
    {
        PauseScreen.SetActive(true);
        CreateUI.SetActive(true);
        choice = 1;

        int roomNumber = Random.Range(1, 100000);
        roomNumberCode = roomNumber.ToString("D5");
        RoomCodeText.text = roomNumberCode;

        PhotonNetwork.ConnectUsingSettings();

    }

    public void EnterRoomButton()
    {
        PauseScreen.SetActive(true);
        EnterUI.SetActive(true);
        choice = 2;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CheckRoomCode()
    {
        roomNameCreate = roomNameInput.text;

        PhotonNetwork.JoinRoom(roomNameCreate);
        //if (CheckIfRoomExists(roomNameCreate))
        //{
        //    //Debug.Log("���� ������");
            
        //}
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
       if (choice == 2)
        {
            roomNameInput.text = "";
            WrongCodeText.SetActive(true);
            Invoke(nameof(OffWarningText), 1.5f);
        }
    }

    public void ReadyRoomGameP2()
    {
        photonView.RPC("SetToReady", RpcTarget.All);
    }

    [PunRPC]
    void SetToReady()
    {
        if (!isP2Ready)
        {
            if (PhotonNetwork.IsMasterClient) Player1_P2_ready.text = "Ready";
            else if (!PhotonNetwork.IsMasterClient) Player2_P2_ready.text = "Ready";
            isP2Ready = true;
            ReadyButtonText.text = "Unready";
        }
        else
        {
            if (PhotonNetwork.IsMasterClient) Player1_P2_ready.text = "Not Ready";
            else if (!PhotonNetwork.IsMasterClient) Player2_P2_ready.text = "Not Ready";
            isP2Ready = false;
            ReadyButtonText.text = "Ready";
        }
    }

    public void GameStartInRoom()
    {
        if (isP2Ready)
        {
            Invoke(nameof(MoveToScene_CharacterSelct), 1.5f);
        }
    }

    private void OffWarningText()
    {
        WrongCodeText.SetActive(false);
    }
    public void OptionButton()
    {
        PauseScreen.SetActive(true);
        OptionUI.SetActive(true);
    }
    public void QuitButton()
    {
        PhotonNetwork.Disconnect();
        Application.Quit();
    }
    public override void OnConnected()
    {
        //Debug.Log("���� ���� ����");
    }


    public void CancelButton()
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        choice = 0;
        PauseScreen.SetActive(false);
        MatchUI.SetActive(false);
        EnterUI.SetActive(false);
        CreateUI.SetActive(false);
        CreateUI_P2.SetActive(false);
        Player2UIForP1.SetActive(false);
        WaitingText.SetActive(true);
        MatchText.SetActive(false);
        OptionUI.SetActive(false);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(cause);

    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("room update " + roomList.Count);
        PrintRoom();
        cachedRoomList = roomList;
    }

    public bool CheckIfRoomExists(string roonID)
    {
        if (cachedRoomList == null) { return false; }
        foreach (var room in cachedRoomList)
        {
            if (room.Name == roonID) return true;
        }
        return false;
    }

    //string choiceRoomID()
    //{
    //    string roomID;
    //    do
    //    {
    //        roomID = System.Guid.NewGuid().ToString();
    //    } while (CheckIfRoomExists(roomID));

    //    return roomID;
    //}

    public string FindAvailableJoinRoom()
    {
        if (cachedRoomList == null) return null;
        foreach (var room in cachedRoomList)
        {
            if (room.PlayerCount < room.MaxPlayers && room.IsOpen && room.IsVisible)
            {
                return room.Name;
            }
        }
        return null;
    }

    void PrintRoom()
    {
        foreach (var room in cachedRoomList)
        {
            Debug.Log("roomName : " + room.Name);
        }
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        if (choice == 1)
        {
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2, IsVisible = false, IsOpen = true };
            PhotonNetwork.CreateRoom(roomNumberCode, roomOptions);
        }
        else if (choice == 2)
        {

        }
        else if (choice == 3)
        {
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2, IsVisible = true, IsOpen = true };
            PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: roomOptions);
        }
    }

    public override void OnJoinedRoom()
    {
        //Debug.Log("�濡 �����Ͽ����ϴ� : " + PhotonNetwork.CurrentRoom.Name);

        if (choice == 3 && !PhotonNetwork.IsMasterClient)
        {
            WaitingText.SetActive(false);
            MatchText.SetActive(true);
        }
        else if (choice == 2)
        {
            Player2RoomName.text = roomNameCreate;
            roomNameInput.text = "";
            EnterUI.SetActive(false);
            CreateUI_P2.SetActive(true);
        }
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && choice == 1)
        {
            Player2UIForP1.SetActive(true);
        }
        else if (PhotonNetwork.IsMasterClient && choice == 3)
        {
            WaitingText.SetActive(false);
            MatchText.SetActive(true);
            Invoke(nameof(MoveToScene_CharacterSelct), 1.5f);
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //base.OnPlayerLeftRoom(otherPlayer);
        if (PhotonNetwork.IsMasterClient && choice == 1)
        {
            Player2UIForP1.SetActive(false);
        }
    }
    private void MoveToScene_CharacterSelct()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        photonView.RPC("RPC_MoveScene", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_MoveScene()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            TransitionManager.Instance().Transition("CharacterSelect", transition, 0);
            //SceneManager.LoadScene("CharacterSelect");
        }
        else
        {
            Debug.Log("상대 플레이어가 방을 떠났습니다");
            CancelButton();
        }
    }
}
