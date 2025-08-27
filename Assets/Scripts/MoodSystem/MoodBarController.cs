
using Game.Scripts.Gameplay;
using System;
using UnityEngine;
using UnityEngine.UI;
public class MoodBarController : MonoBehaviour
{
    //[SerializeField] private MoodDisplay _display;
    [SerializeField] private SongWheelController songWheelController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Slider moodSlider;
    [SerializeField, Range(0f, 1f)] private float startValue = 0.5f;
    [SerializeField] private Sprite[] moodFillImage;
    [SerializeField] private Image ChangeMoodImage;

    // speed modifiers, v.v. có thể bind từ đây
    public float MovementSpeedModifier { get; private set; } = 1f;
    public float SongWheelTimeModifier { get; private set; } = 1f;

    private float _moodValue;

    private void Awake()
    {

        _moodValue = Mathf.Clamp01(startValue);
        UpdateUI();
    }
    #region MOOD_CONTROLER
    private void HandleFull()
    {
        ChangeMoodImage.sprite = moodFillImage[0]; // 100% mood
        playerController.movement.UnlockRun();
        OnMoodMax();
        ApplyEffects(_moodValue);
        ApplyEffectForTimeWin();
    }

    private void HandleCritical()
    {
        ChangeMoodImage.sprite = moodFillImage[0]; // 90% mood
        playerController.movement.UnlockRun();
        ApplyEffects(_moodValue);
        ApplyEffectForTimeWin();
    }

    private void HandleHigh()
    {
        ChangeMoodImage.sprite = moodFillImage[1]; // 75% mood
        playerController.movement.UnlockRun();
        ApplyEffects(_moodValue);
        ApplyEffectForTimeWin();
    }



    private void HandleMedium()
    {
        playerController.movement.LockRun();
        //ChangeMoodImage.sprite = moodFillImage[2]; // 50% mood
        ChangeMoodImage.sprite = moodFillImage[2]; // 50% mood
        ApplyEffects(_moodValue);
    }
    private void HandleLow()
    {
        playerController.movement.LockRun();
        ChangeMoodImage.sprite = moodFillImage[3]; // 30% mood
        ApplyEffects(_moodValue);
        ApplyEffectForTimeLose();
    }

    private void HandleEmpty()
    {
        playerController.movement.LockRun();
        ChangeMoodImage.sprite = moodFillImage[moodFillImage.Length - 1]; // 0% mood
        ApplyEffects(_moodValue);
        OnMoodMin();
        ApplyEffectForTimeLose();
    }
    #endregion
    /// <summary> Tăng mood thêm delta (0→1) </summary>
    public void IncreaseMood(float delta)
    {
        SetMood(_moodValue + delta);
    }

    /// <summary> Giảm mood đi delta (0→1) </summary>
    public void DecreaseMood(float delta)
    {
        SetMood(_moodValue - delta);
    }

    private void SetMood(float newValue)
    {
        _moodValue = Mathf.Clamp01(newValue);
        UpdateUI();
        //songWheelController.ModifierSongWheelTime(1 + _moodValue - 0.5f);

        // Check full / zero states
        switch (_moodValue)
        {
            case var _ when Mathf.Approximately(_moodValue, 0f):
                Debug.Log("Mood is empty");
                HandleEmpty();
                break;
            case <= 0.3f:
                HandleLow();
                break;
            case < 0.5f:
                ApplyEffectForTimeLose();
                break;
            case 0.5f:
                HandleMedium();
                break;
            case <= 0.75f:
                HandleHigh();
                break;
            case <= 0.9f:
                HandleCritical();
                break;
            case > 0.9f:
                HandleFull();
                break;
            default:
                break;
        }
    }


    private void UpdateUI()
    {
        if (moodSlider != null)
            moodSlider.value = _moodValue;
    }

    private void ApplyEffects(float amount)
    {
        // Ví dụ: khi mood cao: tăng speed, tăng thời gian chọn
        MovementSpeedModifier = 0.5f + amount;        // max +50% speed
        playerController.movement.SetModifierSpeed(MovementSpeedModifier);
        // Bạn có thể broadcast event hoặc gán trực tiếp cho player / UI
    }

    private void ApplyEffectForTimeWin()
    {
        playerController.detection.SetTimeOut(playerController.detection.GetTimeOut() + 2);
    }

    private void ApplyEffectForTimeLose()
    {
        playerController.detection.SetTimeOut(playerController.detection.GetTimeOut() - 2);
    }

    private void OnMoodMax()
    {
        //songWheelController.ModifierSongWheelTime(1 + _moodValue - 0.5f);
        GameManager.Instance.songWheelController.ActivateNewSongWheel();
        SongWheelManager.Instance.AddMoreRandomSongWheelNumbers();
        //playerController.UnlockRun();
    }

    private void OnMoodMin()
    {
        GameManager.Instance.GameLose();

    }
}
