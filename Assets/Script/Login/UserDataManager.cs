using UnityEngine;

/// <summary>
/// 로그인한 사용자의 정보를 저장하고, 게임 내 어디서든 접근할 수 있도록 하는 싱글톤 매니저입니다.
/// </summary>
public class UserDataManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UserDataManager Instance { get; private set; }

    // 저장할 사용자 정보
    public string Nickname { get; private set; }
    public string UserId { get; private set; }
    // TODO: 레벨, 재화 등 필요한 다른 정보들을 여기에 추가할 수 있습니다.

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 전환되어도 이 오브젝트를 파괴하지 않음
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있다면 중복 생성 방지를 위해 스스로를 파괴
        }
    }

    public void SetUserData(string userId, string nickname)
    {
        this.UserId = userId;
        this.Nickname = nickname;
        Debug.Log($"사용자 정보 설정 완료: UserId={this.UserId}, Nickname={this.Nickname}");
    }
}