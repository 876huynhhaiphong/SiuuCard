using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Phần logic hover + đổ mana của card trên tay. Hiển thị (artwork, text, animation)
/// làm sau. Cuộn chuột lên khi đang hover = đổ 1 mana vào card.
/// Yêu cầu: Canvas có GraphicRaycaster + scene có EventSystem (Input System UI Input Module).
/// </summary>
public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardInstance Card { get; private set; }

    private ManaSystem mana;
    private EyeColorController eyes;
    private bool hovering;

    public void Bind(CardInstance card, ManaSystem manaSystem, EyeColorController eyeController)
    {
        Card = card;
        mana = manaSystem;
        eyes = eyeController;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        eyes?.SetHovered(Card);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        eyes?.ClearHovered(Card);
    }

    private void OnDisable()
    {
        if (hovering) eyes?.ClearHovered(Card);
        hovering = false;
    }

    private void Update()
    {
        if (!hovering || Card == null || mana == null || Mouse.current == null) return;

        if (Mouse.current.scroll.ReadValue().y > 0.1f)
            mana.TryInfuse(Card, 1);
    }
}
