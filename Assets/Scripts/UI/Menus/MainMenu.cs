using LitMotion;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Generator generator;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button generateButton;

    [SerializeField]
    private Button optionsButton;

    [SerializeField]
    private Button exitButton;

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnStartButtonClicked() => GameManager.Instance.StartGame();

    private void OnGenerateButtonClicked() => generator.Generate();

    private void OnOptionsButtonClicked() => Debug.Log("Options button clicked!");

    private void OnExitButtonClicked() => Application.Quit(); // TODO: add confirmation dialog
}
