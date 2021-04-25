 using UnityEngine;
 using UnityEngine.EventSystems;
 using UnityEngine.UI;
  
  // copied from https://answers.unity.com/questions/1313950/unity-ui-mouse-keyboard-navigate-un-highlight-butt.html
  
 [RequireComponent(typeof(Selectable))]
 public class Menu_UnselectHack : MonoBehaviour, IPointerEnterHandler, IDeselectHandler
 {
     public void OnPointerEnter(PointerEventData eventData)
     {
         if (!EventSystem.current.alreadySelecting)
             EventSystem.current.SetSelectedGameObject(this.gameObject);
     }
  
     public void OnDeselect(BaseEventData eventData)
     {
         this.GetComponent<Selectable>().OnPointerExit(null);
     }
 }