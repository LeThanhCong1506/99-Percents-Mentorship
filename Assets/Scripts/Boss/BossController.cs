using DG.Tweening;
using Game.Scripts.Gameplay;
using System.Collections;
using TMPro;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private static readonly int IsAttack = Animator.StringToHash("isAttack");
    private BossCameraController _bossCameraController;

    [SerializeField] private GameObject wideCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bossTrigger;
    [SerializeField] private Transform position;
    [SerializeField] private BossAI bossAI;
    [SerializeField] private float pushLeght = 20f;
    [SerializeField] private float pushTime = 3f;

    private void Start()
    {
        _bossCameraController = FindFirstObjectByType<BossCameraController>();
        //animator = GetComponent<Animator>();
    }

    public void OnDetectPlayer(PlayerController playerController)
    {
        StartCoroutine(DetectProcess(playerController));
    }

    private IEnumerator DetectProcess(PlayerController playerController)
    {
        wideCamera.gameObject.SetActive(true);
        playerController.movement.StopMoving();
        playerController.movement.StopPlayer();
        playerController.movement.enabled = false;
        yield return CutScence();

        yield return PushPlayerBack(playerController);

        _bossCameraController.ChangeState( BossCameraController.State.Boss);
        //var dialog = GameObject.Find("DialogPanel").GetComponent<CanvasGroup>();
        //dialog.alpha = 1f;
        //dialog.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, 75);
        //dialog.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "So cold, maybe i can sing to make him happy, i can do this!!";

        StartCoroutine(bossAI.EnterPlay());
        playerController.movement.enabled = true;
        //playerController.movement.UnStop();

        yield return new WaitForSeconds(4f);
        //dialog.alpha = 0f;

        var instruction = GameObject.Find("Instruction");
        instruction.GetComponent<CanvasGroup>().alpha = 1f;
    }

    private IEnumerator PushPlayerBack(PlayerController playerController)
    {
        bossAI.UpdateUI(0);
        animator.SetBool(IsAttack, true);
        yield return playerController.transform.DOMoveX(playerController.transform.position.x - pushLeght, pushTime).WaitForCompletion();
    }

    private IEnumerator CutScence()
    {
        var sequence = DOTween.Sequence();
        sequence.Append( transform.DOMove(position.position + Vector3.up * 10f, 1).SetEase(Ease.Linear));
        sequence.Append( transform.DOMove(position.position, 0.2f).SetEase(Ease.Linear));
        yield return sequence.WaitForCompletion();
        yield return new WaitForSeconds(2f);
    }
}
