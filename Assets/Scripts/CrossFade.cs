using System.Collections;
using UnityEngine;
using DG.Tweening;      // DOTween library for animations

public class CrossFade : SceneTransition
{
    [SerializeField] private CanvasGroup crossFade;

    private void Awake()
    {
        crossFade.alpha = 0f;   // start transparent
        crossFade.blocksRaycasts = false;   // prevents blocked UI interactions
    }

    public override IEnumerator AnimateTransitionIn()   // FadeIn
    {
        var tweener = crossFade.DOFade(1f, 0.8f);
        yield return tweener.WaitForCompletion();
    }

    public override IEnumerator AnimateTransitionOut()  // FadeOut
    {
        var tweener = crossFade.DOFade(0f, 0.8f);
        yield return tweener.WaitForCompletion();
    }
}
