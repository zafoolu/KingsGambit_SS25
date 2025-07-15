
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Settings")]
    [SerializeField] private string tooltipText = "Default Tooltip Text";
    [SerializeField] private bool useDynamicText = false;
    
    // Für dynamischen Text
    private System.Func<string> dynamicTextFunction;
    
    // UI Event Handlers
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }
    
    private void ShowTooltip()
    {
        if (useDynamicText && dynamicTextFunction != null)
        {
            // Für dynamischen Text: Funktion übergeben, kein Timer
            TooltipScreenSpaceUI.ShowTooltip_Static(dynamicTextFunction, null);
        }
        else
        {
            // Für statischen Text: String übergeben, kein Timer
            TooltipScreenSpaceUI.ShowTooltip_Static(tooltipText, null);
        }
    }
    
    private void HideTooltip()
    {
        TooltipScreenSpaceUI.HideTooltip_Static();
    }
    
    // Öffentliche Methoden zum Setzen des Texts
    public void SetTooltipText(string newText)
    {
        tooltipText = newText;
        useDynamicText = false;
    }
    
    public void SetDynamicTooltipText(System.Func<string> textFunction)
    {
        dynamicTextFunction = textFunction;
        useDynamicText = true;
    }
    
    // Für formatierte Tooltips
    public void SetFormattedTooltip(string title, string description)
    {
        tooltipText = $"<b>{title}</b>\n{description}";
        useDynamicText = false;
    }
    
    public void SetFormattedTooltip(string title, string description, string additionalInfo)
    {
        tooltipText = $"<b>{title}</b>\n{description}\n<i>{additionalInfo}</i>";
        useDynamicText = false;
    }
}