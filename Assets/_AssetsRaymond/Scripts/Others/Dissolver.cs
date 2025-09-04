using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialGroup
{
    public string groupName;
    public Material[] materials;
}

public class Dissolver : MonoBehaviour
{
    public float dissolveDuration = 2;
    public float dissolveStrength;
    [SerializeField] private MaterialGroup[] targetMaterials;

    private void Start()
    {
        // Initialize all target materials to fully dissolved (strength = 1)
        dissolveStrength = 1f;
        if (targetMaterials == null || targetMaterials.Length == 0) return;
        for (int i = 0; i < targetMaterials.Length; i++)
        {
            MaterialGroup group = targetMaterials[i];
            if (group == null || group.materials == null) continue;
            for (int j = 0; j < group.materials.Length; j++)
            {
                if (group.materials[j] == null) continue;
                group.materials[j].SetFloat("_DissolveStrength", dissolveStrength);
            }
        }
    }


    public void StartDissolver()
    {
        StartCoroutine(Dissolve());
    }

    public void StartUnDissolver()
    {
        StartCoroutine(UnDissolve());
    }

    private IEnumerator Dissolve()
    {
        float elapsedTime = 0;

        if (targetMaterials == null || targetMaterials.Length == 0)
        {
            yield break;
        }

        while ( elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;

            dissolveStrength = Mathf.Lerp(0, 1, elapsedTime / dissolveDuration);
            for (int i = 0; i < targetMaterials.Length; i++)
            {
                MaterialGroup group = targetMaterials[i];
                if (group == null || group.materials == null) continue;
                for (int j = 0; j < group.materials.Length; j++)
                {
                    if (group.materials[j] == null) continue;
                    group.materials[j].SetFloat("_DissolveStrength", dissolveStrength);
                }
            }

            yield return null;

        }
    }

    private IEnumerator UnDissolve()
    {
        float elapsedTime = 0;

        if (targetMaterials == null || targetMaterials.Length == 0)
        {
            yield break;
        }

        while ( elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;

            dissolveStrength = Mathf.Lerp(1, 0, elapsedTime / dissolveDuration);
            for (int i = 0; i < targetMaterials.Length; i++)
            {
                MaterialGroup group = targetMaterials[i];
                if (group == null || group.materials == null) continue;
                for (int j = 0; j < group.materials.Length; j++)
                {
                    if (group.materials[j] == null) continue;
                    group.materials[j].SetFloat("_DissolveStrength", dissolveStrength);
                }
            }

            yield return null;

        }
    }

    
}
