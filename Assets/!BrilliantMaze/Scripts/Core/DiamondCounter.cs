using TMPro;
using UnityEngine;

public class DiamondCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private int count;

    private void Awake()
    {
        if (text == null) text = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    public void Increment()
    {
        count++;
        UpdateText();
    }

    private void UpdateText() => text.text = count.ToString();
}