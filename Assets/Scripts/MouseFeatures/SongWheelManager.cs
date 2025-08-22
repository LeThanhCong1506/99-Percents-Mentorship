using Game.Scripts.Gameplay;
using System.Collections.Generic;
using UnityEngine;

public class SongWheelManager : MonoBehaviour
{
    public static SongWheelManager Instance;
    private List<int> _selectSlices = new();
    private Vector2[] _sliceSize;
    private int newSongWheelNumber;
    //private float _currentFillAmount = 1f;
    public int[] DirectionNumber => _directionNumberList.ToArray();
    private List<int> _directionNumberList;
    private int _currentDirectionIndex;
    public SongWheelView view;
    private void Awake()
    {
        Instance = this;
    }
    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        StartCoroutine(view.ActivateWheel());
    //    }

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        if (!view.WheelActive) return;

    //    }

    //    if (Input.GetMouseButtonUp(1))
    //    {
    //        StartCoroutine(view.ReleaseWheel());
    //    }
    //}
    private void Start()
    {
        _currentDirectionIndex = 6;

        SetupDirectionNumbers();
    }
    public SongDirection GetRandomSongDirection()
    {
        return (SongDirection)DirectionNumber[Random.Range(0, DirectionNumber.Length)];
    }
    private void SetupDirectionNumbers()
    {
        _directionNumberList = new List<int>();
        _directionNumberList.Add(0);
        _directionNumberList.Add(7);
    }
    public void AddMoreRandomSongWheelNumbers()
    {
        if (_currentDirectionIndex == 0)
            return;
        _directionNumberList.Add(_currentDirectionIndex);
        _currentDirectionIndex--;
    }

    private void SelectSongWheel()
    {
        if (!view.WheelActive) return;
        if (view.Slices[view.CurrentSlice].gameObject.activeSelf)
        {
            _selectSlices.Add(view.CurrentSlice);
            //AppearSongNotes();
        }
        if (_selectSlices.Count == 2)
        {
            //_playerController.detection.SelectSongWheel(_selectSlices.ToArray());
        }
    }
}
