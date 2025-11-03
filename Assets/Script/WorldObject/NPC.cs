using UnityEngine;

/// <summary>
/// NPC의 상호작용을 처리합니다. IInteractable을 구현합니다.
/// </summary>
public class NPC : MonoBehaviour, IInteractable
{
    [Tooltip("NPC의 이름입니다.")]
    public string npcName = "주민";

    [Tooltip("F키를 누르면 활성화될 ItemCombPanel UI")]
    public GameObject itemCombPanel;

    private bool isPanelOpen = false;

    public string GetInteractText()
    {
        return $"'F' 키를 눌러 {npcName}에게 말 걸기";
    }

    public void Interact(GameObject interactor)
    {
        // interactor는 상호작용한 플레이어입니다.
        Debug.Log($"{interactor.name}이(가) {npcName}에게 말을 걸었습니다.");

        // ItemCombPanel 활성화
        if (itemCombPanel != null)
        {
            itemCombPanel.SetActive(true);
            isPanelOpen = !isPanelOpen;
            itemCombPanel.SetActive(isPanelOpen);
        }
        else
        {
            Debug.LogWarning("ItemCombPanel이 할당되지 않았습니다!");
        }

        // 예시: 플레이어를 바라보게 하기 (스프라이트가 x축으로만 뒤집히는 경우)
        // float directionToPlayer = interactor.transform.position.x - transform.position.x;
        // transform.localScale = new Vector3(Mathf.Sign(directionToPlayer), 1, 1);
    }

    void Update() {
        //ESC 키로 패널 닫기
        if (isPanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            isPanelOpen = false;
            itemCombPanel.SetActive(false);
        }
    }
}
