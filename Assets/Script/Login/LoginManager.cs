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
/// 사용자 정보 요청에 사용될 데이터 구조체
/// </summary>
[System.Serializable]
public class UserInfoRequestData
{
    public string token;
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

                    // 유저 정보 요청 코루틴 시작 (필요 시)
                    StartCoroutine(GetUserInfoCoroutine(response.token));
                }
                else
                {
                    Debug.LogError("로그인은 성공했으나, 서버로부터 유효한 AccessToken을 받지 못했습니다.");
                    if (statusText != null) statusText.text = "토큰 수신 오류. 다시 시도해주세요.";
                }
                // 씬 전환
                SceneManager.LoadScene("Connection");
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
        // 1. POST 요청 본문에 담을 데이터 생성 (제거)
        // UserInfoRequestData requestData = new UserInfoRequestData ... (제거)
        // string json = JsonUtility.ToJson(requestData); (제거)
        // byte[] bodyRaw = Encoding.UTF8.GetBytes(json); (제거)

        // POST 방식으로 UnityWebRequest 생성
        using (UnityWebRequest www = new UnityWebRequest(serverConfig.userInfoUrl, "POST"))
        {
            // 2. UploadHandler 제거 (Body를 보내지 않음)
            // www.uploadHandler = new UploadHandlerRaw(bodyRaw); (제거)

            // 3. DownloadHandler는 응답을 받아야 하므로 유지
            www.downloadHandler = new DownloadHandlerBuffer();

            // 4. Content-Type 헤더 제거 (Body가 없으므로 불필요)
            // www.SetRequestHeader("Content-Type", "application/json"); (제거)

            // 5. Authorization 헤더 수정 ("Bearer"와 token 사이에 공백 추가)
            www.SetRequestHeader("Authorization", "Bearer " + token); // <-- 수정됨
            Debug.Log("Authorization 헤더 설정: Bearer " + token); // <-- 수정됨

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("유저 정보 조회 성공: " + www.downloadHandler.text);
                // TODO: 여기서 받은 유저 정보를 파싱하여 게임 내에 저장하거나 활용할 수 있습니다.
            }
            else
            {
                Debug.LogError("유저 정보 조회 실패: " + www.error);
                Debug.LogError("조회 실패 응답 코드: " + www.responseCode);
                Debug.LogError("조회 실패 내용: " + www.downloadHandler.text);
            }
        }
    }
}
