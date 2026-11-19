using UnityEngine;

public class JetVFX : MonoBehaviour
{
    [SerializeField] ParticleSystem JetEngine1;
    [SerializeField] ParticleSystem JetEngine2;

    [SerializeField] ParticleSystem JetVapor1;
    [SerializeField] ParticleSystem JetVapor2;

    [SerializeField] GameObject WingTrails;

    ParticleSystem[] engines;
    ParticleSystem[] vapors;

    float[] baseThrustLenghts;
    float[] baseAlphaValues;

    Gradient[] vaporGradients;
    GradientColorKey[][] vaporColorKeys;
    GradientAlphaKey[][] vaporAlphaKeys;

    float CurrentAlpaValue;
    float CurrentThrustLenght;

    private JetPhysics Physics;

    private void Start()
    {

        Physics = GetComponentInParent<JetPhysics>();

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
    }

    private void Update()
    {
        WingTrails.SetActive(Physics.State == JetState.Air ? true : false);

        CurrentThrustLenght = Physics.Thrust;

        CurrentAlpaValue = Mathf.Clamp01(Physics.AOA);

        Apply();
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