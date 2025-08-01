using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections;

public enum SongDirection
{
    Up = 0,
    UpRight = 7,
    Right = 6,
    DownRight = 5,
    Down = 4,
    DownLeft = 3,
    Left = 2,
    UpLeft = 1
}

public class EnemyController : MonoBehaviour
{
    public float moveDistance = 2f;
    public float moveDuration = 0.2f;

    private Vector3 leftPos;
    private Vector3 rightPos;
    private Vector3 startPos;
    public bool isMoving = true;
    // Sự kiện đưa ra hướng mới  
    public event Action<SongDirection[]> OnSignalDirection;

    private SongDirection[] currentDir;

    void Start()
    {
        // Xác định vị trí ban đầu, trái và phải dựa trên vị trí hiện tại
        Vector3 startPos = transform.position;
        leftPos = startPos + Vector3.left * moveDistance;
        rightPos = startPos + Vector3.right * moveDistance;

        // Bắt đầu di chuyển qua lại
        StartCoroutine(MoveBackAndForth());
    }

    private void Update()
    {
        DetectPlayer();
    }

    private IEnumerator MoveBackAndForth()
    {
        while (isMoving)
        {
            // Đi sang trái
            yield return StartCoroutine(MoveToPosition(leftPos));
            // Đi sang phải
            yield return StartCoroutine(MoveToPosition(rightPos));
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        startPos = transform.position;
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / moveDuration);
            elapsed += Time.deltaTime / 1.5f;
            yield return null;
        }

        transform.position = target;
    }

    private void MoveBackToFirstPosition()
    {
        Vector3 start = transform.position;
        Vector3 endpos = startPos;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, endpos, elapsed / moveDuration);
            elapsed += Time.deltaTime / 1.5f;
        }
        transform.position = leftPos;
    }

    private void DetectPlayer()
    {
        float detectRadius = 3.5f;
        LayerMask playerLayer = LayerMask.GetMask("Player");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                isMoving = false; // Dừng di chuyển khi phát hiện player
                MoveBackToFirstPosition(); // Trở về vị trí ban đầu
                return;
            }
        }
    }


    public bool SignalRandomDirection()
    {
        currentDir = new SongDirection[2];

        for (int i = 0; i < currentDir.Length; i++)
        {
            // Chọn ngẫu nhiên một hướng từ SongDirection  
            currentDir[i] = (SongDirection)Random.Range(0, 7);
        }

        OnSignalDirection?.Invoke(currentDir);
        // Khởi động hành động (di chuyển hoặc effect) theo hướng đó  
        StartCoroutine(MoveInDirection(currentDir));

        return true;
    }
    public void StopMovement()
    {
        this.gameObject.GetComponent<CircleCollider2D>().enabled = false;
        this.gameObject.GetComponent<EnemyController>().enabled = false;
    }

    public void MakeMovement()
    {
        this.gameObject.GetComponent<CircleCollider2D>().enabled = true;
        this.gameObject.GetComponent<EnemyController>().enabled = true;
    }

    private IEnumerator MoveInDirection(SongDirection[] dir)
    {
        yield return new WaitForSeconds(2f); // Đợi một chút trước khi bắt đầu di chuyển
        for (int i = 0; i < dir.Length; i++)
        {
            Vector3 dirVec = DirectionToVector(dir[i]);
            Vector3 start = transform.position;
            Vector3 end = start + dirVec * moveDistance;

            float t = 0f;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, t / moveDuration);
                yield return null;
            }

            t = 0;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(end, start, t / moveDuration);
                yield return null;
            }
            // Đợi một chút trước khi di chuyển tiếp  
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Vector3 DirectionToVector(SongDirection dir)
    {
        // mỗi dir cách nhau 45°, và bạn muốn dir=0 là 90°  
        float angleDeg = (int)dir * 45f + 90f;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
    }
}
