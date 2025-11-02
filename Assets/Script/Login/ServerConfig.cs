using UnityEngine;

/// <summary>
/// 서버 관련 URL 및 설정을 중앙에서 관리하기 위한 ScriptableObject 입니다.
/// Assets/Create/Configuration/Server Configuration 메뉴를 통해 생성할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "ServerConfig", menuName = "Configuration/Server Configuration")]
public class ServerConfig : ScriptableObject
{
    [Header("API Endpoints")]
    [Tooltip("로그인 API 엔드포인트 URL")]
    public string loginUrl = "http://52.192.129.141:8080/api/v1/auth/sign-in";

    [Tooltip("사용자 정보 조회 API 엔드포인트 URL")]
    public string userInfoUrl = "http://52.192.129.141:8080/api/v1/user/info";

    // TODO: 추후 회원가입, 랭킹 등 다른 API가 추가되면 여기에 변수를 선언하여 관리할 수 있습니다.
}