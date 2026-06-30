using System.IO;
using System.Media;

namespace HoroshieIgry.Core.Audio;

/// <summary>Процедурная генерация коротких WAV-тонов без внешних файлов.</summary>
public static class ProceduralSoundPlayer
{
    public static byte[] CreateTone(double frequencyHz, int durationMs, double volume = 0.15)
    {
        const int sampleRate = 22050;
        var sampleCount = sampleRate * durationMs / 1000;
        var data = new byte[sampleCount * 2];

        for (var i = 0; i < sampleCount; i++)
        {
            var t = (double)i / sampleRate;
            var envelope = 1.0 - (double)i / sampleCount;
            var sample = Math.Sin(2 * Math.PI * frequencyHz * t) * envelope * volume;
            var value = (short)(sample * short.MaxValue);
            data[i * 2] = (byte)(value & 0xFF);
            data[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        using var stream = new MemoryStream(44 + data.Length);
        using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true);

        writer.Write("RIFF"u8);
        writer.Write(36 + data.Length);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(data.Length);
        writer.Write(data);

        return stream.ToArray();
    }

    public static void Play(byte[] clip)
    {
        try
        {
            var player = new SoundPlayer(new MemoryStream(clip));
            player.Play();
        }
        catch
        {
            // ignore audio errors on devices without output
        }
    }

    public static void PlayTone(double frequencyHz, int durationMs, double volume = 0.15)
        => Play(CreateTone(frequencyHz, durationMs, volume));

    public static void PlaySequence(byte[][] clips, int gapMs = 85)
    {
        _ = Task.Run(async () =>
        {
            foreach (var clip in clips)
            {
                Play(clip);
                await Task.Delay(gapMs);
            }
        });
    }
}
