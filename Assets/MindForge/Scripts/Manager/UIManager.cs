using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IGameStateListener
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject gameoverPanel;

    [Header("Info")]
    [SerializeField] private GameObject info;
    [SerializeField] private TextMeshProUGUI infoText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        info.SetActive(false);
    }

    public void GameStateChangedCallback(EGameState gameState)
    {
        menuPanel.SetActive(gameState == EGameState.MENU);
        gamePanel.SetActive(gameState == EGameState.GAME);
        levelCompletePanel.SetActive(gameState == EGameState.LEVELCOMPLETE);
        gameoverPanel.SetActive(gameState == EGameState.GAMEOVER);
    }

    public void ShowInfo(string message, float duration = 2f)
    {
        LeanTween.cancel(info); // Hủy tất cả các tween đang chạy trên info để tránh xung đột

        infoText.text = message;
        info.SetActive(true);

        info.transform.localScale = Vector3.zero; // Bắt đầu từ scale 0 để có hiệu ứng pop-up

        LeanTween.scale(info, Vector3.one, .4f)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true); // Đảm bảo tween hoạt động ngay cả khi Time.timeScale = 0

        LeanTween.delayedCall(info, duration, () => HideInfo());
    }

    private void HideInfo()
    {
        LeanTween.scale(info, Vector3.zero, 0.3f)
            .setEase(LeanTweenType.easeInBack)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                info.SetActive(false);
            });
    }
}
