using UnityEngine;
using UnityEngine.Purchasing;

namespace OneHit.IAP
{
     public class IAPListener : MonoBehaviour
     {
          public void OnPurchased(Product product)
          {
               Purchaser.Instance.OnPurchased(product);
          }

          public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
          {
               Purchaser.Instance.OnPurchaseFailed(product, reason);
          }

          public void Restore()
          {
               Purchaser.Instance.CheckRestore();
          }

          public void OpenURL(string link)
          {
               Application.OpenURL(link);
          }
     }
}