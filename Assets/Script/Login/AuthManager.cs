// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections;
// using System.Text;
// using System.Text;
// using System.Diagnostics;

// public class AuthManager : MonoBehaviour
// {
//     public static AuthManager Instance;

//     private string accessToken;

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//             LoadTokens();
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     public void Login(string userId, string password, System.Action<bool> onLoginComplete)
//     {
//         StartCoroutine(RequestLogin(userId, password, onLoginComplete));
//     }

//     private IEnumerator RequestLogin(string userId, string password, System.Action<bool> onLoginComplete)
//     {
//         var lginData = new { userId = userId, password = password };
//         string jsonBody = JsonUtility.ToJson(loginData);
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

//         using (UnityWebRequest request = new UnityWebRequest(GameURL.Server.LOGIN, "POST"))
//         {
//             request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             request.downloadHandler = new DownloadHandlerBuffer();
//             request.SetRequestHeader("Content-Type", "application/json");

//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 var responseJson = request.downloadHandler.text;
//                 var responseData = JsonUtility.FromJson<LoginResponse>(responseJson);
//                 this.accessToken = responseData.accessToken;
//                 SaveTokens();
//                 onLoginComplete(true);
//             }
//             else
//             {
//                 Debug.LogError($"Login failed: {request.error}");
//                 onLoginComplete?.Invoke(false);
//             }
//         }
//     }

//     public void GetPlayerData(System.Action<string> onComplete)
//     {
//         StartCoroutine(ReqeustWithAuth(GameURL.Server.PLAYER_INFO, onComplete));
//     }

//     private IEnumerator RequestWithAuth(System.Action<string> onComplete)
//     {
//         if (string.IsNullOrEmpty(accessToken))
//         {
//             Debug.LogError("Access token is missing.");
//             onComplete?.Invoke(null);
//             yield break;
//         }

//         using (UnityWebRequest reqeust = UnityWebRequest.Get(GameURL.Server.PLAYER_INFO))
//         {
//             reqeust.SetRequestHeader("Authorization", $"Bearer {this.accessToken}");

//             yield return reqeust.SendWebRequest();

//             if (reqeust.result == UnityWebRequest.Result.Success)
//             {
//                 onComplete?.Invoke(reqeust.downloadHandler.text);
//             }
//             else if (reqeust.responseCode == 401)
//             {
//                 Debug.Log("Access Token 만료. 재발급을 시도합니다.");

//                 onComplete?.Invoke(null);
//             }
//             else
//             {
//                 Debug.LogError($"Request failed: {reqeust.error}");
//                 onComplete?.Invoke(null);
//             }
//         }


//     }
//     private void SaveTokens()
//     {
//         PlayerPrefs.SetString("AccessToken", this.accessToken);
//         PlayerPrefs.Save();
//     }

//     private void LoadTokens()
//     {
//         this.accessToken = PlayerPrefs.GetString("accessToken", null);
//     }
// }

// [System.Serializable]
// public class TokenResponse
// {
//     public string accessToken;
// }
