using NAudio.Wave;
using NAudio.Dsp; // Cần thiết cho Complex và FastFourierTransform
using System;
using System.Diagnostics;

namespace Spotifree.Services
{
    public class FrequencySpectrumProvider : ISampleProvider, IDisposable
    {
        private readonly ISampleProvider _source;
        private readonly float[] _fftBuffer;
        private readonly Complex[] _fftComplexBuffer;
        private readonly float[] _fftInputBuffer;
        private int _fftInputBufferPosition;

        private const int FftSize = 64;
        private const int NumberOfBands = 32;

        public event Action<float[]>? FrequencyDataAvailable;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public FrequencySpectrumProvider(ISampleProvider source)
        {
            _source = source;
            _fftBuffer = new float[NumberOfBands];
            _fftComplexBuffer = new Complex[FftSize];
            _fftInputBuffer = new float[FftSize];
            _fftInputBufferPosition = 0;
        }

        private void CalculateFft()
        {
            for (int i = 0; i < FftSize; i++)
            {
                _fftComplexBuffer[i].X = _fftInputBuffer[i] * (float)FastFourierTransform.HammingWindow(i, FftSize);
                _fftComplexBuffer[i].Y = 0; // Phần ảo = 0
            }

            FastFourierTransform.FFT(true, (int)Math.Log(FftSize, 2.0), _fftComplexBuffer);

            for (int i = 0; i < NumberOfBands; i++)
            {
                double magnitude = Math.Sqrt(_fftComplexBuffer[i].X * _fftComplexBuffer[i].X + _fftComplexBuffer[i].Y * _fftComplexBuffer[i].Y);

                double db = 20 * Math.Log10(magnitude + 0.000001);

                // Chuẩn hóa giá trị từ 0.0 đến 1.0
                double minDb = -60;
                double maxDb = 0;
                double normalizedValue = (db - minDb) / (maxDb - minDb);

                _fftBuffer[i] = (float)Math.Max(0, Math.Min(1.0, normalizedValue));
            }

            FrequencyDataAvailable?.Invoke(_fftBuffer);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                _fftInputBuffer[_fftInputBufferPosition++] = buffer[offset + i];

                if (_fftInputBufferPosition >= FftSize)
                {
                    _fftInputBufferPosition = 0;
                    CalculateFft();
                }
            }

            return samplesRead;
        }

        public void Dispose()
        {
        }
    }
}