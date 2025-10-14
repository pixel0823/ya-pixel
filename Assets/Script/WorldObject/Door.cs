using UnityEngine;

/// <summary>
/// 문과의 상호작용을 처리합니다. IInteractable을 구현합니다.
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
    [Tooltip("이 문을 통해 이동할 목적지 이름입니다.")]
    public string destination = "어딘가";

    public string GetInteractText()
    {
        return $"'F' 키를 눌러 {destination}(으)로 이동";
    }

    public void Interact(GameObject interactor)
    {
        // interactor는 상호작용한 플레이어입니다.
        // 지금은 디버그 메시지만 출력합니다.
        // 추후 여기에 SceneManager.LoadScene(\"씬이름\") 같은 씬 이동 로직을 구현할 수 있습니다.
        Debug.Log($"{interactor.name}이(가) 문을 사용했습니다. 목적지: {destination}");
    }
}
