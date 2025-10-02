using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    [SerializeField]
    private List<Button> crewCountButtons;

    [SerializeField]
    private List<Button> maxPlayerCountButtons;
    private CreateGameRoomData roomData;

    public void UpdateMaxPlayerCount(int count)
    {
        roomData.MaxPlayerCount = count;
        for (int i = 0; i < maxPlayerCountButtons.Count; i++)
        {
            if (i == count - 4)
            {
                maxPlayerCountButtons[i].image.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                maxPlayerCountButtons[i].image.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }

    public void UpdateCrewCount(int count)
    {
        roomData.crewCount = count;
        for (int i = 0; i < crewCountButtons.Count; i++)
        {
            if (i == count - 1)
            {
                crewCountButtons[i].image.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                crewCountButtons[i].image.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        int limitMaxPlayer = count == 1 ? 4 : count == 2 ? 7 : 9;
        if (roomData.MaxPlayerCount < limitMaxPlayer)
        {
            UpdateMaxPlayerCount(limitMaxPlayer);
        }
        else
        {
            UpdateMaxPlayerCount(roomData.MaxPlayerCount);
        }

        for (int i = 0; i < maxPlayerCountButtons.Count; i++)
        {
            var text = maxPlayerCountButtons[i].GetComponentInChildren<Text>();
            if (i < limitMaxPlayer - 4)
            {
                maxPlayerCountButtons[i].interactable = false;
                text.color = Color.gray;
            }
            else
            {
                maxPlayerCountButtons[i].interactable = true;
                text.color = Color.white;
            }
        }
    }

    // Start는 클래스의 최상위에 위치해야 합니다.
    void Start()
    {
        roomData = new CreateGameRoomData() { crewCount = 1, MaxPlayerCount = 9 };
    }

    public int GetMaxPlayerCount()
    {
        return roomData.MaxPlayerCount;
    }

    void Update()
    {
        // 필요시 구현
    }
}

public class CreateGameRoomData
{
    public int crewCount;
    public int MaxPlayerCount;
}
