using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text;

/// <summary>
/// 로그인 요청에 사용될 데이터 구조체
/// </summary>
[System.Serializable]
public class LoginRequestData
{
    public string userId;
    public string password;
}

/// <summary>
/// 로그인 응답으로 받을 데이터 구조체 (JWT 토큰 포함)
/// </summary>
[System.Serializable]
public class LoginResponse
{
    public string token;
    public string expirationTime;
}

/// <summary>
/// 사용자 정보 응답으로 받을 데이터 구조체
/// </summary>
[System.Serializable]
public class UserInfoResponse
{
    public string userId;
    public string nickname;
}
/// <summary>
/// 로그인, 사용자 정보 조회 등 인증 관련 로직을 관리합니다.
/// UI 요소와 연동하여 사용자 입력을 받고 결과를 피드백합니다.
/// </summary>
public class LoginManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("서버 URL 설정 파일 (ScriptableObject)")]
    [SerializeField] private ServerConfig serverConfig;
    
    [Header("UI Elements")]
    [Tooltip("사용자 ID를 입력받는 InputField")]
    [SerializeField] private TMP_InputField userIdInput;
    [Tooltip("비밀번호를 입력받는 InputField")]
    [SerializeField] private TMP_InputField passwordInput; // Inspector에서 Input Type을 'Password'로 설정하는 것을 권장합니다.
    [Tooltip("로그인 실행 버튼")]
    [SerializeField] private Button loginButton;
    [Tooltip("로그인 시도 결과(성공, 실패, 오류)를 표시할 텍스트")]
    [SerializeField] private TMP_Text statusText; 
    
    private void Start()
    {
        // serverConfig가 할당되었는지 확인합니다.
        if (serverConfig == null)
        {
            Debug.LogError("LoginManager: 'Server Config'가 Inspector에 할당되지 않았습니다!");
            return;
        }

        // 로그인 버튼에 리스너를 동적으로 추가합니다.
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }
    }
    
    /// <summary>
    /// 로그인 버튼 클릭 시 호출될 함수입니다.
    /// </summary>
    public void OnLoginButtonClicked()
    {
        string userId = userIdInput.text;
        string password = passwordInput.text;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
        {
            if (statusText != null) statusText.text = "아이디와 비밀번호를 모두 입력해주세요.";
            Debug.LogWarning("아이디 또는 비밀번호가 입력되지 않았습니다.");
            return;
        }
        
        // 기존에 실행 중인 코루틴이 있다면 중지하고 새로 시작
        StopAllCoroutines();
        StartCoroutine(LoginCoroutine(userId, password));
    }
    
    /// <summary>
    /// 서버에 로그인 요청을 보내는 코루틴입니다.
    /// </summary>
    private IEnumerator LoginCoroutine(string userId, string password)
    {
        if (statusText != null) statusText.text = "로그인 중...";
        
        LoginRequestData requestData = new LoginRequestData
        {
            userId = userId,
            password = password
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(serverConfig.loginUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("로그인 성공: " + www.downloadHandler.text);
                if (statusText != null) statusText.text = "로그인 성공!";

                LoginResponse response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);

                // 받은 토큰이 유효한지 확인합니다.
                if (response != null && !string.IsNullOrEmpty(response.token))
                {
                    // JWT 토큰 저장 (예: PlayerPrefs 사용)
                    PlayerPrefs.SetString("AccessToken", response.token);
                    PlayerPrefs.Save();

                    // 유저 정보 요청 코루틴이 끝날 때까지 기다립니다.
                    yield return StartCoroutine(GetUserInfoCoroutine(response.token));
                    SceneManager.LoadScene("Connection");
                }
                else
                {
                    Debug.LogError("로그인은 성공했으나, 서버로부터 유효한 AccessToken을 받지 못했습니다.");
                    if (statusText != null) statusText.text = "토큰 수신 오류. 다시 시도해주세요.";
                }
            }
            else
            {
                Debug.LogError("로그인 실패: " + www.error);
                Debug.LogError("응답 내용: " + www.downloadHandler.text);
                
                string errorMessage = "로그인 실패: 아이디 또는 비밀번호를 확인해주세요.";
                // 서버에서 구체적인 에러 메시지를 보내주는 경우, 이를 파싱하여 활용할 수 있습니다.
                // 예시: {"code": "NP", "message": "No Permission."}
                if (!string.IsNullOrEmpty(www.downloadHandler.text))
                {
                    // 간단하게 서버 응답을 그대로 보여주거나, JSON을 파싱하여 message만 추출할 수 있습니다.
                    errorMessage += $"\n({www.error})";
                }

                if (statusText != null) statusText.text = errorMessage;
            }
        }
    }

    /// <summary>
    /// 토큰을 이용해 서버에서 사용자 정보를 가져오는 코루틴입니다.
    /// </summary>
    private IEnumerator GetUserInfoCoroutine(string token)
    {
        // 코루틴 시작 시점에 serverConfig 또는 URL이 유효한지 다시 한번 확인합니다.
        if (serverConfig == null || string.IsNullOrEmpty(serverConfig.userInfoUrl))
        {
            Debug.LogError("GetUserInfoCoroutine: ServerConfig 또는 userInfoUrl이 유효하지 않습니다. Inspector 설정을 확인해주세요.");
            // 코루틴을 즉시 종료합니다.
            yield break;
        }

        // POST 요청 본문에 담을 JSON 데이터 생성
        // 서버에서 토큰을 어떤 Key로 받을지 모르므로, 가장 일반적인 "token"을 사용합니다.
        // 만약 서버가 다른 Key(예: "accessToken")를 사용한다면 이 부분을 수정해야 합니다.
        string jsonBody = $"{{\"token\":\"{token}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        // UnityWebRequest.Post 헬퍼 메소드를 사용하여 POST 요청 생성
        using (UnityWebRequest www = UnityWebRequest.Post(serverConfig.userInfoUrl, jsonBody, "application/json"))
        {
            // Authorization 헤더는 별도로 설정해줍니다.
            // Content-Type은 Post 메소드에서 자동으로 설정됩니다.
            www.SetRequestHeader("Authorization", "Bearer " + token);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("유저 정보 조회 성공: " + www.downloadHandler.text);
                UserInfoResponse userInfo = JsonUtility.FromJson<UserInfoResponse>(www.downloadHandler.text);

                // UserDataManager에 사용자 정보 저장
                if (userInfo != null)
                {
                    UserDataManager.Instance.SetUserData(userInfo.userId, userInfo.nickname);
                    // UserDataManager에 데이터가 잘 들어갔는지 확인하기 위한 Debug.Log 출력
                    Debug.Log($"[LoginManager] UserDataManager에 저장된 닉네임: {UserDataManager.Instance.Nickname}");
                }
            }
            else
            {
                Debug.LogError("유저 정보 조회 실패: " + www.error);
                Debug.LogError("응답 내용: " + www.downloadHandler.text);
            }
        }
    }
}
