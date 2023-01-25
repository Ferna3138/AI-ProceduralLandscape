using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlendType { polynomial, sMinCubic, SquaredMin};
[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {
    public NoiseSettings[] noiseLayers;
    public BlendType blendType;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public float minHeight {
        get {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }


    public float maxHeight {
        get {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }


    protected override void OnValidate() {
        foreach (NoiseSettings settings in noiseLayers) {
            settings.ValidateValues();
            base.OnValidate();
        } 
    }

}
