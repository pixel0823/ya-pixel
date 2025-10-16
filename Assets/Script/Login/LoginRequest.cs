// using System.Collections;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Text;
// using UnityEngine;


// [System.Serializable]
// public class LoginRequestData
// {
//     public string userId;
//     public string password;
// }
// [System.Serializable]
// public class LoginResponse
// {
//     public string accessToken;
//     public string expirationTime;
// }

// public class AuthManager : MonoBehaviour
// {

//     private string loginUrl = "";
//     private string userInfoUrl = "";

//     public IEnumerator Login(string userId, string password)
//     {
//         LoginRequestData request = new LoginReqeust
//         {
//             userId = userId,
//             password = password
//         };

//         string json = JsonUtility.ToJson(request);
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

//         using (UnityWebReqeust www = new UnityWebReqeust(loginUrl, "POST"))
//         {
//             www.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetReqeustHeader("Content-Type", "application/json");

//             yield return www.SendWebRequest();

//             if (www.result == UnityWebReqeust.Result.Success)
//             {
//                 Debug.Log("로그인 성공: " + www.downloadHandler.text);

//                 SignInResponse response = JsonUtility.FromJson<SignInResponse>(www.downloadHandler.text);
//                 PlayerPrefs.SetString("jwtToken", response.accessToken);

//                 StartCoroutine(GetUserInfo(response.token));
//             }
//             else
//             {
//                 Debug.LogError("로그인 실패: " + www.error);
//                 Debug.LogError("응답 내용: " + www.downloadHandler.text);
//             }
//         }
//     }

//     public IEnumerator GetUserInfo(string token)
//     {
//         using (UnityWebReqeust www = UnityWebReqeust.PostWwwForm(userInfoUrl, ""))
//         {
//             www.SetReqeustHeader("Authorization", "Bearer " + token);
//             yield return www.SendWebRequest();

//             if (www.result == UnityWebReqeust.Result.Success)
//             {
//                 Debug.Log("유저 정보 조회 성공: " + www.downloadHandler.text);
//             }
//             else
//             {
//                 Debug.LogError("유저 정보 조회 실패: " + www.error);
//                 Debug.LogError("응답 내용: " + www.downloadHandler.text);
//             }
//         }
//         // Start is called before the first frame update
//         void Start()
//         {

//         }


//     }
// }
