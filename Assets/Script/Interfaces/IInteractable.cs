using UnityEngine;

/// <summary>
/// 상호작용 가능한 모든 오브젝트가 구현해야 하는 인터페이스입니다.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 상호작용 UI에 표시될 텍스트를 반환합니다.
    /// </summary>
    /// <returns>예: "E키를 눌러 문 열기"</returns>
    string GetInteractText();

    /// <summary>
    /// 상호작용을 실행합니다.
    /// </summary>
    /// <param name="interactor">상호작용을 시도하는 게임 오브젝트 (예: 플레이어)</param>
    void Interact(GameObject interactor);
}
