using UnityEngine;
public interface ITab
{
    public void Open(ITabOpener opener);
    public void OpenUpdate(ITabOpener opener) { }
    public void Close();
}