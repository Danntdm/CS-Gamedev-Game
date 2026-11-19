
using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Material flashMaterial;
    public float flashDuration = 0.08f;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Material[][] whiteMaterials;
    private Coroutine flashCoroutine;

    void Awake()
    {
        // Only change the enemy's visible mesh materials.
        Renderer[] allRenderers =
            GetComponentsInChildren<Renderer>(true);

        var meshRenderers =
            new System.Collections.Generic.List<Renderer>();

        foreach (Renderer renderer in allRenderers)
        {
            if (renderer is MeshRenderer ||
                renderer is SkinnedMeshRenderer)
            {
                meshRenderers.Add(renderer);
            }
        }

        renderers = meshRenderers.ToArray();

        originalMaterials = new Material[renderers.Length][];
        whiteMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterials;

            whiteMaterials[i] =
                new Material[originalMaterials[i].Length];

            for (int j = 0; j < whiteMaterials[i].Length; j++)
            {
                whiteMaterials[i][j] = flashMaterial;
            }
        }
    }

    public void Flash()
    {
        if (flashMaterial == null)
            return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        // Briefly replace all visual materials with white.
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].sharedMaterials = whiteMaterials[i];
        }

        yield return new WaitForSeconds(flashDuration);

        RestoreMaterials();

        flashCoroutine = null;
    }

    void RestoreMaterials()
    {
        if (renderers == null || originalMaterials == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].sharedMaterials = originalMaterials[i];
        }
    }

    void OnDisable()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        RestoreMaterials();
    }
}
