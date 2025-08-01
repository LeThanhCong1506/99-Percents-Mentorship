using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class BirdController : MonoBehaviour
{
    public float moveDistance = 2f;
    public float moveDuration = 0.5f;
    public float flySpeed = 10f;
    private float hoverHeight = 1.5f;
    public float stayDuration = 10f;


    // Sự kiện đưa ra hướng mới  
    public event Action<SongDirection[]> OnSignalDirection;

    private SongDirection[] currentDir;

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
                t += Time.deltaTime * 1.8f;
                transform.position = Vector3.Lerp(start, end, t / moveDuration);
                yield return null;
            }

            t = 0;
            while (t < moveDuration)
            {
                t += Time.deltaTime * 1.8f;
                transform.position = Vector3.Lerp(end, start, t / moveDuration);
                yield return null;
            }
            // Đợi một chút trước khi di chuyển tiếp  
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void StopMovement()
    {
        this.gameObject.GetComponent<CircleCollider2D>().enabled = false;
        this.gameObject.GetComponent<BirdController>().enabled = false;
    }

    public void MakeMovement()
    {
        this.gameObject.GetComponent<CircleCollider2D>().enabled = true;
        this.gameObject.GetComponent<BirdController>().enabled = true;
    }

    private Vector3 DirectionToVector(SongDirection dir)
    {
        // mỗi dir cách nhau 45°, và bạn muốn dir=0 là 90°  
        float angleDeg = (int)dir * 45f + 90f;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
    }

    public void FlyIntoPlayer()
    {
        StartCoroutine(FlyCoroutine());
    }

    private IEnumerator FlyCoroutine()
    {
        Vector3 offset = Vector3.up * hoverHeight;
        Vector3 targetPos = GameManager.Instance.player.gameObject.transform.position + offset;

        // 1. Fly tới vị trí hover trên Player
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                flySpeed * Time.deltaTime
            );
            yield return null;
        }

        // 2. Ở lại "bám theo" Player trong stayDuration
        float elapsed = 0f;
        while (elapsed < stayDuration)
        {
            // Cập nhật vị trí mục tiêu liên tục
            targetPos = GameManager.Instance.player.gameObject.transform.position + offset;
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                flySpeed * Time.deltaTime
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Sau khi kết thúc stayDuration, có thể chuyển sang trạng thái khác
        OnStayFinished();
    }

    private void OnStayFinished()
    {
        // Ví dụ: bay đi chỗ khác hoặc hạ cánh
        Debug.Log("Bird stay time ended.");
        // TODO: logic tiếp theo
    }
}
