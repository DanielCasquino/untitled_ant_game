using System.Collections;
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

    public void OnDamage()
    {
        StopAllCoroutines();
        StartCoroutine(MakeVignetteRed());
    }

    public IEnumerator MakeVignetteRed()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        blurMaterial.SetColor("_VignetteColor", Color.red);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            blurMaterial.SetColor("_VignetteColor", Color.Lerp(Color.red, Color.black, t));
            yield return null;
        }
        blurMaterial.SetColor("_VignetteColor", Color.black);
    }
}