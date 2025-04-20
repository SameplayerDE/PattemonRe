using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MeltySynth;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;

    private MidiPlayer midiPlayer;
    private MidiFile midiFile;

    // 94 2B 00 00 LOOP
    
    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
    }

    protected override void LoadContent()
    {
        midiPlayer = new MidiPlayer(@"C:\Users\asame\Music\Pokemon Platinum\SoundFonts\BANK_BGM_FIELD.sf2");
        midiFile = new MidiFile(@"C:\Users\asame\Music\Pokemon Platinum\SEQ_TOWN02_D.mid", MidiFileLoopType.PokemonPlatinum);
    }

    protected override void UnloadContent()
    {
        midiPlayer.Dispose();
    }

    protected override void Update(GameTime gameTime)
    {
        if (midiPlayer.State == SoundState.Stopped)
        {
            midiPlayer.Play(midiFile, true);
        }

        //Console.WriteLine(midiPlayer.Sequencer.Position);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }
}