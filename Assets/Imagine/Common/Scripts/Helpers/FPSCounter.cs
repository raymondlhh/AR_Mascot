using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This script calculate the current fps and show it to a text ui.
/// </summary>

namespace Imagine.WebAR.Samples
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fpsText;

        public float updateRateSeconds = 3.0f;

        int frameCount = 0;
        float elapsedTime = 0f;
        float fps = 0f;

        void Update()
        {
            frameCount++;
            elapsedTime += Time.unscaledDeltaTime;
            if (elapsedTime >= updateRateSeconds)
            {
                fps = 1 / (elapsedTime / frameCount);
                frameCount = 0;
                elapsedTime = 0;
                fpsText.text = System.Math.Round(fps, 1).ToString("0.0");
            }
        }
    }
}
