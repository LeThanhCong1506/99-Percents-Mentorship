using DG.Tweening;
using Game.Scripts.Gameplay;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SongWheelController : MonoBehaviour
{
    private static readonly int IsSing = Animator.StringToHash("isSing");

    [Header("UI")]
    [SerializeField] private Animator _animator;
    [SerializeField] private RectTransform _wheelRect;
    [SerializeField] private RectTransform _wheelRectForBirdEnemy;
    [SerializeField] private Image[] _slices;
    [SerializeField] private Image[] _songNotes;
    [SerializeField] private Sprite[] _songNoteSprites;
    [SerializeField] PlayerController _playerController;
    [SerializeField] private float _offsetY = 60f; // Khoảng cách dọc từ vị trí của người chơi đến bánh xe

    //private int _songWheelTime = 3000;
    //public int _songWheelTimeForDisplay = 3000;
    private List<int> _selectSlices = new();
    //private int[] _lastSelectedSlices;
    private bool _wheelActive;
    private int _currentSlice = -1;
    private Vector2[] _sliceSize;
    private int newSongWheelNumber;
    private float _currentFillAmount = 1f; // Add this field to your class

    private void Start()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
        newSongWheelNumber = 6;
        _sliceSize = new Vector2[_slices.Length];
        for (int i = 0; i < _slices.Length; i++)
            _sliceSize[i] = _slices[i].GetComponent<RectTransform>().sizeDelta;
    }
    private void Synchron()
    {
        var positionOnScreen = Camera.main.WorldToScreenPoint(_playerController.transform.position);
        _wheelRect.position = positionOnScreen + Vector3.up * _offsetY;
        //_playerController.transform.position
    }

    private void FixedUpdate()
    {
        Synchron();
        EnemyBirdSynchron();
    }

    private void EnemyBirdSynchron()
    {
        if(_playerController.detection.GetCurrentEnemy() == null) return;
        var positionOnScreen = Camera.main.WorldToScreenPoint(_playerController.detection.GetCurrentEnemy().transform.position);
        _wheelRectForBirdEnemy.position = positionOnScreen;
    }

    private void Update()
    {
      
        if (Input.GetMouseButtonDown(1))
        {
            InputManager.Instance?.UnlockCursor();
            OpenSongWheel();
        }

        //if (_playerController.detection.IsPlaying() && _playerController.detection.TimeOut != 10)
        //{
        //    if (_songWheelTimeForDisplay <= 0)
        //        _songWheelTimeForDisplay = 4950;
        //    _songWheelTimeForDisplay -= (int)(Time.deltaTime * 1000 * 0.5f); // Giảm thời gian mỗi giây
        //}

        if (_wheelActive)
        {
            UpdateAnimation();
        }

        if (Input.GetMouseButtonDown(0))
        {
            SelectSongWheel();
        }

        if (Input.GetMouseButtonUp(1))
        {
            CloseSongWheel();
        }

    }

    private void UpdateAnimation()
    {
        UpdateSelection();
        PlayerCountDownAnimation();

    }

    private void SelectSongWheel()
    {
        if (!_wheelActive) return;
        if (!_playerController.detection.IsPlaying()) return;

        if (_slices[_currentSlice].gameObject.activeSelf)
        {
            _selectSlices.Add(_currentSlice);
            AppearSongNotes();
        }
        if (_selectSlices.Count == 2)
        {
            Debug.Log("send data to player");
            _playerController.detection.OnSelectSongWheel(_selectSlices.ToArray());
        }
    }
    private void OpenSongWheel()
    {
        if (_wheelActive) return;
        _animator.SetBool(IsSing, true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ActivateWheel();
    }
    private void CloseSongWheel()
    {
        _animator.SetBool(IsSing, false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ReleaseWheel();
    }
    private void PlayerCountDownAnimation()
    {
        if (_currentSlice != -1 && _slices[_currentSlice].gameObject.activeSelf)
        {
            Image timerImage = _slices[_currentSlice].transform.GetChild(1).gameObject.GetComponent<Image>();
            float targetFill = Mathf.Clamp01((float)_playerController.detection.GetTimeOutCountDown() / _playerController.detection.GetTimeOut());
            _currentFillAmount = Mathf.Lerp(_currentFillAmount, targetFill, Time.deltaTime * 8f); // 8f is smoothing speed
            timerImage.fillAmount = _currentFillAmount;
            if (!_playerController.detection.IsPlaying())
            {
                timerImage.fillAmount = 0;
            }
            else if (timerImage.fillAmount == 1)
                timerImage.fillAmount = 0;
            //Debug.Log($"Timer for fillAmount: {_songWheelTimeForDisplay} ms");
        }
    }


    public void ActivateNewSongWheel()
    {
        if (newSongWheelNumber < 1)
            return;
        _slices[newSongWheelNumber].gameObject.SetActive(true);
        newSongWheelNumber--;
    }
    public void ActivateWheel()
    {
        //_animator.SetBool(IsSing, true);
        _wheelRect.gameObject.SetActive(true);
        _wheelRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        _wheelRect.localScale = Vector3.zero;

        // Animation mở bánh xe
        _wheelRect.DOScale(1f, 0.2f)
            .SetEase(Ease.OutBack).OnComplete(() => { _wheelActive = true; });
    }
  
    public void ReleaseWheel()
    {
        _wheelRect.DOScale(0f, 0.15f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                _wheelRect.gameObject.SetActive(false);
                _wheelActive = false;
            });


        ResetHighlight();
        _currentSlice = -1;
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

        if (slice != _currentSlice)
        {
            HighlightSlice(slice);
            _currentSlice = slice;
        }
    }

    private void AppearSongNotes()
    {
        for (int i = 0; i < _songNotes.Length; i++)
        {
            if (_songNotes[i].gameObject.activeSelf)
            {
                _songNotes[i].sprite = _songNoteSprites[Random.Range(0, _songNoteSprites.Length)];
            }
        }
        _wheelRectForBirdEnemy.gameObject.SetActive(true);
        _wheelRectForBirdEnemy.localScale = Vector3.zero;

        // Animation mở bánh xe
        _wheelRectForBirdEnemy.DOScale(1f, 0.2f)
            .SetEase(Ease.OutBack).OnComplete(() => { _wheelActive = true; });
    }

    private IEnumerator HighLightSongNote()
    {
        Debug.Log("HighLightSongNote started");

        int[] _lastSelectedSlices = null;
        if (_selectSlices.Count == 2)
        {
            _lastSelectedSlices = _selectSlices.ToArray();

        }
        if (_lastSelectedSlices == null || _lastSelectedSlices.Length == 0)
        {
            Debug.LogWarning("HighLightSongNote: _lastSelectedSlices is null or empty");
            yield break;
        }

        for (int i = 0; i < _lastSelectedSlices.Length; i++)
        {
            int sliceIndex = _lastSelectedSlices[i];

            Color highlightColor = GetColorByDirection(sliceIndex);
            if (_songNotes[i].gameObject.activeSelf)
            {
                _songNotes[i].color = highlightColor;
                Debug.Log($"HighLightSongNote: Set color {highlightColor} for songNote {sliceIndex}");
            }
            else
            {
                Debug.LogWarning($"HighLightSongNote: songNote {sliceIndex} is not active");
            }
        }


        yield return new WaitForSeconds(1);

        Debug.Log("HighLightSongNote: Resetting song notes color and hiding wheelRectForBirdEnemy");
        _wheelRectForBirdEnemy.gameObject.SetActive(false);
        for (int i = 0; i < _songNotes.Length; i++)
        {
            if (_songNotes[i].gameObject.activeSelf)
            {
                _songNotes[i].color = Color.white;
                Debug.Log($"HighLightSongNote: Reset color for songNote {i}");
            }
        }
        Debug.Log("HighLightSongNote finished");
        _selectSlices.Clear(); // Xóa danh sách đã chọn sau khi hoàn thành
    }

    private Color GetColorByDirection(int dir)
    {
        string hex;
        switch (dir)
        {
            case 0: hex = "#138a94"; break;      // Xanh
            case 7: hex = "#6f4cdc"; break;      // Xanh ngọc
            case 6: hex = "#d44aca"; break;      // Xanh dương
            case 5: hex = "#704edd"; break;      // Tím
            case 4: hex = "#09afde"; break;      // Đỏ
            case 3: hex = "#60d4a0"; break;      // Vàng
            case 2: hex = "#85ce41"; break;      // Trắng xám
            case 1: hex = "#c8c011"; break;      // Xám
            default: hex = "#FFFFFF"; break;      // Trắng
        }
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
            return color;
        return Color.white;
    }

    private void TurnOfSongNotesWhenLost()
    {
        //Debug.Log("HighLightSongNote: Resetting song notes color and hiding wheelRectForBirdEnemy");
        _wheelRectForBirdEnemy.gameObject.SetActive(false);
        for (int i = 0; i < _songNotes.Length; i++)
        {
            if (_songNotes[i].gameObject.activeSelf)
            {
                _songNotes[i].color = Color.white;
                //Debug.Log($"HighLightSongNote: Reset color for songNote {i}");
            }
        }
        //Debug.Log("HighLightSongNote finished");
        _selectSlices.Clear(); // Xóa danh sách đã chọn sau khi hoàn thành
    }

    private void HighlightSlice(int slice)
    {
        for (int i = 0; i < _slices.Length; i++)
        {
            if (_slices[i].gameObject.activeSelf)
            {
                _slices[i].transform.GetChild(0).gameObject.SetActive(false);
                _slices[i].transform.GetChild(1).gameObject.SetActive(false);
                _slices[i].GetComponent<RectTransform>().sizeDelta = _sliceSize[i];
            }
        }

        if (!_slices[slice].gameObject.activeSelf)
        {
            return;
        }

        _slices[slice].GetComponent<RectTransform>().sizeDelta = _sliceSize[slice] + new Vector2(10, 10);

        _slices[slice].transform.GetChild(0).gameObject.SetActive(true);
        _slices[slice].transform.GetChild(1).gameObject.SetActive(true);
    }

    private void ResetHighlight()
    {
        for (int i = 0; i < _slices.Length; i++)
        {
            if (_slices[i].gameObject.activeSelf)
                _slices[i].GetComponent<RectTransform>().sizeDelta = _sliceSize[i];
        }
    }
    public void OnPlayerWin()
    {
        StartCoroutine(HighLightSongNote());
        //_songWheelTimeForDisplay = 4950;
        for (int i = 0; i < _slices.Length; i++)
        {
            if (_slices[i].gameObject.activeSelf)
            {
                _slices[i].transform.GetChild(1).gameObject.GetComponent<Image>().fillAmount = 0;
            }
        }
    }
    public void OnPlayerLose()
    {
        TurnOfSongNotesWhenLost();
        //_songWheelTimeForDisplay = 4950;
        for (int i = 0; i < _slices.Length; i++)
        {
            if (_slices[i].gameObject.activeSelf)
            {
                _slices[i].transform.GetChild(1).gameObject.GetComponent<Image>().fillAmount = 0;
            }
        }
    }
}
