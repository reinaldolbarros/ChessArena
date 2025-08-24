using UnityEngine;
using UnityEngine.UI;

public class GamePanelUI : MonoBehaviour
{
    public static GamePanelUI Instance { get; private set; }

    [Header("Game Control Buttons")]
    [SerializeField] private Button surrenderButton;
    [SerializeField] private Button drawButton;
    [SerializeField] private Button pauseButton;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (surrenderButton != null)
            surrenderButton.onClick.AddListener(() => GameManager.Instance?.SurrenderGame());

        if (drawButton != null)
            drawButton.onClick.AddListener(() => GameManager.Instance?.OfferDraw());
    }

    public void EnableGameButtons(bool enable)
    {
        if (surrenderButton != null)
            surrenderButton.interactable = enable;

        if (drawButton != null)
            drawButton.interactable = enable;

        if (pauseButton != null)
            pauseButton.interactable = enable;
    }
}
