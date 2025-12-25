using UnityEngine;
public class Tab : MonoBehaviour
{
    public virtual void Open(ITabOpener opener) { gameObject.SetActive(true); }
    public virtual void OpenUpdate(ITabOpener opener) { }
    public virtual void Close() { gameObject.SetActive(false); }
}