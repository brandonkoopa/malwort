using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BitCrusherEffect : MonoBehaviour // Removed IAudioEffectPlugin as it's not standard Unity and likely from an asset/custom setup.
                                            // If you need IAudioEffectPlugin, add it back and implement its methods if necessary.
{
    [Header("Bit Crusher Settings")]

    [Tooltip("Target bit depth. Lower values increase quantization noise. 12 is a starting point for SNES-like feel.")]
    [Range(1, 16)]
    public int bitDepth = 12; // DEFAULT: Aiming for SNES-like quality (less than perfect 16-bit due to console limitations/compression)

    [Tooltip("Reduces effective sample rate. 1.0 = no reduction. 0.5 = half rate. ~0.66 aims for SNES-like ~32kHz from 48kHz source.")]
    [Range(0.01f, 1f)]
    public float sampleRateReductionFactor = 0.66f; // DEFAULT: Aiming for ~32kHz (SNES) from a 48kHz source rate. Adjust if your source rate differs.

    private float stepSizeReciprocal; // Pre-calculate 1.0f / stepSize for efficiency
    private float lastSampleValueL;   // Store last sample per channel if needed
    private float lastSampleValueR;   // Store last sample per channel if needed
    private int downsampleCounter;
    private int downsampleInterval;

    // We don't need SetSampleRate or originalSampleRate if we calculate based on AudioSettings.outputSampleRate
    // The IAudioEffectPlugin interface methods are removed unless you specifically need them for your plugin framework.

    void Awake()
    {
        // It's often better to calculate based on the actual output sample rate
        UpdateDownsampleInterval();
        UpdateStepSize();
    }

    void OnValidate()
    {
        // Update calculations when values are changed in the inspector
        UpdateDownsampleInterval();
        UpdateStepSize();
    }

    void UpdateDownsampleInterval()
    {
        // Calculate how many samples to skip for the desired reduction factor
        // Ensure sampleRateReductionFactor is not zero to avoid division by zero
        if (sampleRateReductionFactor > 0.0001f)
        {
            downsampleInterval = Mathf.Max(1, Mathf.RoundToInt(1.0f / sampleRateReductionFactor));
        }
        else
        {
            downsampleInterval = int.MaxValue; // Effectively disable downsampling if factor is near zero
        }
        downsampleCounter = 0; // Reset counter when interval changes
    }

    void UpdateStepSize()
    {
        // Calculate the reciprocal of the quantization step size based on bit depth
        // If bitDepth is 16 or higher, we effectively bypass the quantization
        if (bitDepth < 16)
        {
            // Calculate the number of steps: 2^(bitDepth - 1) because audio is typically -1 to 1
            // Or simply 2^bitDepth if treating the range 0 to 1 after shifting? Let's stick to the original logic's apparent intent.
            // Original logic: stepSize = 1 << (bitDepth - 1); floatSample /= stepSize; -> This scales based on powers of 2 related to integer representation.
            // A common way for float audio (-1 to 1):
            float numSteps = Mathf.Pow(2, bitDepth);
            stepSizeReciprocal = 1.0f / numSteps; // This interpretation quantizes the -1 to 1 range into 2^bitDepth levels
                                                  // Using pre-calculated reciprocal is slightly faster in the loop
        }
        else
        {
            stepSizeReciprocal = 0; // Indicates no quantization needed
        }
    }


    void OnAudioFilterRead(float[] data, int channels)
    {
        bool applyDownsampling = downsampleInterval > 1;
        bool applyQuantization = bitDepth < 16 && stepSizeReciprocal > 0; // Check if quantization should be applied

        if (!applyDownsampling && !applyQuantization)
        {
            return; // Nothing to do
        }

        for (int i = 0; i < data.Length; i += channels)
        {
            // --- Sample Rate Reduction (Downsampling) ---
            if (applyDownsampling)
            {
                if (downsampleCounter % downsampleInterval == 0)
                {
                    // This is a sample we process, update lastSampleValue for relevant channels
                    lastSampleValueL = data[i]; // Store Left (or Mono) channel value
                    if (channels > 1)
                    {
                        lastSampleValueR = data[i + 1]; // Store Right channel value if stereo
                    }
                    // Reset counter relative to interval AFTER processing this sample
                    downsampleCounter = 1; // Start count for next interval
                }
                else
                {
                    // This is a sample we skip (hold previous value)
                    data[i] = lastSampleValueL; // Hold Left (or Mono)
                    if (channels > 1)
                    {
                        data[i + 1] = lastSampleValueR; // Hold Right
                    }
                    downsampleCounter++;
                    // Continue to next frame ONLY if we are not applying quantization to the held sample
                    // If we ARE quantizing, we should quantize the held sample too.
                    if (!applyQuantization) continue;
                }
            }

            // --- Bit Depth Reduction (Quantization) ---
            // This part runs for samples that were just captured (downsampleCounter == 1 or downsampling is off)
            // OR for held samples if quantization is enabled.
            if (applyQuantization)
            {
                // Process Left (or Mono) channel
                float currentSampleL = data[i];
                // Quantize: Scale up, round, scale down
                // This method quantizes the -1 to 1 range
                data[i] = Mathf.Round(currentSampleL / stepSizeReciprocal) * stepSizeReciprocal;


                // Process Right channel if stereo
                if (channels > 1)
                {
                    float currentSampleR = data[i + 1];
                    data[i + 1] = Mathf.Round(currentSampleR / stepSizeReciprocal) * stepSizeReciprocal;
                }

                 // Update lastSampleValue AFTER quantization if downsampling is also active,
                 // so the held value is the quantized one.
                 // (This logic might need refinement based on exact desired interaction)
                if(applyDownsampling && downsampleCounter == 1) // If this was the sample we just captured
                {
                   lastSampleValueL = data[i];
                   if(channels > 1) lastSampleValueR = data[i+1];
                }
            }
        }
    }
}