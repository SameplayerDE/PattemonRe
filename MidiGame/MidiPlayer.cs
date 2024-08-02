using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;
using MeltySynth;
using Microsoft.Xna.Framework.Media;

public class MidiPlayer : IDisposable
{
    private static readonly int sampleRate = 44100;
    private static readonly int bufferLength = sampleRate / 10;

    private Synthesizer synthesizer;
    public MidiFileSequencer Sequencer;
    
    private DynamicSoundEffectInstance dynamicSound;
    private byte[] buffer;

    public MidiPlayer(string soundFontPath)
    {
        synthesizer = new Synthesizer(soundFontPath, sampleRate);
        Sequencer = new MidiFileSequencer(synthesizer);

        dynamicSound = new DynamicSoundEffectInstance(sampleRate, AudioChannels.Stereo);
        buffer = new byte[4 * bufferLength];

        dynamicSound.BufferNeeded += (s, e) => SubmitBuffer();
    }

    public void Play(MidiFile midiFile, bool loop)
    {
        Sequencer.Play(midiFile, loop);

        if (dynamicSound.State != SoundState.Playing)
        {
            SubmitBuffer();
            dynamicSound.Play();
        }
    }

    public void Stop()
    {
        Sequencer.Stop();
    }

    private void SubmitBuffer()
    {
        Sequencer.RenderInterleavedInt16(MemoryMarshal.Cast<byte, short>(buffer));
        dynamicSound.SubmitBuffer(buffer, 0, buffer.Length);
    }

    public void Dispose()
    {
        if (dynamicSound != null)
        {
            dynamicSound.Dispose();
            dynamicSound = null;
        }
    }

    public SoundState State => dynamicSound.State;
}