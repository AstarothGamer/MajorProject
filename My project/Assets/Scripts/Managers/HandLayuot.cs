using System.Collections.Generic;
using UnityEngine;

public class HandLayout : MonoBehaviour
{
    [Header("Layout Settings")]
    public float spacing = 150f;     
    public float maxWidth = 800f;    
    public float curveHeight = 50f;  
    public float rotationAngle = 10f;
    public float yTransform = 100f;

    public void UpdateLayout(List<Card> cards)
    {
        int count = cards.Count;
        if (count == 0) return;

        float totalWidth = Mathf.Min(maxWidth, spacing * (count - 1));
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            Card card = cards[i];

            float t = count == 1 ? 0.5f : (float)i / (count - 1);

            float x = startX + t * totalWidth;

            float y = -Mathf.Pow((t - 0.5f) * 2, 2) * curveHeight;

            float angle = Mathf.Lerp(-rotationAngle, rotationAngle, t);

            card.SetLayoutTransform(new Vector2(x, yTransform + y), angle);
            
            card.transform.SetSiblingIndex(i);
        }
    }
}
