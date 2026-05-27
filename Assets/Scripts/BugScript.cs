using UnityEngine;
using UnityEngine.UI;

public class BugScript : MonoBehaviour
{
    private Sprite[] animSprites;
    private Image bugImage;
    private int currentFrame = 0;
    private float animTimer;
    private float frameRate = 0.2f;
    public float throwForce = 300f;

    public void SetupAnimation(Sprite[] sprites)
    {
        animSprites = sprites;
        bugImage = GetComponent<Image>();

        // Set initial starting sprite
        if (animSprites != null && animSprites.Length > 0)
            bugImage.sprite = animSprites[0];
    }

    private void Update()
    {
        // Don't animate if we don't have exactly 2 frames assigned
        if (animSprites == null || animSprites.Length < 2) return;

        animTimer += Time.deltaTime;
        if (animTimer >= frameRate)
        {
            animTimer = 0f;
            // Toggles between index 0 and 1
            currentFrame = currentFrame == 0 ? 1 : 0;
            bugImage.sprite = animSprites[currentFrame];
        }
    }
    public void Removed()
    {
        // 1. Add Rigidbody2D and get reference
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();

        // 2. Ensure a Collider2D exists so it can move/fall
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        // 3. Generate a random normalized direction vector
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // 4. Apply the impulse force to throw the object
        rb.AddForce(randomDirection * throwForce, ForceMode2D.Impulse);

        Destroy(gameObject, 2f);
    }
}
