using UnityEngine;

[ExecuteInEditMode]
public class CameraBlur : MonoBehaviour
{
    public Material blurMaterial;
    [Range(0.0f, 5.0f)]
    public float blurSize = 5f;
    [Range(0.0f, 1.0f)]
    public float blurRadius = 0f;
    [Range(0.0f, 1.0f)]
    public float blurFalloff = 1f;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (blurMaterial != null)
        {
            blurMaterial.SetFloat("_BlurSize", blurSize);
            blurMaterial.SetVector("_BlurCenter", new Vector4(0.5f, 0.5f, 0, 0)); // center
            blurMaterial.SetFloat("_BlurRadius", blurRadius);
            blurMaterial.SetFloat("_BlurFalloff", blurFalloff);
            Graphics.Blit(src, dest, blurMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}