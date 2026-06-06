using UnityEngine;
using UnityEngine.UI;

public class BugScript : MonoBehaviour
{
    private Sprite[] animSprites;
    private Image bugImage;
    private int currentFrame = 0;
    private float animTimer;
    private float frameRate = 0.2f;

    [Header("Death Animation Settings")]
    public float throwForce = 800f; // Increased value for punchy UI scaling
    public float gravity = 2500f;

    private Vector3 _velocity;
    private bool _isRemoved;

    public void SetupAnimation(Sprite[] sprites)
    {
        animSprites = sprites;
        bugImage = GetComponent<Image>();

        if (animSprites != null && animSprites.Length > 0)
            bugImage.sprite = animSprites[0];
    }

    private void Update()
    {
        if (_isRemoved)
        {
            // Simulate manual UI physics for the throwing effect
            _velocity.y -= gravity * Time.deltaTime;
            transform.localPosition += _velocity * Time.deltaTime;

            // Spin the bug as it flies off
            transform.Rotate(0, 0, 500f * Time.deltaTime);
            return;
        }

        if (animSprites == null || animSprites.Length < 2) return;

        animTimer += Time.deltaTime;
        if (animTimer >= frameRate)
        {
            animTimer = 0f;
            currentFrame = currentFrame == 0 ? 1 : 0;
            bugImage.sprite = animSprites[currentFrame];
        }
    }

    public void Removed()
    {
        if (_isRemoved) return;
        _isRemoved = true;

        // Disable the button component so players can't click it mid-air
        if (TryGetComponent<Button>(out Button btn))
        {
            btn.interactable = false;
        }

        // Swap to a death frame if you add a 3rd sprite to your array
        if (animSprites != null && animSprites.Length > 2)
        {
            bugImage.sprite = animSprites[2];
        }

        // Generate a random outward velocity vector
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        // Force an upward arc slant to look like a pop explosion
        if (randomDir.y < 0) randomDir.y = -randomDir.y;

        _velocity = new Vector3(randomDir.x, randomDir.y, 0) * throwForce;

        Destroy(gameObject, 1.5f);
    }
}
