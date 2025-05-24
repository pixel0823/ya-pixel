using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.EventSystems;

public class LoginUi : MonoBehaviour
{
    public TMP_InputField idInput;
    public TMP_InputField pwInput;
    public Button loginbtn;
    public Button joinbtn;
    public TMP_Text loginResultText;

    void Start()
    {
        if (idInput == null) Debug.LogError("idInput이 연결되지 않았습니다");
        if (pwInput == null) Debug.LogError("pwInput이 연결되지 않았습니다!");
        if (loginbtn == null) Debug.LogError("loginbtn이 연결되지 않았습니다!");
        if (joinbtn == null) Debug.LogError("joinbtn이 연결되지 않았습니다!");
        if (loginResultText == null) Debug.LogError("loginResultText가 연결되지 않았습니다!");

        loginResultText.gameObject.SetActive(false);
        loginbtn.onClick.AddListener(OnLogin);
        joinbtn.onClick.AddListener(OnJoin);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (idInput.isFocused)
            {
                pwInput.ActivateInputField();
            }
            else if (pwInput.isFocused)
            {
                EventSystem.current.SetSelectedGameObject(loginbtn.gameObject);
            }
            else if (EventSystem.current.currentSelectedGameObject == loginbtn.gameObject)
            {
                idInput.ActivateInputField();
            }
        }   
    }

    void OnLogin()
    {
        string id = idInput.text;
        
        string pw = pwInput.text;
        Debug.Log("id : " + id);
        Debug.Log("ps: " + pw);
        ServerConnector.Instance.Login(id, pw, OnLoginSuccess, OnLoginError);
    }

    void OnJoin()
    {

    }

    void OnLoginSuccess(string response)
    {
        LoginResponse loginRes = JsonUtility.FromJson<LoginResponse>(response);

        loginResultText.gameObject.SetActive(true);

        if (loginRes.result == "success")
        {
            PlayerPrefs.SetString("jwt_token", loginRes.token);
            PlayerPrefs.Save();
            loginResultText.text = "로그인 성공!";
            Debug.Log("저장된 토큰: " + loginRes.token);
        }

    }

    void OnLoginError(string error)
    {
        loginResultText.gameObject.SetActive(true);
        loginResultText.text = "로그인 실패: " + error;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string result;
        public string token;
    }



}
