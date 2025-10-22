using UnityEngine;
using Photon.Pun;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuUI;
    public GameObject lobbyChoiceUI; // "방 만들기", "방 참가하기" 버튼이 있는 공용 UI
    public GameObject connectingUI; // "연결 중..."을 표시할 UI

    [Header("Managers")]
    public ConnectionManager connectionManager;

    void Start()
    {
        // 시작 시 메인 메뉴만 활성화
        mainMenuUI.SetActive(true);
        if (lobbyChoiceUI != null) lobbyChoiceUI.SetActive(false);
        if (connectingUI != null) connectingUI.SetActive(false);

        // 다른 씬에서 메인 메뉴로 돌아왔을 경우를 대비해, 연결이 되어있다면 끊어줍니다.
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void OnSinglePlayer()
    {
        GameModeManager.Instance.CurrentMode = GameMode.Single;
        mainMenuUI.SetActive(false);
        if (lobbyChoiceUI != null) lobbyChoiceUI.SetActive(true);
    }

    public void OnMultiPlayer()
    {
        GameModeManager.Instance.CurrentMode = GameMode.Multi;
        mainMenuUI.SetActive(false);
        if (connectingUI != null) connectingUI.SetActive(true);

        if (connectionManager != null)
        {
            connectionManager.Connect();
        }
    }
}
