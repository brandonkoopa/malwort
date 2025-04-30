using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Bitcrusher : MonoBehaviour
{
    [Header("Bitcrusher Settings")]

    [Tooltip("Number of bits to represent audio amplitude (1-16). Lower values are more distorted.")]
    [Range(1, 16)]
    public int bitdepth = 8; // Default to 8-bit

    [Tooltip("Factor by which to reduce the sample rate (1 = no reduction). Higher values sound more 'grainy'.")]
    [Range(1, 50)] // Increased range for more extreme effects
    public int sampleRateReduction = 1; // Default to no reduction

    [Tooltip("Volume multiplier for the processed (wet) signal.")]
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f; // Default to full volume for the effect

    [Tooltip("Mix between original (Dry) and processed (Wet) signal. 0 = Dry, 1 = Wet.")]
    [Range(0.0f, 1.0f)]
    public float dryWet = 1.0f; // Default to fully wet (only effect is heard)

    // Internal variables for processing
    private float step; // The quantization step based on bitdepth
    private float[] lastSample; // Holds the last processed sample for sample rate reduction (per channel)
    private int currentSampleIndex = 0; // Counter for sample rate reduction

    void Awake()
    {
        // Initialize lastSample array - will be resized if needed in OnAudioFilterRead
        lastSample = new float[2]; // Assume stereo initially, will adapt
    }

    // --- Preset Function ---
    [ContextMenu("Set 16-Bit Preset")]
    void Set16BitPreset()
    {
        // Values inspired by SNES/Genesis era - often not true 16-bit/44.1kHz CD quality
        bitdepth = 10;             // Slightly reduced bit depth for character
        sampleRateReduction = 2;   // Simulate lower console sample rates (e.g., ~22-32kHz)
        volume = 0.85f;            // Slightly reduce volume to prevent clipping potentially caused by quantization
        dryWet = 1.0f;             // Fully wet for the effect
        Debug.Log("Bitcrusher: 16-Bit Preset Applied (Bitdepth: 10, Rate Reduction: 2, Volume: 0.85, Mix: 1.0)");

        // Ensure internal step value updates if the script is running
        CalculateStep();
    }

    // Calculate the quantization step based on current bitdepth
    void CalculateStep()
    {
        step = Mathf.Pow(2, bitdepth);
    }

    // This function is called by Unity whenever the AudioSource needs audio data
    void OnAudioFilterRead(float[] data, int channels)
    {
        // Recalculate step value if bitdepth changed
        // (Optimization: could check if bitdepth actually changed, but calculation is cheap)
        CalculateStep();

        // Ensure the lastSample array matches the number of channels
        if (lastSample == null || lastSample.Length != channels)
        {
            lastSample = new float[channels];
        }

        // Process each sample in the buffer
        for (int i = 0; i < data.Length; i += channels)
        {
            // --- Sample Rate Reduction ---
            currentSampleIndex++;
            if (currentSampleIndex >= sampleRateReduction)
            {
                currentSampleIndex = 0; // Reset counter

                // This is a sample we *process*, store its value for potential holding later
                for (int j = 0; j < channels; j++)
                {
                    // Store the current sample value *before* any processing
                    // This will become the value held during the next reduction phase
                    lastSample[j] = data[i + j];
                }
            }
            else
            {
                 // Hold the *previously stored* sample value for this reduction phase
                for (int j = 0; j < channels; j++)
                {
                    data[i + j] = lastSample[j];
                }
            }


            // --- Process each channel for the current sample index (either new or held) ---
            for (int j = 0; j < channels; j++)
            {
                float drySample = data[i + j]; // Store original sample before processing for Dry/Wet mix

                // Use the value determined by sample rate reduction (either current or held)
                float sampleToProcess = lastSample[j]; // If we reset index, this is the current sample. If holding, it's the previous.

                // --- Bit Depth Reduction (Quantization) & Volume ---
                // Apply quantization to the selected sample
                float wetSample = Mathf.Round(sampleToProcess * step) / step;

                // Apply volume control to the wet signal
                wetSample *= volume;

                // --- Dry/Wet Mix ---
                // Blend the original dry signal with the processed wet signal
                data[i + j] = drySample * (1.0f - dryWet) + wetSample * dryWet;
            }
        }
    }

    // Optional: Update step calculation in editor if values change while not playing
    #if UNITY_EDITOR
    void OnValidate()
    {
        CalculateStep();
    }
    #endif
}