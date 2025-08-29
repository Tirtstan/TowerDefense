using UnityEngine;

public interface IGameSelectable
{
    public Transform Transform { get; }
    public void Select();
    public void Deselect();
}
