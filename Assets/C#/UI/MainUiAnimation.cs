using UnityEngine;
using UnityEngine.UI;

public class MainUiAnimation : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] frames;
    public float frameTime = 0.05f;

    int currentFrame = 0;
    float timer = 0f;

    void Update()
    {
        if (targetImage == null)
        {
            Debug.LogError("targetImage x");
            return;
        }
        if (frames == null || frames.Length == 0)
        {
            Debug.LogError("frames 배열이 비어있거나 연결 x");
            return;
        }

        timer += Time.deltaTime;
        if (timer >= frameTime)
        {
            timer -= frameTime;
            currentFrame = (currentFrame + 1) % frames.Length;
            targetImage.sprite = frames[currentFrame];
        }
    }
}
