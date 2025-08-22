using Game.Scripts.Gameplay;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDetection : MonoBehaviour
{
    [SerializeField] private SongWheelController _songWheelController;
    private PlayerController playerController;
    private AbstractEnemy currentEnemy;
    private SongDirection[] _targetDir;

    public float TimeOut;
    [SerializeField] private float timeOutCountDown;
    private bool _isPlaying;
    private bool _selected;

    private void Awake()
    {
        TimeOut = 15;
        playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        DetectEnemy();
        if (TimeOut<=1)
        {
            GameManager.Instance.GameLose();
            //StopAllCoroutines();

        }
    }

    public float GetTimeOutCountDown()
    {
        return timeOutCountDown;
    }

    public void SetTimeOut(float setTime)
    {
        TimeOut = setTime;
    }

    public float GetTimeOut()
    {
        return TimeOut;
    }

    private void DetectEnemy()
    {
        if (currentEnemy != null)
        {
            if (Vector2.Distance(transform.position, currentEnemy._startPoint) > 10)
            {
                currentEnemy = null;
            }
            return;
        }
        Collider2D[] hits = GetEnemies();

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<AbstractEnemy>();
            if (enemy != null)
            {
                if (enemy.IsWin) continue;
                else
                {
                    OnDetectEnemy(enemy);
                }
            }
        }

    }
    public bool IsPlaying()
    {
        return _isPlaying;
    }

    public GameObject GetCurrentEnemy()
    {
        return currentEnemy?.gameObject;
    }

    private void OnDetectEnemy(AbstractEnemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.Singal += x => StartCoroutine(OnEnemySignal(x));

        if (_isPlaying) return;
        StartCoroutine(StartPlay());
    }

    private IEnumerator OnEnemySignal(SongDirection[] songDirection)
    {
        //_selected = true;
        _isPlaying = true;
        _targetDir = songDirection;
        Debug.Log("on signal dir: " + songDirection[1]);
        yield return null;
        yield return AwaitInput(TimeOut);
    }
    private IEnumerator AwaitInput(float timeOut)
    {
        float t = timeOut; // Start from timeOut
        timeOutCountDown = t;
        while (t > 0 && timeOutCountDown > 0)
        {
            if (_selected) break;
            t -= Time.deltaTime;
            timeOutCountDown = t;
            yield return null;
        }

        //_awaitImage.gameObject.SetActive(false);
        OnPlayerResult(false);
       
    }
    private Collider2D[] GetEnemies()
    {
        float detectRadius = 3.5f;
        LayerMask enemyLayer = LayerMask.GetMask("Flying");
        DrawDebugCircle(transform.position, detectRadius, Color.red);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);
        return hits;
    }

    public void OnSelectSongWheel(int[] sliceIndex)
    {
        _selected = true;
        bool result = CheckCondition(sliceIndex);
        Debug.Log("check result: "+result);
        OnPlayerResult(result);
    }

    private void OnPlayerResult(bool result)
    {
        if (!_selected)
        {
            return;
        }
        if (result)
        {
            OnPlayerWin();
        }
        else
        {
            Debug.Log("On plyaer lose");
            OnPayerLose();
        }
        _selected = false;
    }

    private bool CheckCondition(int[] sliceIndex)
    {
        //if (_targetDir == null) return ;
        bool correct = sliceIndex.Length == 2 && _targetDir != null && sliceIndex.Length == _targetDir.Length;
        if (correct)
        {
            for (int i = 0; i < sliceIndex.Length; i++)
            {
                if (sliceIndex[i] != (int)_targetDir[i])
                {
                    correct = false;
                    break;
                }
            }
        }
        return correct;
    }

    private IEnumerator  StartPlay()
    {
        Debug.Log("start play");
        yield return currentEnemy?.OnPlayerRequest(playerController);
    }
    public void OnPlayerWin()
    {
        StopAllCoroutines();

        //_targetDir.;
        _isPlaying = false;
        playerController.movement.UnStop();
        _songWheelController.OnPlayerWin();
        currentEnemy?.OnWinning();
        currentEnemy = null;
        //_selected = false;
    }
    public void OnPayerLose()
    {
        StopAllCoroutines();
        _targetDir = null;
        _selected = false;
        _isPlaying = false;
        _songWheelController.OnPlayerLose();
        if (currentEnemy == null) return;
        Debug.Log("On enemy lose");
        StartCoroutine(currentEnemy.OnPlayerMissed());
    }

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
}