using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class PasswordPanelController : MonoBehaviour
{
    public TMP_InputField passwordInput;
    public Button confirmButton;
    public Button cancelButton;
    public TMP_Text passwordText;

    private RoomInfo targetRoom;
    private ConnectionManager connectionManager;

    public void Setup(RoomInfo roomInfo, ConnectionManager manager)
    {
        this.targetRoom = roomInfo;
        this.connectionManager = manager;
        passwordInput.text = "";
    }
    void Start()
    {
        confirmButton.onClick.AddListener(OnClick_Confirm);
        cancelButton.onClick.AddListener(OnClick_Cancel);
    }

    private void OnClick_Confirm()
    {
        string enteredPassword = passwordInput.text;
        string roomPassword = (string)targetRoom.CustomProperties["password"];

        if (enteredPassword == roomPassword)
        {
            Debug.Log("비밀번호 일치! ");
            PhotonNetwork.JoinRoom(targetRoom.Name);

        }
        else
        {
            Debug.Log("비밀번호 불일치! ");
            passwordInput.text = "";
            StopCoroutine(ShowWarningAndClear()); // 이미 경고가 떠있다면 중지
            StartCoroutine(ShowWarningAndClear()); // 경고 메시지를 보여주는 코루틴 시작
        }
    }

    private void OnClick_Cancel()
    {
        if (connectionManager != null)
        {
            connectionManager.HidePasswordPanel();
        }
    }

    /// <summary>
    /// 사용자에게 비밀번호가 틀렸다는 경고를 잠시 보여주고 지웁니다.
    /// </summary>
    private IEnumerator ShowWarningAndClear()
    {
        passwordText.text = "비밀번호가 틀렸습니다.";
        passwordText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); // 2초 동안 메시지 보여주기
        passwordText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
