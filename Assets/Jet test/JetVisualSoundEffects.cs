using UnityEngine;

public class JetVisualSoundEffects : MonoBehaviour
{
    [Header("AudioSources")]
    [SerializeField] AudioSource MainSource;

    [SerializeField] AudioSource SecondarySource;

    [Header("Audio Clips")]
    [SerializeField] AudioClip LowThrustSFX1;
    [SerializeField] AudioClip LowThrustSFX2;

    [SerializeField] AudioClip HighGTurnSFX;

    [SerializeField] AudioClip AfterBurnerSFX;

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
