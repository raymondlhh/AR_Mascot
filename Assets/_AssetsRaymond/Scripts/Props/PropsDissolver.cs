using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialGroup
{
    public string groupName;
    public Material[] materials;
}

public class PropsDissolver : MonoBehaviour
{
    public float dissolveDuration = 2;
    public float dissolveStrength;
    [SerializeField] private MaterialGroup[] targetMaterials;
    
    [Header("Mascot Props Integration")]
    [SerializeField] private MascotProps mascotProps;

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

    // New functions for MascotProps integration
    

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

    private IEnumerator DissolveGroup(string groupName)
    {
        float elapsedTime = 0;

        if (targetMaterials == null || targetMaterials.Length == 0)
        {
            yield break;
        }

        // Find the specific group
        MaterialGroup targetGroup = null;
        for (int i = 0; i < targetMaterials.Length; i++)
        {
            if (targetMaterials[i] != null && targetMaterials[i].groupName == groupName)
            {
                targetGroup = targetMaterials[i];
                break;
            }
        }

        if (targetGroup == null || targetGroup.materials == null)
        {
            yield break;
        }

        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;

            dissolveStrength = Mathf.Lerp(0, 1, elapsedTime / dissolveDuration);
            
            // Only apply dissolve to the specific group
            for (int j = 0; j < targetGroup.materials.Length; j++)
            {
                if (targetGroup.materials[j] == null) continue;
                targetGroup.materials[j].SetFloat("_DissolveStrength", dissolveStrength);
            }

            yield return null;
        }
    }

    private IEnumerator UnDissolveGroup(string groupName)
    {
        float elapsedTime = 0;

        if (targetMaterials == null || targetMaterials.Length == 0)
        {
            yield break;
        }

        // Find the specific group
        MaterialGroup targetGroup = null;
        for (int i = 0; i < targetMaterials.Length; i++)
        {
            if (targetMaterials[i] != null && targetMaterials[i].groupName == groupName)
            {
                targetGroup = targetMaterials[i];
                break;
            }
        }

        if (targetGroup == null || targetGroup.materials == null)
        {
            yield break;
        }

        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;

            dissolveStrength = Mathf.Lerp(1, 0, elapsedTime / dissolveDuration);
            
            // Only apply undissolve to the specific group
            for (int j = 0; j < targetGroup.materials.Length; j++)
            {
                if (targetGroup.materials[j] == null) continue;
                targetGroup.materials[j].SetFloat("_DissolveStrength", dissolveStrength);
            }

            yield return null;
        }
    }

    #region Animation Events
    public void DissolveAll()
    {
        StartCoroutine(Dissolve());
    }

    public void UnDissolveAll()
    {  
        StartCoroutine(UnDissolve());
    }

    public void DissolveQuest3()
    {
        StartCoroutine(DissolveGroup("Quest3"));
    }

    public void UnDissolveQuest3()
    {
        StartCoroutine(UnDissolveGroup("Quest3"));
    }
    #endregion

    
}
