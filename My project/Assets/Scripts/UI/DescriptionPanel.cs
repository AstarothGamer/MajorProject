using TMPro;
using UnityEngine;

public class DescriptionPanel : MonoBehaviour
{
    public static DescriptionPanel Instance;

    public GameObject root;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statText;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string title, string description, string stats, Vector2 position)
    {
        root.SetActive(true);

        titleText.text = title;
        descriptionText.text = description;
        statText.text = stats;

        root.transform.position = position;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}