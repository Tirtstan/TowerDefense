using EasyTextEffects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class TextEffectOnSelectHover
    : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerEnterHandler,
        IPointerExitHandler
{
    [Header("Components")]
    [SerializeField]
    private TextEffect textEffect;
    private bool isSelected;
    private bool isHovered;

    private void Awake()
    {
        textEffect.StopAllEffects();
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        UpdateTextEffect();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        UpdateTextEffect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateTextEffect();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateTextEffect();
    }

    private void UpdateTextEffect()
    {
        if (isSelected || isHovered)
        {
            textEffect.StartManualEffects();
        }
        else
        {
            textEffect.StopAllEffects();
        }
    }
}
