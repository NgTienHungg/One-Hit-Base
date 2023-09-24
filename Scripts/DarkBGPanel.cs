using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace OneHit.Internet
{
     public class DarkBGPanel : MonoBehaviour
     {
          [Header("Background")]
          public Image darkBG;
          public float appearDuration = 0.4f;
          public float disappearDuration = 0.3f;

          [Header("Popup")]
          public Transform popupTransform;
          public CanvasGroup popupCanvas;
          public float startScale = 0.5f;

          protected virtual void OnValidate()
          {
               darkBG = GetComponent<Image>();
               popupTransform = transform.Find("Popup");
               popupCanvas = popupTransform.GetComponent<CanvasGroup>();
          }

          protected virtual void Awake()
          {
               GetComponent<CanvasGroup>().alpha = 1f;
          }

          protected virtual void Start()
          {
               gameObject.SetActive(false);

               darkBG.color = new Color(1, 1, 1, 0);

               popupCanvas.alpha = 0;
               popupCanvas.interactable = false;
               popupTransform.localScale = Vector3.one * startScale;
          }

          public virtual void Enable()
          {
               gameObject.SetActive(true);

               darkBG.DOFade(1f, appearDuration).SetEase(Ease.OutCubic);

               popupTransform.DOScale(1f, appearDuration).SetEase(Ease.OutBack);
               popupCanvas.DOFade(1f, appearDuration).OnComplete(() =>
               {
                    popupCanvas.interactable = true;
               });
          }

          public virtual void Disable()
          {
               darkBG.DOFade(0f, disappearDuration).SetEase(Ease.OutCubic).OnComplete(() =>
               {
                    gameObject.SetActive(false);
               });

               popupCanvas.interactable = false;
               popupCanvas.DOFade(0f, disappearDuration);
               popupTransform.DOScale(startScale, disappearDuration).SetEase(Ease.InBack);
          }
     }
}