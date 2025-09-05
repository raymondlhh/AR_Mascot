using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsAnimation : MonoBehaviour
{
    [Header("Props Animators")]
    public List<Animator> propsAnimators = new List<Animator>();
    

    public void PlaySolarSystem()
    {
        foreach (Animator animator in propsAnimators)
        {
            if (animator != null)
            {
                animator.Play("SolarSystem");
            }
        }
    }

}
