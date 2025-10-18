using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public GameObject MainMenuUI;
    public GameObject PasswordPanel; // 비밀번호 입력 패널
    public GameObject RoomListPanel; // 방 목록을 담고 있는 UI 패널
    public GameObject MultiUI;
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    public TMP_Text maxPlayerText;


    void Start()
    {
        // We will connect on button click, not on start
        // PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect()
    {
        if (MainMenuUI != null) MainMenuUI.SetActive(false);
        if (MultiUI != null) MultiUI.SetActive(true);

        PhotonNetwork.AutomaticallySyncScene = true;
        // TODO: 여기에 "서버에 접속 중..."과 같은 로딩 UI를 켜는 코드를 추가하면 좋습니다.
        // 예: if (ConnectingPanel != null) ConnectingPanel.SetActive(true);

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() // 포톤 서버 (마스터 서버)에 접속
    {
        base.OnConnectedToMaster();
        Debug.Log("<color=green>OnConnectedToMaster:</color> 포톤 마스터 서버에 접속했습니다.");

        // 로비 진입 요청
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby() // 로비 접속
    {
        base.OnJoinedLobby();
        Debug.Log("<color=green>OnJoinedLobby:</color> 로비에 접속했습니다. 이제 방 목록을 받을 수 있습니다.");

        // 로비에 성공적으로 접속했으므로, 메인 메뉴 UI를 끄고 멀티플레이 UI를 켭니다.
        if (MainMenuUI != null) MainMenuUI.SetActive(false);
        if (MultiUI != null) MultiUI.SetActive(true);
        // TODO: "서버에 접속 중..." UI가 있었다면 여기서 끕니다.
        // 예: if (ConnectingPanel != null) ConnectingPanel.SetActive(false);
        SetRoomListInteractable(true);
    }

    public override void OnLeftLobby() // 로비 퇴장
    {
        base.OnLeftLobby();
        print("Left Lobby");

        if (MainMenuUI != null) MainMenuUI.SetActive(true);
        if (MultiUI != null) MultiUI.SetActive(false);
    }

    public GameObject roomListItemPrefab;
    public Transform roomListContent;

    // RoomInfo 객체를 캐싱하여 방 목록을 관리합니다.
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    // 생성된 UI 아이템(프리팹)을 관리합니다.
    private Dictionary<string, GameObject> roomListEntries = new Dictionary<string, GameObject>();

    /// <summary>
    /// '방 목록 보기' 버튼에 연결할 함수입니다.
    /// </summary>
    public void OnRoomListButtonClicked()
    {
        Debug.Log("<color=yellow>UI Event:</color> '방 목록 보기' 버튼이 클릭되었습니다.");
        if (RoomListPanel != null) RoomListPanel.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"<color=cyan>OnRoomListUpdate:</color> 방 목록 업데이트 수신. {roomList.Count}개의 변경사항 감지.");
        if (roomListItemPrefab == null)
        {
            Debug.LogError("ConnectionManager: 'Room List Item Prefab'이(가) Inspector에 설정되지 않았습니다.");
            return;
        }
        if (roomListContent == null)
        {
            Debug.LogError("ConnectionManager: 'Room List Content'가 Inspector에 설정되지 않았습니다.");
            return;
        }

        // 변경된 방 목록 정보를 캐시에 업데이트합니다.
        UpdateCachedRoomList(roomList);
        // 캐시된 정보를 바탕으로 UI를 업데이트합니다.
        UpdateRoomListView();
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // 목록에서 제거되었거나, 닫혔거나, 보이지 않는 방은 캐시에서 삭제합니다.
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
                continue;
            }

            // 캐시된 방 정보를 갱신하거나 새로 추가합니다.
            cachedRoomList[info.Name] = info;
        }
    }

    private void UpdateRoomListView()
    {
        // 기존에 생성된 UI 리스트를 모두 삭제합니다.
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        roomListEntries.Clear();

        // 캐시된 방 목록을 기반으로 새로운 UI 리스트를 생성합니다.
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            Debug.Log($"<color=lightblue>UpdateRoomListView:</color> '{info.Name}' 방 UI 생성 중...");

            GameObject entry = Instantiate(roomListItemPrefab, roomListContent);
            entry.SetActive(true);

            RoomListItem listItem = entry.GetComponent<RoomListItem>();
            if (listItem != null)
            {
                listItem.Setup(info, this);
            }
            else
            {
                Debug.LogError("'RoomListItem' 프리팹의 최상단에 RoomListItem.cs 스크립트가 없습니다.");
            }

            roomListEntries.Add(info.Name, entry);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("<color=green>OnJoinedRoom:</color> 방에 입장했습니다. 방 목록 UI를 숨깁니다.");
        // 방에 입장하면 캐시와 UI 목록을 초기화합니다.
        cachedRoomList.Clear();
        // 방 목록 패널을 비활성화합니다.
        if (RoomListPanel != null) RoomListPanel.SetActive(false);

        // 마스터 클라이언트가 아닌 경우에만 씬을 로드합니다.
        // 마스터 클라이언트는 CreateRoom.cs에서 씬을 로드합니다.
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("클라이언트로서 'Map1' 씬을 로드합니다.");
            PhotonNetwork.LoadLevel("Map1");
        }
    }

    /// <summary>
    /// RoomListItem에서 호출하여 비밀번호 입력 패널을 활성화합니다.
    /// </summary>
    /// <param name="roomInfo">입장할 방의 정보</param>
    public void ShowPasswordPanel(RoomInfo roomInfo)
    {
        // ✅ 1. 비밀번호 패널이 비활성화 상태일 수 있으니 켭니다.
        if (PasswordPanel != null) PasswordPanel.SetActive(true);

        // ✅ 2. 비밀번호 패널에 CanvasGroup이 없다면 추가하고, 상호작용을 '반드시' 가능하도록 설정합니다.
        if (PasswordPanel != null)
        {
            CanvasGroup passwordCG = PasswordPanel.GetComponent<CanvasGroup>();
            if (passwordCG == null)
            {
                passwordCG = PasswordPanel.AddComponent<CanvasGroup>();
            }
            passwordCG.alpha = 1f;
            passwordCG.interactable = true;
            passwordCG.blocksRaycasts = true; // 클릭 이벤트가 통과하지 않도록 보장
        }

        // ✅ 3. 배경이 되는 방 목록 패널의 상호작용을 비활성화합니다. (기존 코드)
        if (RoomListPanel != null)
        {
            CanvasGroup roomListCG = RoomListPanel.GetComponent<CanvasGroup>();
            if (roomListCG != null)
            {
                roomListCG.alpha = 0.5f;
                roomListCG.interactable = false;
            }
        }

        // ✅ 4. 비밀번호 패널 컨트롤러에 roomInfo와 ConnectionManager 자신을 전달합니다.
        PasswordPanelController controller = PasswordPanel.GetComponent<PasswordPanelController>();
        if (controller != null)
        {
            controller.Setup(roomInfo, this);
        }
    }

    /// <summary>
    /// 비밀번호 입력 패널이 닫힐 때 호출할 함수입니다. (예: '취소' 버튼에 연결)
    /// </summary>
    public void HidePasswordPanel()
    {
        if (PasswordPanel != null) PasswordPanel.SetActive(false);

        CanvasGroup canvasGroup = RoomListPanel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f; // 원래 투명도로 복원
        canvasGroup.interactable = true; // 상호작용 활성화
    }

    public override void OnLeftRoom()
    {
        Debug.Log("<color=orange>OnLeftRoom:</color> 방에서 퇴장했습니다. 로비로 돌아갑니다.");
        SetRoomListInteractable(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogError($"<color=red>OnDisconnected:</color> 포톤 서버와의 연결이 끊어졌습니다. 원인: {cause}");

        // 연결이 끊겼으므로, 다시 메인 메뉴로 돌아갑니다.
        if (MainMenuUI != null) MainMenuUI.SetActive(true);
        if (MultiUI != null) MultiUI.SetActive(false);
        // TODO: "서버에 접속 중..." UI가 있었다면 여기서 끕니다.

        SetRoomListInteractable(false);
    }

    private void SetRoomListInteractable(bool interactable)
    {
        // ✅ 먼저 RoomListPanel이 비어있는지 확인합니다.
        if (RoomListPanel == null)
        {
            // 만약 비어있다면, 어떤 변수가 문제인지 정확히 알려주는 에러 메시지를 출력합니다.
            Debug.LogError("ConnectionManager: 'RoomListPanel' 변수가 Inspector에 할당되지 않았습니다! UI 상태를 변경할 수 없습니다.");
            return; // 함수를 즉시 종료하여 추가 오류를 방지합니다.
        }

        // 이 아래 코드는 기존과 동일합니다.
        CanvasGroup canvasGroup = RoomListPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = RoomListPanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.interactable = interactable;
    }
}
