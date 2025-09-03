using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        GameManager.OnGameEnd += OnGameEnd;
    }

    private void OnGameEnd() => playerInput.DeactivateInput();

    private void OnDestroy()
    {
        GameManager.OnGameEnd -= OnGameEnd;
    }
}
