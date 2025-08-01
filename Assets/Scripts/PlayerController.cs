using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        private static readonly int IsJump = Animator.StringToHash("isJump");
        private static readonly int IsRun = Animator.StringToHash("isRun");


        public float moveSpeed = 5f;
        public float jumpForce = 10f;
        public Transform groundCheck;
        public bool isSignaling = false;
        public float groundCheckRadius = 0.2f;
        public LayerMask groundLayer;

        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Animator animator;

        private bool _isGrounded;
        private int _jumpCount = 0;
        private Vector2 _lastMoveInput = Vector2.right;
        private int _maxJumpCount = 0;

     

        // invicible
        [SerializeField] private GameObject visualObject;
        [SerializeField] private float invincibleDuration = 1f;
        private bool _isInvincible = false;

        public void Stop()
        {
            rb.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            DetectEnemy();
            CheckGround();
          
            animator.SetBool(IsJump, !_isGrounded);
           
        }

        private void CheckGround()
        {
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (_isGrounded)
            {
                _jumpCount = 0;
                
            }


        }


        public void Move(Vector2 input)
        {
            rb.linearVelocity = new Vector2(input.x * moveSpeed, rb.linearVelocity.y);


            if (input != Vector2.zero)
                _lastMoveInput = input.normalized;

            bool check = Mathf.Abs(input.x) > 0.01f && _isGrounded;

            animator.SetBool(IsRun, Mathf.Abs(input.x) > 0.01f && _isGrounded);

            if (Mathf.Abs(input.x) > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Sign(input.x), 1, 1);
            }

        }

        public void Jump()
        {
            if (_jumpCount <= _maxJumpCount && _isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                _jumpCount++;
                Debug.Log(_jumpCount + " " + _maxJumpCount);
            }
            //debug isground and iswin
            if (GameManager.Instance.IsWin && isSignaling)
            {
                StartCoroutine(DoubleJump());
            }
        }

        public IEnumerator DoubleJump()
        {
            yield return new WaitForSeconds(0.5f);
            if (!_isGrounded)
            {
                _maxJumpCount++; // Cho phép nhảy đôi
                Debug.Log("Double Jump Enabled: " + _maxJumpCount);
                if (_jumpCount <= _maxJumpCount)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce + 5);
                    Debug.Log("Double Jump: " + _jumpCount + " " + _maxJumpCount);
                }
                _maxJumpCount--; // Reset lại số lần nhảy đôi sau khi thực hiện
                isSignaling = false; // Reset trạng thái signaling của player
                GameManager.Instance.IsWin = false;
            }
        }

        public void StartInvincibility()
        {
            if (_isInvincible) return;

            StartCoroutine(InvincibilityCoroutine());
        }

        private IEnumerator InvincibilityCoroutine()
        {
            _isInvincible = true;

            float timer = 0f;
            bool visible = true;

            while (timer < invincibleDuration)
            {
                visible = !visible;
                if (visualObject != null)
                    visualObject.SetActive(visible);

                yield return new WaitForSeconds(0.1f);
                timer += 0.1f;
            }

            if (visualObject != null)
                visualObject.SetActive(true);

            _isInvincible = false;
        }

        private void DetectEnemy()
        {
            float detectRadius = 3.5f; // Bán kính phát hiện enemy
            LayerMask enemyLayer = LayerMask.GetMask("Flying"); // Đảm bảo bạn đã gán layer "Enemy" cho các enemy
                                                                // Draw detection circle in Scene view for debugging
            DrawDebugCircle(transform.position, detectRadius, Color.red);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);

            foreach (var hit in hits)
            {
                // Có thể kiểm tra thêm nếu cần, ví dụ: tag hoặc component
                if (hit.CompareTag("Enemy"))
                {
                    if (GameManager.Instance.IsWin && isSignaling)
                    {
                        hit.gameObject.GetComponent<EnemyController>().StopMovement();
                        isSignaling = false;
                        Debug.Log("Enemy signaling stopped: " + hit.gameObject.name);
                    }
                    else
                    if (!isSignaling)
                    {
                        Debug.Log("Enemy detected: " + hit.gameObject.name);
                        hit.gameObject.GetComponent<EnemyController>().OnSignalDirection += GameManager.Instance.OnEnemySignal;
                        isSignaling = hit.gameObject.GetComponent<EnemyController>().SignalRandomDirection();
                    }

                }
                else if (hit.CompareTag("Bird"))
                {
                    //debug is win and signaling
                    if (GameManager.Instance.IsWin && isSignaling)
                    {
                        Debug.Log("isWin: " + GameManager.Instance.IsWin);
                        if (hit.gameObject.GetComponent<BirdController>() != null)
                            hit.gameObject.GetComponent<BirdController>().FlyIntoPlayer();
                        hit.gameObject.GetComponent<BirdController>().StopMovement();
                    }
                    else
                    if (!isSignaling)
                    {
                        Debug.Log("Bird detected: " + hit.gameObject.name);
                        hit.gameObject.GetComponent<BirdController>().OnSignalDirection += GameManager.Instance.OnEnemySignal;
                        isSignaling = hit.gameObject.GetComponent<BirdController>().SignalRandomDirection();
                    }
                }
            }
        }

        // Helper method to draw a circle in the Scene view
        private void DrawDebugCircle(Vector3 center, float radius, Color color, int segments = 32)
        {
            float angle = 0f;
            Vector3 lastPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            for (int i = 1; i <= segments; i++)
            {
                angle += 2 * Mathf.PI / segments;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                Debug.DrawLine(lastPoint, nextPoint, color, 0.01f);
                lastPoint = nextPoint;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //if (other.CompareTag("EndPoint"))
            //{
            //    Destroy(other.gameObject);
            //    GameController.Instance.ShowEndGame();
            //}

            //if (other.CompareTag("Coin"))
            //{
            //    Destroy(other.gameObject);
            //    GameController.Instance.EarnCoin();
            //}

            if (other.CompareTag("Trap") && !_isInvincible)
            {
                GameManager.Instance.TakeDamage(1);
                StartInvincibility();
            }
        }

    
       

    }


}