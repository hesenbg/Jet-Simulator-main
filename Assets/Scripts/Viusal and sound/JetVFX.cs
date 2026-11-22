using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetVFX : MonoBehaviour
{
    [SerializeField] ParticleSystem JetEngine1;
    [SerializeField] ParticleSystem JetEngine2;

    [SerializeField] ParticleSystem JetVapor1;
    [SerializeField] ParticleSystem JetVapor2;

    [SerializeField] GameObject Flare;

    [SerializeField] GameObject WingTrails;

    [SerializeField] GameObject[] Flares;

    [SerializeField] float thrustSmoothing = 5f;
    [SerializeField] float alphaSmoothing = 5f;

    ParticleSystem[] engines;
    ParticleSystem[] vapors;

    float[] baseThrustLenghts;
    float[] baseAlphaValues;

    Gradient[] vaporGradients;
    GradientColorKey[][] vaporColorKeys;
    GradientAlphaKey[][] vaporAlphaKeys;

    float CurrentAlpaValue;
    float CurrentThrustLenght;

    FlareState previousFlareState;

    [SerializeField] JetData data;

    [SerializeField] CombatSystem CombatSystem;

    private void Start()
    {
        engines = new[] { JetEngine1, JetEngine2 };
        vapors = new[] { JetVapor1, JetVapor2 };

        baseThrustLenghts = new float[engines.Length];
        for (int i = 0; i < engines.Length; i++)
        {
            baseThrustLenghts[i] = engines[i].main.startLifetime.constant;
        }

        baseAlphaValues = new float[vapors.Length];
        vaporGradients = new Gradient[vapors.Length];
        vaporColorKeys = new GradientColorKey[vapors.Length][];
        vaporAlphaKeys = new GradientAlphaKey[vapors.Length][];

        for (int i = 0; i < vapors.Length; i++)
        {
            Gradient gradient = vapors[i].colorOverLifetime.color.gradient;

            vaporGradients[i] = gradient;
            vaporColorKeys[i] = gradient.colorKeys;
            vaporAlphaKeys[i] = gradient.alphaKeys;
            baseAlphaValues[i] = vaporAlphaKeys[i][0].alpha;
        }

        CurrentThrustLenght = data.GetThrust;
        CurrentAlpaValue = Mathf.Clamp01(data.GetAOA);
    }

    private void Update()
    {
        WingTrails.SetActive(data.CurrentJetState == JetState.Air ? true : false);

        float targetThrust = data.GetThrust;
        float targetAlpha = Mathf.Clamp01(data.GetAOA);

        CurrentThrustLenght = Mathf.Lerp(CurrentThrustLenght, targetThrust, 1f - Mathf.Exp(-thrustSmoothing * Time.deltaTime));
        CurrentAlpaValue = Mathf.Lerp(CurrentAlpaValue, targetAlpha, 1f - Mathf.Exp(-alphaSmoothing * Time.deltaTime));

        Apply();

        UpdateFlares();
    }

    private void UpdateFlares()
    {
        if (CombatSystem.Object == null || !CombatSystem.Object.IsValid)
            return;

        FlareState state = CombatSystem.CurrentFlareState;

        if (state == previousFlareState)
            return;

        previousFlareState = state;

        if (state == FlareState.Using)
        {
            StartCoroutine(SpawnFlares());
        }
    }

    private IEnumerator SpawnFlares()
    {
        for (int i = 0; i < CombatSystem.CombatData.NumberOfBurst; i++)
        {
            foreach (GameObject point in Flares)
            {
                GameObject flare = Instantiate(Flare, point.transform.position, point.transform.rotation);
                Destroy(flare, CombatSystem.GetFlareDuration);
            }

            yield return new WaitForSeconds(CombatSystem.CombatData.TimeBetweenFlareBursts);
        }
    }

    private void Apply()
    {
        for (int i = 0; i < engines.Length; i++)
        {
            ParticleSystem.MainModule main = engines[i].main;
            main.startLifetime = baseThrustLenghts[i] * CurrentThrustLenght;
        }

        for (int i = 0; i < vapors.Length; i++)
        {
            vaporAlphaKeys[i][0].alpha = baseAlphaValues[i] * CurrentAlpaValue;
            vaporGradients[i].SetKeys(vaporColorKeys[i], vaporAlphaKeys[i]);

            ParticleSystem.ColorOverLifetimeModule colorModule = vapors[i].colorOverLifetime;
            colorModule.color = new ParticleSystem.MinMaxGradient(vaporGradients[i]);
        }
    }
}