using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] 
    private Image _progressBar;
    
    [SerializeField]
    private float _fillSpeed = 0.5f;
    
    [SerializeField]
    private Gradient _gradient;
    
    [SerializeField]
    private UnityEvent<float> _onProgressChange;
    
    [SerializeField]
    private UnityEvent _onProgressComplete;
    
    private Coroutine AnimationCoroutine;

    private void Start()
    {
        if (_progressBar.type != Image.Type.Filled)
        {
            Debug.LogError("ProgressBar precisa ser do tipo 'Filled' para funcionar corretamente.");
            this.enabled = false;
        }
    }

    public void SetProgress(float progress)
    {
        SetProgress(progress, _fillSpeed);
    }
    
    public void SetProgress(float progress, float speed)
    {
        if (progress < 0 || progress > 1)
        {
            progress = Mathf.Clamp01(progress);
        }

        if (progress != _progressBar.fillAmount)
        {
            if (AnimationCoroutine != null)
            {
                StopCoroutine(AnimationCoroutine);
            }
            
            AnimationCoroutine = StartCoroutine(AnimateProgress(progress, speed));
        }
    }
    
    private IEnumerator AnimateProgress(float progress, float speed)
    {
        float time = 0;
        float initialProgress = _progressBar.fillAmount;

        while (time < 1)
        {
            _progressBar.fillAmount = Mathf.Lerp(initialProgress, progress, time);
            
            time += Time.deltaTime * speed;
            
            _progressBar.color = _gradient.Evaluate(1 - _progressBar.fillAmount);
            
            _onProgressChange?.Invoke(_progressBar.fillAmount);
            yield return null;
        }
        
        _progressBar.fillAmount = progress;
        _progressBar.color = _gradient.Evaluate(1 - _progressBar.fillAmount);
        
        _onProgressChange?.Invoke(_progressBar.fillAmount);
        _onProgressComplete?.Invoke();
    }
}
