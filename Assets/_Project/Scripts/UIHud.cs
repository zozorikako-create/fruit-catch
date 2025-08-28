using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class UIHud : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI lifeText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI overScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private Button retryButton;

    [Header("Life (Heart Only)")]
    [Tooltip("表示に使うハート文字（❤ が出ない環境では ♥ や ♡ へ変更）")]
    [SerializeField] private string fullHeart = "❤";
    [Tooltip("UIが崩れないための表示上限（例：3）")]
    [SerializeField] private int maxLifeDisplay = 3;

    private void Awake()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (retryButton != null) retryButton.onClick.AddListener(() =>
        {
            var gm = FindObjectOfType<GameManager>();
            gm?.Retry();
        });
    }

    public void UpdateHud(int score, int life, float time)
    {
        if (scoreText) scoreText.text = $"Score: {score:000}";
        if (timeText) timeText.text = $"Time: {time:0.0}";

        if (lifeText)
        {
            int c = Mathf.Clamp(life, 0, maxLifeDisplay);
            lifeText.text = $"Life: {BuildHearts(c)}"; // 残りライフ分だけ❤を並べる
        }
    }

    public void ShowGameOver(int score, int best)
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (overScoreText) overScoreText.text = $"Score: {score}";
        if (bestScoreText) bestScoreText.text = $"Best: {best}";
    }

    public void SetBest(int best)
    {
        if (bestScoreText) bestScoreText.text = $"Best: {best}";
    }

    // 演出用にフックだけ残しておく
    public void PlayCatchEffect() { }
    public void PlayMissEffect() { }

    // "❤" が2バイトの場合にも安全に繰り返し生成
    private string BuildHearts(int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(fullHeart)) return "";
        var sb = new StringBuilder(fullHeart.Length * count);
        for (int i = 0; i < count; i++) sb.Append(fullHeart);
        return sb.ToString();
    }
}