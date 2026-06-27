using UnityEngine;

public class PanelMover : MonoBehaviour
{
    public Animator anim;
    [SerializeField]public bool cheese;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cheese)
        {
            anim.SetTrigger("Dih");   
        }
    }
}
