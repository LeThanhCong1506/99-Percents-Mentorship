using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private static readonly int IsJump = Animator.StringToHash("isJump");
    private static readonly int IsRun = Animator.StringToHash("isRun");
    private static readonly int IsGiveUp = Animator.StringToHash("isGiveUp");
    [Header("Movement")]
    public float _moveSpeed = 5f;
    public float _modifierSpeed = 1f;
    public float _runModifier = 1f;
    public float _jumpForce = 10f;
    public Transform _groundCheck;
    public float _groundCheckRadius = 0.2f;
    public LayerMask _groundLayer;
    public bool _canRun;

    // --- NEW: Feather particle prefab & settings ---
    [Header("Feather Effect")]
    [SerializeField] private ParticleSystem _featherParticlePrefab;
    [SerializeField] private AudioClip _jumpSFX;

    [Header("State")]
    private bool _checkDoubleJump = false;
    public bool CheckDoubleJump => _checkDoubleJump;

    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;
    private bool _isGrounded;
    private bool _isStop;
    [SerializeField] private bool _isRunning;
    private int _jumpCount;
    private int _maxJumpCount;
    public void UnlockRun()
    {
        _canRun = true;
    }
    private void Update()
    {
        if (_isStop)
        {
            return;
        }
        HandleKeyboardInput();
        CheckGround();
        _animator.SetBool(IsJump, !_isGrounded);
    }
    private void CheckGround()
    {
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        if (_isGrounded) _jumpCount = 0;
    }
    public void PlayGiveUpAnimation()
    {
        _animator.SetTrigger(IsGiveUp);
        _isStop = true;
    }
    public void Move(Vector2 input)
    {
        if (_isStop) return;

        if (float.IsNaN(input.x) || float.IsNaN(input.y)) return; // Không hợp lệ

        _rb.linearVelocity = new Vector2(input.x * ModifierSpeed(), _rb.linearVelocity.y);
        _animator.SetBool(IsRun, Mathf.Abs(input.x) > 0.01f && _isGrounded);

        if (Mathf.Abs(input.x) > 0.01f)
            transform.localScale = new Vector3(Mathf.Sign(input.x), 1, 1);
    }

    public float ModifierSpeed()
    {
        return _moveSpeed * _modifierSpeed * _runModifier;
    }
    public void Run(bool value)
    {
        if (!_canRun) return;
        _runModifier = value ? 1.5f : 1;
    }
    public void Jump()
    {
        if (_isStop) return;
        if (_jumpCount <= _maxJumpCount && _isGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _jumpCount++;
            if (_checkDoubleJump)
                StartCoroutine(DoubleJump());
        }

    }

    public IEnumerator DoubleJump()
    {
        yield return new WaitForSeconds(0.5f);
        if (!_isGrounded)
        {
            _maxJumpCount++;
            if (_jumpCount <= _maxJumpCount)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce + 2.5f);

                // Play feather particle at a specific position (e.g., player's feet)
                if (_featherParticlePrefab != null)
                {
                    ParticleSystem feather = Instantiate(
                        _featherParticlePrefab,
                        transform.position + Vector3.up * 2f, // adjust offset as needed
                        Quaternion.identity
                    );
                    feather.Play();
                    Destroy(feather.gameObject, feather.main.duration);
                }
                AudioManager.Instance.PlaySFX(_jumpSFX);
            }
            _maxJumpCount--;
            _checkDoubleJump = false;
        }
    }

    public bool GetCheckGround()
    {
        return _isGrounded;
    }
    public void StopPlayer()
    {
        _isStop = true;
        _rb.linearVelocityX = 0f;
        _animator.SetBool(IsRun, false);
        if (GameManager.Instance == null) return;
        //GameManager.Instance.IsInputEnable = false;
    }
    public void StopMoving()
    {
        this.enabled = false;
    }
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            InputManager.Instance.UnlockCursor();
        Run(Input.GetKey(KeyCode.LeftShift));
        //if (!IsInputEnable)
        //    return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        Vector2 input = new Vector2(horizontal, 0f);
        Move(input);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            Jump();
    }

    public void SetModifierSpeed(float mult)
    {
        _modifierSpeed = mult;
        Debug.Log("move speed" + _moveSpeed);
    }

    public void UnStop()
    {
        _isStop = false;
        if (GameManager.Instance != null)
        GameManager.Instance.IsInputEnable = true;
    }

    public void CanDoubleJump()
    {
        _checkDoubleJump = true;
    }

    public void LockDoubleJump()
    {
        _checkDoubleJump = false;
    }
}
