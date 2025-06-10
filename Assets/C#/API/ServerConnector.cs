using System.Collections;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;


public class ServerConnector : MonoBehaviour
{
    public static ServerConnector Instance { get; private set; }

    private string serverUrl = "http://15.164.123.214:8080";

    private void Awake()
    {
        // Singleton 패턴 
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void Login(string userId, string password, System.Action<string> onSuccess, System.Action<string> onFail)
    {
        StartCoroutine(LoginCoroutine(userId, password, onSuccess, onFail));
    }

    private IEnumerator LoginCoroutine(string userId, string password, System.Action<string> onSuccess, System.Action<string> onFail)
    {
        string url = $"{serverUrl}/api/user/login";

        LoginRequest req = new LoginRequest(userId, password);
        string json = JsonUtility.ToJson(req);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onFail?.Invoke(request.downloadHandler.text);
        }

    }


    [System.Serializable]
    public class LoginRequest
    {
        public string userId;
        public string password;
        public LoginRequest(string userId, string password)
        {
            this.userId = userId;
            this.password = password;
        }
    }

    
    
}
