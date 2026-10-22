using UnityEngine;

public class JetSoundLogic : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] AudioSource EngineThrust;

    [SerializeField] AudioSource AfterBurnerThrust;

    [SerializeField] AudioSource FrameAerodynamics;

    [Header("SFXs")]

    [SerializeField] AudioClip EngineThrustLow;

    [SerializeField] AudioClip AfterBurner;

    [Header("References")]
    [SerializeField] JetData data;

    [Header("Settings")]
    [SerializeField] float MaxEngineThrustValue;


    [SerializeField] float EngineBasePitch;

    [SerializeField] float MonuverPitchEffectivness;

    [SerializeField] float MonuverRollEffectivness;


    [SerializeField] float InterpolationSpeed;


    private void Awake()
    {
        EngineBasePitch = EngineThrust.pitch;
    }


    private void Update()
    {
        EngineThrust.volume = data.GetThrust*MaxEngineThrustValue;
        
        //EngineThrust.pitch = Mathf.Lerp(EngineThrust.pitch, EngineBasePitch +)

    }
}