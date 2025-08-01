using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerController player;
    [SerializeField] private SongWheelController songWheelController;
    private float inputTimeout = 10f;
    private bool awaitingInput = false;
    private SongDirection[] targetDir;
    private int _userPositivePoint = 3;
    private bool _isGameEnd;
    private bool _isGamePaused;
    public bool IsWin;
    public bool IsWinToStopEnemy;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ẩn và khóa con trỏ chuột trong cửa sổ game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        IsWinToStopEnemy = false;
        IsWin = false;
        _userPositivePoint = 3;
    }

    void Update()
    {
        if (_isGamePaused) return;
        if (_isGameEnd) return;

        HandleKeyboardInput();
    }

    public void OnEnemySignal(SongDirection[] dir)
    {
        targetDir = dir;
        // Mở Song Wheel tại vị trí nào đó hoặc ở giữa màn hình
        awaitingInput = true;
        StartCoroutine(WaitForInput());
    }

    private IEnumerator WaitForInput()
    {
        float timer = 0f;
        while (awaitingInput && timer < inputTimeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (awaitingInput)
        {
            // Timeout, xem như chọn sai
            OnPlayerResult(false);
        }
    }

    // Gọi từ SongWheelController khi người chơi nhả chuột
    public void OnPlayerSelect(int[] sliceIndex)
    {
        if (!awaitingInput) return;

        awaitingInput = false;
        bool correct = false;
        //check sliceindex có bằng targetDir không, nếu có 1 cái sai thì trả về false luôn
        if (sliceIndex.Length == 2)
        {
            if (sliceIndex.Length != targetDir.Length)
            {
                correct = false;
            }
            else
            {
                correct = true;
                for (int i = 0; i < sliceIndex.Length; i++)
                {
                    Debug.Log($"Slice {sliceIndex[i]} và hướng {(int)targetDir[i]}");
                    if (sliceIndex[i] != (int)targetDir[i])
                    {
                        correct = false;
                        break;
                    }
                }
            }
            OnPlayerResult(correct);
        }
    }

    private void OnPlayerResult(bool success)
    {
        if (success)
        {
            Debug.Log("Đúng! Enemy bị cảm hóa.");
            // TODO: hiệu ứng cảm hóa, thưởng điểm…
            IsWin = true;
            IsWinToStopEnemy = true; // Đặt trạng thái thắng để dừng enemy
        }
        else
        {
            IsWin = false;
            player.isSignaling = false; // Reset trạng thái signaling của player
            Debug.Log("Sai! Bị trượt.");
            // TODO: xử lý thất bại (giảm HP, replay…)
        }
    }

    private void HandleKeyboardInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D hoặc ←/→
        Vector2 input = new Vector2(horizontal, 0f);
        player.Move(input);

        // Nhảy với Space hoặc W
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            player.Jump();
        }
    }

    public void TakeDamage(int i)
    {
        _userPositivePoint--;
        if (_userPositivePoint == 0)
        {
            ShowFailGame();
        }
    }

    public void ShowFailGame()
    {
        if (_isGameEnd) return;
        _isGameEnd = true;
        PauseGame();
    }

    public void PauseGame()
    {
        player.Stop();
        _isGamePaused = true;
    }

    public void ResumeGame() => _isGamePaused = false;
}
