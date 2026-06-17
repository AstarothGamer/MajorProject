using UnityEngine;

public class OutlineOnPoint : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material material;

    public int index = 1;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
        
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
            material = targetRenderer.materials[index];
    }

    public void Outline(bool show)
    {
        material.SetFloat("_ShowOutline", show ? 1 : 0);
    }
}
