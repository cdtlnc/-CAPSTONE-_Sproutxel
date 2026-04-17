using UnityEngine;

public class TEST_OKAY : MonoBehaviour
{
    [SerializeField] private GameObject OKAY;       // OKAY

    public void HideUIElement()
    {
        if (OKAY != null)
        {
            OKAY.SetActive(false);
        }
    }
}
