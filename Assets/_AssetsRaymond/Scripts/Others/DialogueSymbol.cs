using DG.Tweening;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

// Static manager for tracking dialogue symbols
public static class SymbolManager
{
    private static List<GameObject> activeSymbols = new List<GameObject>();

    public static void RegisterSymbol(GameObject symbol)
    {
        if (symbol != null && !activeSymbols.Contains(symbol))
        {
            activeSymbols.Add(symbol);
        }
    }

    public static void UnregisterSymbol(GameObject symbol)
    {
        if (symbol != null && activeSymbols.Contains(symbol))
        {
            activeSymbols.Remove(symbol);
        }
    }

    public static void DestroyAllSymbols()
    {
        List<GameObject> symbolsToDestroy = new List<GameObject>(activeSymbols);
        
        foreach (GameObject symbol in symbolsToDestroy)
        {
            if (symbol != null)
            {
                Object.Destroy(symbol);
            }
        }
        
        activeSymbols.Clear();
        Debug.Log($"SymbolManager: Destroyed {symbolsToDestroy.Count} dialogue symbols");
    }
}

public class DialogueSymbol : MonoBehaviour
{
    private Transform symbolTransform;
    private TMP_Text symbolText;
    private Rigidbody symbolRigidbody;
    private BoxCollider symbolCollider;

    private Dialogue3DTheme dialogue3DTheme;
    private float destroyTimer = 10f; // 10 seconds timer

    private void Awake()
    {
        symbolTransform = transform.GetChild(0).transform;
        symbolText = symbolTransform.GetComponent<TMP_Text>();
        symbolRigidbody = symbolTransform.GetComponent<Rigidbody>();
        symbolCollider = symbolTransform.GetComponent<BoxCollider>();
        
        // Register this symbol with the manager
        SymbolManager.RegisterSymbol(gameObject);
    }

    private void Update()
    {
        
    }

    private void OnDestroy()
    {
        // Unregister this symbol from the manager
        SymbolManager.UnregisterSymbol(gameObject);
    }

    public void SetSymbol(char c, Dialogue3DTheme theme)
    {
        symbolText.text = c.ToString();
        dialogue3DTheme = theme;
    }

    public void FadeInAnimation()
    {
        switch (dialogue3DTheme)
        {
            case Dialogue3DTheme.Default:
                FadeInAnimation_Default();
                break;
            case Dialogue3DTheme.Mita:
                break;
            case Dialogue3DTheme.Player:
                FadeInAnimation_Player();
                break;
            case Dialogue3DTheme.ChibiMita:
                break;
        }
    }

    public void JumpAnimation()
    {
        symbolCollider.enabled = true;
        symbolRigidbody.useGravity = true;

        switch (dialogue3DTheme)
        {
            case Dialogue3DTheme.Default:
                JumpAnimation_Default();
                break;
            case Dialogue3DTheme.Mita:
                break;
            case Dialogue3DTheme.Player:
                JumpAniamtion_Player();
                break;
            case Dialogue3DTheme.ChibiMita:
                break;
        }
    }

    public void FadeOutAnimation()
    {
        switch (dialogue3DTheme)
        {
            case Dialogue3DTheme.Default:
                FadeOutAniamtion_Default();
                break;
            case Dialogue3DTheme.Mita:
                break;
            case Dialogue3DTheme.Player:
                FadeOutAniamtion_Default();
                break;
            case Dialogue3DTheme.ChibiMita:
                break;
        }
    }

    #region FadeIn Animation
    private void FadeInAnimation_Default()
    {
        symbolText.DOFade(1f, 0.35f);
    }

    private void FadeInAnimation_Player()
    {
        symbolTransform.localScale = Vector3.one * 1.7f;
        symbolTransform.localPosition = new Vector3(Random.Range(1.2f, 1.5f), Random.Range(1.2f, 1.5f), Random.Range(-1f, -0.5f));
        symbolTransform.localEulerAngles = new Vector3(Random.Range(20f, 30f), Random.Range(-40f, -30f), Random.Range(-15f, 15f));
        symbolTransform.DOScale(Vector3.one, 0.4f).SetEase(Ease.Linear);
        symbolTransform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.Linear);
        symbolTransform.DOLocalRotate(Vector3.zero, 0.4f).SetEase(Ease.Linear).SetDelay(0.05f);
        symbolText.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
    }
    #endregion

    #region Jump Animation
    private void JumpAnimation_Default()
    {
        symbolRigidbody.AddForceAtPosition(CommonTools.GetRandomVector3(), CommonTools.GetRandomVector3(), ForceMode.Impulse);
    }

    private void JumpAniamtion_Player()
    {
        Vector3 force = transform.right * Random.Range(-0.5f, -0.25f) + transform.up * Random.Range(1.0f, 1.5f) + transform.forward * Random.Range(-0.25f, 0.25f);
        symbolRigidbody.AddForceAtPosition(force, CommonTools.GetRandomVector3(), ForceMode.Impulse);
    }
    #endregion

    #region FadeOut Animation
    private void FadeOutAniamtion_Default()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(symbolTransform.DOScale(1.2f, 0.2f));
        sequence.Append(symbolTransform.DOScale(0f, 0.8f));
    }
    #endregion

    #region Auto Destroy
    private void StartAutoDestroy()
    {
        // Destroy the GameObject after 10 seconds
        Destroy(gameObject);
    }
    #endregion
}
