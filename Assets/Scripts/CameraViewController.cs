using UnityEngine;
using System.Collections;

public class CameraViewController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [SerializeField] private Transform frontView;
    [SerializeField] private Transform topView;

    [SerializeField] private float moveSpeed = 2f;

    private bool isTopView = false;

    public void ToggleView()
    {
        StopAllCoroutines();

        if (isTopView)
        {
            StartCoroutine(MoveCamera(frontView));
        }
        else
        {
            StartCoroutine(MoveCamera(topView));
        }

        isTopView = !isTopView;
    }

    IEnumerator MoveCamera(Transform target)
    {
        while (
            Vector3.Distance(mainCamera.transform.position, target.position) > 0.01f ||
            Quaternion.Angle(mainCamera.transform.rotation, target.rotation) > 0.1f
        )
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                target.rotation,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        mainCamera.transform.position = target.position;
        mainCamera.transform.rotation = target.rotation;
    }
}