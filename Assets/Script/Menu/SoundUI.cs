using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundUI : MonoBehaviour
{
    [Header("Volume Controls")]
    public Slider volumeSlider;
    public Button volumeBtn;
    public Button muteBtn;

    private bool isMuted = false;
    private float previousVolume = 20f;

    private void Start()
    {
        // 초기 설정
        if (volumeSlider != null)
        {
            volumeSlider.value = 20f;
        }

        // 버튼 리스너 추가
        if (volumeBtn != null)
        {
            volumeBtn.onClick.AddListener(OnVolumeBtnClicked);
        }

        if (muteBtn != null)
        {
            muteBtn.onClick.AddListener(OnMuteBtnClicked);
            muteBtn.gameObject.SetActive(false); // 처음에는 MuteBtn 비활성화
        }

        // Slider 리스너 추가
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    // VolumeBTN 클릭 시 호출
    private void OnVolumeBtnClicked()
    {
        isMuted = true;

        // 현재 볼륨 저장
        if (volumeSlider != null)
        {
            previousVolume = volumeSlider.value;
            volumeSlider.value = 0f;
        }

        // 버튼 전환
        if (volumeBtn != null)
        {
            volumeBtn.gameObject.SetActive(false);
        }

        if (muteBtn != null)
        {
            muteBtn.gameObject.SetActive(true);
        }
    }

    // MuteBTN 클릭 시 호출
    private void OnMuteBtnClicked()
    {
        isMuted = false;

        // 볼륨을 20으로 설정
        if (volumeSlider != null)
        {
            volumeSlider.value = 20f;
        }

        // 버튼 전환
        if (muteBtn != null)
        {
            muteBtn.gameObject.SetActive(false);
        }

        if (volumeBtn != null)
        {
            volumeBtn.gameObject.SetActive(true);
        }
    }

    // Slider 값 변경 시 호출
    private void OnSliderValueChanged(float value)
    {
        // Slider를 수동으로 0으로 설정하면 mute 상태로 전환
        if (value == 0 && !isMuted)
        {
            isMuted = true;

            if (volumeBtn != null)
            {
                volumeBtn.gameObject.SetActive(false);
            }

            if (muteBtn != null)
            {
                muteBtn.gameObject.SetActive(true);
            }
        }
        // Slider를 0보다 크게 설정하면 unmute 상태로 전환
        else if (value > 0 && isMuted)
        {
            isMuted = false;

            if (muteBtn != null)
            {
                muteBtn.gameObject.SetActive(false);
            }

            if (volumeBtn != null)
            {
                volumeBtn.gameObject.SetActive(true);
            }
        }

        // 여기에 실제 볼륨 적용 로직 추가 가능
        // 예: AudioListener.volume = value / 100f;
    }

    private void OnDestroy()
    {
        // 리스너 제거
        if (volumeBtn != null)
        {
            volumeBtn.onClick.RemoveListener(OnVolumeBtnClicked);
        }

        if (muteBtn != null)
        {
            muteBtn.onClick.RemoveListener(OnMuteBtnClicked);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}
