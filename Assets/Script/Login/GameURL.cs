using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 개임에서 사용하는 모든 URL을 중앙에서 관리하는 클래스
/// </summary>
public class GameURL
{
    /// <summary>
    /// 서버 관련 URL을 관리하는 클래스
    /// </summary>
    public static class Server
    {
#if UNITY_EDITOR
        // 유니티 에디터에서 실행할 때 사용할 개발용(로컬) 서버 주소
        private const string BaseURL = "http://localhost:8080";
#else
        // 실제 빌드된 게임에서 사용할 운영 서버 주소
        private const string BaseURL = "http://localhost:8080";
#endif
        // 외부에서 접근할 최종 URL 프롶퍼티
        public static string BASE_URL => BaseURL;

        // --- API 엔드포인트 ---
        public static string LOGIN => $"{BaseURL}/api/login";
        public static string REISSUE_TOKEN => $"{BaseURL}/api/token/reissue"; public static string PLAYER_INFO => $"{BaseURL}/api/player/info";
    }
   
}