using DG.Tweening;
using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SongWheelView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform _wheelRect;
    [SerializeField] private RectTransform _wheelRectForBirdEnemy;
    [SerializeField] private Image[] _songNotes;
    [SerializeField] private Sprite[] _songNoteSprites;
    [SerializeField] PlayerController _playerController;
    //[SerializeField] private float _offsetY = 60f;
    //private List<int> _selectSlices = new();
    public SliceView[] Slices;
    public bool WheelActive { get; private set; }
    public int CurrentSlice = -1;
    //private Vector2[] _sliceSize;
    //private int newSongWheelNumber;
    //private float _currentFillAmount = 1f;

    public IEnumerator ActivateWheel()
    {
        if (WheelActive) yield break;
        WheelActive = true;
        _wheelRect.gameObject.SetActive(true);
        _wheelRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        _wheelRect.localScale = Vector3.zero;
        yield return _wheelRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }
    public IEnumerator ReleaseWheel()
    {
        yield return _wheelRect.DOScale(0f, 0.15f).SetEase(Ease.InBack);
        _wheelRect.gameObject.SetActive(false);
        CurrentSlice = -1;
        WheelActive = false;
    }
    private void Update()
    {
        if (!WheelActive) return;
        UpdateSelection();
        //PlayerCountDownAnimation();
    }

    private void UpdateSelection()
    {
        Vector2 mousePos = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _wheelRect, mousePos, null, out Vector2 localPoint);

        float angle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int slice = 0;
        if (angle >= 20 && angle < 70) slice = 7;
        else if (angle >= 70 && angle < 110) slice = 0;
        else if (angle >= 110 && angle < 160) slice = 1;
        else if (angle >= 160 && angle < 200) slice = 2;
        else if (angle >= 200 && angle < 250) slice = 3;
        else if (angle >= 250 && angle < 290) slice = 4;
        else if (angle >= 290 && angle < 340) slice = 5;
        else if (angle >= 340 || angle < 20) slice = 6;

        if (slice != CurrentSlice)
        {
            HighlightSlice(slice);
            CurrentSlice = slice;
        }
    }
    private void ResetHighlight()
    {
        for (int i = 0; i < Slices.Length; i++)
        {
            var slice = Slices[i];
            slice.UnHighlight();
        }
    }
    private void HighlightSlice(int slice)
    {
        ResetHighlight();
        var slicedSlice = Slices[slice];
        slicedSlice.HighlightSlice();
    }

    //private void PlayerCountDownAnimation()
    //{
    //    if (_currentSlice != -1 && _slices[_currentSlice].gameObject.activeSelf)
    //    {
    //        Image timerImage = _slices[_currentSlice].transform.GetChild(1).gameObject.GetComponent<Image>();
    //        float targetFill = Mathf.Clamp01((float)_playerController.detection.GetTimeOutCountDown() / _playerController.detection.GetTimeOut());
    //        _currentFillAmount = Mathf.Lerp(_currentFillAmount, targetFill, Time.deltaTime * 8f); // 8f is smoothing speed
    //        timerImage.fillAmount = _currentFillAmount;
    //        if (!_playerController.detection.IsPlaying())
    //        {
    //            timerImage.fillAmount = 0;
    //        }
    //        else if (timerImage.fillAmount == 1)
    //            timerImage.fillAmount = 0;
    //    }
    //}

}
