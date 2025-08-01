using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class SongWheelController : MonoBehaviour
{
    private static readonly int isSing = Animator.StringToHash("isSing");

    [Header("UI")]
    public RectTransform wheelRect;       // RectTransform của SongWheelBackground
    public Image[] slices;                // Mảng 8 Image Slice0…Slice7

    [SerializeField] private GameManager gameManager; // Tham chiếu đến GameManager
    [SerializeField] private Animator animator;
    //[Header("Audio")]
    //public AudioClip[] noteClips;         // 8 nốt nhạc tương ứng
    //public AudioSource audioSource;

    private List<int> selectSlices;
    private bool wheelActive = false;
    private int currentSlice = -1;
    private Vector2[] sliceSize;

    void Start()
    {
        selectSlices = new List<int>();
        sliceSize = new Vector2[slices.Length];
        // Lưu kích thước ban đầu của từng slice
        for (int i = 0; i < slices.Length; i++)
        {
            sliceSize[i] = slices[i].GetComponent<RectTransform>().sizeDelta;
        }
    }

    void Update()
    {
        // Khi nhấn giữ chuột phải (hoặc trái tuỳ bạn)
        if (!wheelActive && Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.None; // Mở khóa con trỏ chuột
            Cursor.visible = true;
            ActivateWheel();
        }

        if (wheelActive)
        {
            UpdateSelection();

            if (Input.GetMouseButtonDown(0))
            {
                selectSlices.Add(currentSlice);
                if (gameManager != null && selectSlices.Count == 2)
                {
                    gameManager.OnPlayerSelect(selectSlices.ToArray());
                    selectSlices.Clear(); // Xoá danh sách đã chọn sau khi gửi
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = CursorLockMode.Locked; // Khóa con trỏ chuột
                Cursor.visible = false;
                ReleaseWheel();
            }
        }
    }

    public void ActivateWheel()
    {
        animator.SetBool(isSing, true);
        wheelActive = true;
        wheelRect.gameObject.SetActive(true);
        // Đặt wheel tại vị trí chuột
        wheelRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }



    public void ReleaseWheel()
    {
        animator.SetBool(isSing, false);
        wheelActive = false;
        wheelRect.gameObject.SetActive(false);

        //if (currentSlice >= 0)
        //{
        //    //    PlayNote(currentSlice);
        //    // Thông báo cho GameManager
        //}

        ResetHighlight();
        currentSlice = -1;
    }

    void UpdateSelection()
    {
        Vector2 mousePos = Input.mousePosition;
        // Dùng để lấy vị trí con chuột trong wheelRect, local point là bên trong của RectTransform
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            wheelRect, mousePos, null, out localPoint);

        // Tính góc
        float angle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // Xác định slice (360°/8 = 45° mỗi slice)
        int slice = 0;
        if (angle >= 20 && angle < 70) slice = 7;
        if (angle >= 70 && angle < 110) slice = 0;
        if (angle >= 110 && angle < 160) slice = 1;
        if (angle >= 160 && angle < 200) slice = 2;
        if (angle >= 200 && angle < 250) slice = 3;
        if (angle >= 250 && angle < 290) slice = 4;
        if (angle >= 290 && angle < 340) slice = 5;
        if (angle >= 340 || angle < 20) slice = 6;


        if (slice != currentSlice)
        {
            HighlightSlice(slice);
            currentSlice = slice;
        }
    }

    void HighlightSlice(int slice)
    {
        // Reset toàn bộ
        for (int i = 0; i < slices.Length; i++)
            slices[i].gameObject.GetComponent<RectTransform>().sizeDelta = sliceSize[i];

        // Highlight slice đang chọn
        slices[slice].GetComponent<RectTransform>().sizeDelta = new Vector2(slices[slice].GetComponent<RectTransform>().sizeDelta.x + 10, slices[slice].GetComponent<RectTransform>().sizeDelta.y + 10);
    }

    void ResetHighlight()
    {
        for (int i = 0; i < slices.Length; i++)
            slices[i].gameObject.GetComponent<RectTransform>().sizeDelta = sliceSize[i];
    }

    //void PlayNote(int slice)
    //{
    //    if (slice >= 0 && slice < noteClips.Length)
    //    {
    //        audioSource.PlayOneShot(noteClips[slice]);
    //        // TODO: Gọi thêm logic tương tác với thế giới
    //        Debug.Log($"Played note {slice}");
    //    }
    //}
}
