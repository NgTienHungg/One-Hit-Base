using UnityEngine;

public class ClickNative : MonoBehaviour
{
     private void OnMouseDown()
     {
          Debug.Log("Click native");
          //GameManager.Instance.nativeClick = true;
     }
}