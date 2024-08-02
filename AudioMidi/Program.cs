using System;
using NAudio.Wave;
using MeltySynth;

namespace AudioMidi
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create the synthesizer.
            var sampleRate = 44100;
            var synthesizer = new Synthesizer("TimGM6mb.sf2", sampleRate);

// Read the MIDI file.
            var midiFile = new MidiFile("flourish.mid");
            var sequencer = new MidiFileSequencer(synthesizer);
            sequencer.Play(midiFile, false);

// The output buffer.
            var left = new float[(int)(sampleRate * midiFile.Length.TotalSeconds)];
            var right = new float[(int)(sampleRate * midiFile.Length.TotalSeconds)];

// Render the waveform.
            sequencer.Render(left, right);
        }
    }
}