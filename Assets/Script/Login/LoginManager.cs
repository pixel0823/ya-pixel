// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System;
// using UnityEngine.SeceneManagement;
// using System.Net;
// using System.Dynamic;

// public class LoginManager : MonoBehaviour
// {
//     // Start is called before the first frame update

//     [SerializeField]
//     public TMP_InputField userId;
//     [SerializeField]
//     public TMP_InputField password;
//     [SerializeField]
//     public Button loginButton;

//     [SerializeField]
//     private TMP_Text _loginText;


//     void Start()
//     {
//         _loginText.gameObject.SetActive(false);
//         loginButton.onClick.AddListener(Login);
//     }
//     /// <summary>
//     /// 로그인 시도 함수
//     /// </summary>
//     private void AttemptLogin()
//     {
//         string id = userId.text;
//         string pw = password.text;

//         if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
//         {
//             LoginText("아이디와 비밀번호를 입력해주세요.");
//             return;
//         }

//         // 로그인 버트 비활성화
//         loginButton.interactable = false;
//         LoginText("로그인 중...");

//         AuthenticationManager.Instance.Login(id, pw, HandleLoginResult);
//     }

//     /// <summary>
//     /// 로그인 요청 결과를 처리하는 콜백 함수
//     /// </summary>
//     /// <param name="isSuccess">로그인 성공 여부</param>
//     private void HandlerLoginResult(bool isSuccess)
//     {
//         loginButton.interactable = true;

//         if (isSuccess)
//         {
//             LoginText("로그인 성공! ");

//             InvokeBinder("LoadMainScesne", 1f);
//         }
//         else
//         {
//             LoginText("아이디 또는 비밀번호가 일치하지 않습니다."); 
//             loginButton.interactable = true;
//         }
//     }

 
//     private void LoginText(string message )
//     {
//         _loginText.text = message;
//         _loginText.gameObject.SetActive(true);
//     }
// }
