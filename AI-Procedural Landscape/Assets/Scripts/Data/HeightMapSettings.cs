using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {
    public NoiseSettings noiseSettings;

    public bool useFallOff;
    //Height modificator
    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public float minHeight {
        get {
            return  heightMultiplier * heightCurve.Evaluate(0);
        }
    }


    public float maxHeight {
        get {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }

    //If we're outside the Unity Editor there'll be no method to override
#if UNITY_EDITOR
    protected override void OnValidate() {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }
#endif
}