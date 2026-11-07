using UnityEngine;

/// <summary>
/// Đổi màu mắt nhân vật theo card đang hover / mức mana đã đổ vào card.
/// Gắn lên object mắt, kéo các SpriteRenderer của mắt vào `eyes`.
/// </summary>
public class EyeColorController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] eyes;
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private float lerpSpeed = 8f;
    [Tooltip("Khi hover: pha màu bậc hiện tại với màu bậc kế tiếp để xem trước.")]
    [SerializeField, Range(0f, 1f)] private float hoverPreviewBlend = 0.35f;

    private ManaSystem mana;
    private CardInstance hovered;
    private Color target;

    private void Awake() => target = idleColor;

    public void Bind(ManaSystem manaSystem)
    {
        if (mana != null) mana.OnCardInfused -= HandleInfused;
        mana = manaSystem;
        if (mana != null) mana.OnCardInfused += HandleInfused;
    }

    private void OnDestroy()
    {
        if (mana != null) mana.OnCardInfused -= HandleInfused;
    }

    public void SetHovered(CardInstance card)
    {
        hovered = card;
        Refresh();
    }

    public void ClearHovered(CardInstance card)
    {
        if (hovered != card) return;
        hovered = null;
        Refresh();
    }

    private void HandleInfused(CardInstance card)
    {
        if (card == hovered) Refresh();
    }

    private void Refresh()
    {
        if (hovered == null)
        {
            target = idleColor;
            return;
        }

        Color current = hovered.CurrentEyeColor;
        Color next = hovered.NextTier != null ? hovered.NextTier.eyeColor : current;
        target = Color.Lerp(current, next, hoverPreviewBlend);
    }

    private void Update()
    {
        float t = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
        foreach (var eye in eyes)
            if (eye != null) eye.color = Color.Lerp(eye.color, target, t);
    }
}
