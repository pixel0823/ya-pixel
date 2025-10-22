using UnityEngine;
using Photon.Pun;

/// <summary>
/// 게임 시작 전 전체적인 UI 흐름(패널 전환)을 관리합니다.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuUI;      // 싱글/멀티 선택 UI
    public GameObject lobbyChoiceUI;   // 방 만들기/목록 보기 선택 UI
    public GameObject connectingUI;    // "연결 중..." 표시 UI (선택 사항)
    public GameObject createRoomUI;    // 방 만들기 UI
    public GameObject roomListUI;      // 방 목록 UI

    [Header("Managers")]
    public ConnectionManager connectionManager;

    void Start()
    {
        // 시작 시 메인 메뉴만 활성화하고 나머지는 모두 비활성화
        if (mainMenuUI == null || lobbyChoiceUI == null || createRoomUI == null || roomListUI == null)
        {
            Debug.LogError("MainMenuManager에 하나 이상의 UI 패널이 할당되지 않았습니다! Inspector를 확인해주세요.");
            return;
        }

        mainMenuUI.SetActive(true);
        lobbyChoiceUI.SetActive(false);
        if (connectingUI != null) connectingUI.SetActive(false);
        createRoomUI.SetActive(false);
        roomListUI.SetActive(false);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    //--- MainMenuUI 버튼 핸들러 ---//
    public void OnClick_SinglePlayer()
    {
        Debug.Log("OnClick_SinglePlayer: 함수 호출됨.");
        if (mainMenuUI == null || lobbyChoiceUI == null)
        {
            Debug.LogError("mainMenuUI 또는 lobbyChoiceUI가 비어있습니다! Inspector를 확인해주세요.");
            return;
        }

        GameModeManager.Instance.CurrentMode = GameMode.Single;
        mainMenuUI.SetActive(false);
        lobbyChoiceUI.SetActive(true);
        Debug.Log("OnClick_SinglePlayer: mainMenuUI를 끄고 lobbyChoiceUI를 켰습니다.");
    }

    public void OnClick_MultiPlayer()
    {
        Debug.Log("OnClick_MultiPlayer: 함수 호출됨.");
        if (mainMenuUI == null)
        {
            Debug.LogError("mainMenuUI가 비어있습니다! Inspector를 확인해주세요.");
            return;
        }

        GameModeManager.Instance.CurrentMode = GameMode.Multi;
        mainMenuUI.SetActive(false);

        if (connectingUI != null)
        {
            Debug.Log("Connecting UI를 켭니다.");
            connectingUI.SetActive(true);
        }
        else
        {
            Debug.Log("Connecting UI가 없으므로, lobbyChoiceUI를 켭니다.");
            if (lobbyChoiceUI == null)
            {
                Debug.LogError("lobbyChoiceUI가 비어있습니다! Inspector를 확인해주세요.");
                return;
            }
            lobbyChoiceUI.SetActive(true);
        }

        if (connectionManager != null)
        {
            Debug.Log("ConnectionManager.Connect()를 호출합니다.");
            connectionManager.Connect();
        }
        else
        {
            Debug.LogWarning("ConnectionManager가 할당되지 않았습니다!");
        }
    }

    //--- LobbyChoiceUI 버튼 핸들러 ---//
    public void OnClick_CreateRoom()
    {
        lobbyChoiceUI.SetActive(false);
        createRoomUI.SetActive(true);
    }

    public void OnClick_RoomList()
    {
        lobbyChoiceUI.SetActive(false);
        roomListUI.SetActive(true);
        
        if (connectionManager != null)
        {
            connectionManager.RefreshRoomList();
        }
    }

    //--- 뒤로가기 버튼 핸들러 ---//
    public void OnClick_BackToMainMenu()
    {
        lobbyChoiceUI.SetActive(false);
        mainMenuUI.SetActive(true);
        
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void OnClick_BackToLobbyChoice()
    {
        createRoomUI.SetActive(false);
        roomListUI.SetActive(false);
        lobbyChoiceUI.SetActive(true);
    }

    //--- ConnectionManager로부터 호출될 함수들 ---//

    public void OnJoinedLobby()
    {
        if (connectingUI != null) connectingUI.SetActive(false);
        lobbyChoiceUI.SetActive(true);
    }

    public void OnDisconnected()
    {
        if (connectingUI != null) connectingUI.SetActive(false);
        lobbyChoiceUI.SetActive(false);
        createRoomUI.SetActive(false);
        roomListUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }
}
