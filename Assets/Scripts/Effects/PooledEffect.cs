using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PooledEffect : PooledPrefab<PooledEffect>
{
    [SerializeField] UnityEvent onGet, onRelease;
    [SerializeField] float duration = 5.0f;
    float counter = 0.0f;
    protected override void OnGet()
    {
        onGet.Invoke();
        counter = 0.0f;
    }
    protected override void OnRelease()
    {
        onRelease.Invoke();
    }
    private void Update()
    {
        counter += Time.deltaTime;
        if (counter >= duration && !released) Release();
    }
}