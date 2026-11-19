using UnityEngine;

[DisallowMultipleComponent]
public sealed class HackableTotem : MonoBehaviour
{
    [SerializeField] private Renderer[] statusLights;
    [SerializeField] private Material hackedMaterial;

    public bool IsHacked { get; private set; }

    public void MarkHacked()
    {
        if (IsHacked)
            return;

        IsHacked = true;
        if (statusLights == null || hackedMaterial == null)
            return;

        foreach (Renderer light in statusLights)
        {
            if (light != null)
                light.sharedMaterial = hackedMaterial;
        }
    }
}
