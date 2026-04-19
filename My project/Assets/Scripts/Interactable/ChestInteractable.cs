using UnityEngine;

public class ChestInteractable : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private bool opened = false;

    private void OnMouseDown()
    {
        if (opened) return;

        opened = true;

        if (animator != null)
            animator.SetTrigger("Open");

        ChestRewardManager.Instance.Open();
    }
}