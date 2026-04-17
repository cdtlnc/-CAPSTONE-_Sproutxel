using UnityEngine;

public class SeedInventoryUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private static readonly int IsOpen = Animator.StringToHash("isOpen");   // converts string to a numeric hash = more efficient way to find parameters 

    public void Open()      // play open animation
    {
        if (animator.GetBool(IsOpen)) return;
        animator.SetBool(IsOpen, true);
    }

    public void Close()     // play close animation
    {
        animator.SetBool(IsOpen, false);
    }
}
