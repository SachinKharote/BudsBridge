using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using UniversalTWSSync.App.Models;

namespace UniversalTWSSync.App.Services
{
    public sealed class WasapiAudioSyncService : IAudioSyncService, IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly MMDeviceEnumerator _enumerator;

        private WasapiLoopbackCapture _capture;
        private WasapiOut _leftOutput;
        private WasapiOut _rightOutput;
        private BufferedWaveProvider _leftBuffer;
        private BufferedWaveProvider _rightBuffer;
        private AudioDeviceDescriptor _activeLeftDevice;
        private AudioDeviceDescriptor _activeRightDevice;
        private int _activeLeftDelayMs;
        private int _activeRightDelayMs;
        private bool _isRunning;

        public WasapiAudioSyncService()
        {
            _enumerator = new MMDeviceEnumerator();
        }

        public SyncSessionResult Connect(AudioDeviceDescriptor leftDevice, AudioDeviceDescriptor rightDevice)
        {
            var validation = ValidateDevices(leftDevice, rightDevice);
            if (!validation.Success)
            {
                return validation;
            }

            try
            {
                using (var left = _enumerator.GetDevice(leftDevice.DeviceId))
                using (var right = _enumerator.GetDevice(rightDevice.DeviceId))
                {
                    return new SyncSessionResult
                    {
                        Success = true,
                        Title = "Both devices connected",
                        Details = "Endpoints are available for a real WASAPI sync session.",
                        Quality = "Ready"
                    };
                }
            }
            catch (Exception ex)
            {
                return CreateFailure("Unable to open selected devices.", ex.Message);
            }
        }

        public SyncSessionResult Start(AudioDeviceDescriptor leftDevice, AudioDeviceDescriptor rightDevice, int leftDelayMs, int rightDelayMs, bool autoSyncEnabled)
        {
            var validation = Connect(leftDevice, rightDevice);
            if (!validation.Success)
            {
                return validation;
            }

            lock (_syncRoot)
            {
                try
                {
                    StopInternal();

                    _activeLeftDevice = leftDevice;
                    _activeRightDevice = rightDevice;
                    _activeLeftDelayMs = leftDelayMs;
                    _activeRightDelayMs = rightDelayMs;

                    var captureDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    _capture = new WasapiLoopbackCapture(captureDevice);

                    var waveFormat = _capture.WaveFormat;
                    _leftBuffer = CreateBufferedProvider(waveFormat);
                    _rightBuffer = CreateBufferedProvider(waveFormat);

                    PrimeDelay(_leftBuffer, waveFormat, leftDelayMs);
                    PrimeDelay(_rightBuffer, waveFormat, rightDelayMs);

                    _leftOutput = new WasapiOut(_enumerator.GetDevice(leftDevice.DeviceId), AudioClientShareMode.Shared, false, 100);
                    _rightOutput = new WasapiOut(_enumerator.GetDevice(rightDevice.DeviceId), AudioClientShareMode.Shared, false, 100);

                    _leftOutput.Init(_leftBuffer);
                    _rightOutput.Init(_rightBuffer);

                    _capture.DataAvailable += CaptureOnDataAvailable;

                    _leftOutput.Play();
                    _rightOutput.Play();
                    _capture.StartRecording();

                    _isRunning = true;

                    return new SyncSessionResult
                    {
                        Success = true,
                        Title = "Sync session active",
                        Details = "Live system audio loopback is now feeding both selected devices.",
                        Quality = EvaluateQuality(leftDelayMs, rightDelayMs, autoSyncEnabled)
                    };
                }
                catch (Exception ex)
                {
                    StopInternal();
                    return CreateFailure("Unable to start real-time audio sync.", ex.Message);
                }
            }
        }

        public SyncSessionResult Stop()
        {
            lock (_syncRoot)
            {
                StopInternal();
            }

            return new SyncSessionResult
            {
                Success = true,
                Title = "Sync stopped",
                Details = "The live WASAPI session has been stopped.",
                Quality = "Idle"
            };
        }

        public SyncSessionResult ApplyCalibration(AudioDeviceDescriptor leftDevice, AudioDeviceDescriptor rightDevice, int leftDelayMs, int rightDelayMs, bool autoSyncEnabled)
        {
            _activeLeftDelayMs = leftDelayMs;
            _activeRightDelayMs = rightDelayMs;

            if (_isRunning && _activeLeftDevice != null && _activeRightDevice != null)
            {
                var restartResult = Start(_activeLeftDevice, _activeRightDevice, leftDelayMs, rightDelayMs, autoSyncEnabled);
                restartResult.Title = restartResult.Success ? "Calibration updated" : restartResult.Title;
                restartResult.Details = restartResult.Success
                    ? "The live session was restarted with the new delay offsets."
                    : restartResult.Details;
                return restartResult;
            }

            return new SyncSessionResult
            {
                Success = true,
                Title = "Calibration ready",
                Details = "New delay offsets will be used when sync starts.",
                Quality = EvaluateQuality(leftDelayMs, rightDelayMs, autoSyncEnabled)
            };
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                StopInternal();
                _enumerator.Dispose();
            }
        }

        private static BufferedWaveProvider CreateBufferedProvider(WaveFormat waveFormat)
        {
            var provider = new BufferedWaveProvider(waveFormat);
            provider.BufferDuration = TimeSpan.FromSeconds(8);
            provider.DiscardOnBufferOverflow = true;
            provider.ReadFully = true;
            return provider;
        }

        private void CaptureOnDataAvailable(object sender, WaveInEventArgs e)
        {
            var monoBytes = ConvertToMonoStereo(e.Buffer, e.BytesRecorded, _capture.WaveFormat);
            lock (_syncRoot)
            {
                if (_leftBuffer != null)
                {
                    _leftBuffer.AddSamples(monoBytes, 0, monoBytes.Length);
                }

                if (_rightBuffer != null)
                {
                    _rightBuffer.AddSamples(monoBytes, 0, monoBytes.Length);
                }
            }
        }

        private static byte[] ConvertToMonoStereo(byte[] inputBuffer, int bytesRecorded, WaveFormat waveFormat)
        {
            var output = new byte[bytesRecorded];

            if (waveFormat.Channels != 2)
            {
                Buffer.BlockCopy(inputBuffer, 0, output, 0, bytesRecorded);
                return output;
            }

            if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat && waveFormat.BitsPerSample == 32)
            {
                var input = new WaveBuffer(inputBuffer);
                var mono = new WaveBuffer(output);
                var frames = bytesRecorded / waveFormat.BlockAlign;

                for (var frame = 0; frame < frames; frame++)
                {
                    var sampleIndex = frame * 2;
                    var mixed = (input.FloatBuffer[sampleIndex] + input.FloatBuffer[sampleIndex + 1]) * 0.5f;
                    mono.FloatBuffer[sampleIndex] = mixed;
                    mono.FloatBuffer[sampleIndex + 1] = mixed;
                }

                return output;
            }

            if (waveFormat.Encoding == WaveFormatEncoding.Pcm && waveFormat.BitsPerSample == 16)
            {
                var input = new WaveBuffer(inputBuffer);
                var mono = new WaveBuffer(output);
                var frames = bytesRecorded / waveFormat.BlockAlign;

                for (var frame = 0; frame < frames; frame++)
                {
                    var sampleIndex = frame * 2;
                    var left = input.ShortBuffer[sampleIndex];
                    var right = input.ShortBuffer[sampleIndex + 1];
                    var mixed = (short)((left + right) / 2);
                    mono.ShortBuffer[sampleIndex] = mixed;
                    mono.ShortBuffer[sampleIndex + 1] = mixed;
                }

                return output;
            }

            Buffer.BlockCopy(inputBuffer, 0, output, 0, bytesRecorded);
            return output;
        }

        private static void PrimeDelay(BufferedWaveProvider provider, WaveFormat waveFormat, int delayMs)
        {
            if (delayMs <= 0)
            {
                return;
            }

            var bytes = (int)((long)waveFormat.AverageBytesPerSecond * delayMs / 1000L);
            bytes -= bytes % waveFormat.BlockAlign;

            if (bytes <= 0)
            {
                return;
            }

            provider.AddSamples(new byte[bytes], 0, bytes);
        }

        private static SyncSessionResult ValidateDevices(AudioDeviceDescriptor leftDevice, AudioDeviceDescriptor rightDevice)
        {
            if (leftDevice == null || rightDevice == null)
            {
                return CreateFailure("Select both devices before connecting.", "Choose a device for both the left and right slots.");
            }

            if (string.Equals(leftDevice.DeviceId, rightDevice.DeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return CreateFailure("Choose two different devices.", "The same output endpoint cannot be used for both sides.");
            }

            return new SyncSessionResult
            {
                Success = true
            };
        }

        private static SyncSessionResult CreateFailure(string title, string details)
        {
            return new SyncSessionResult
            {
                Success = false,
                Title = title,
                Details = details,
                Quality = "Error"
            };
        }

        private static string EvaluateQuality(int leftDelayMs, int rightDelayMs, bool autoSyncEnabled)
        {
            var mismatch = Math.Abs(leftDelayMs - rightDelayMs);

            if (autoSyncEnabled)
            {
                mismatch = Math.Max(0, mismatch - 10);
            }

            if (mismatch <= 10)
            {
                return "Live";
            }

            if (mismatch <= 25)
            {
                return "Good";
            }

            if (mismatch <= 45)
            {
                return "Moderate";
            }

            return "Needs calibration";
        }

        private void StopInternal()
        {
            _isRunning = false;

            if (_capture != null)
            {
                _capture.DataAvailable -= CaptureOnDataAvailable;
                try
                {
                    _capture.StopRecording();
                }
                catch
                {
                }

                _capture.Dispose();
                _capture = null;
            }

            DisposeOutput(ref _leftOutput);
            DisposeOutput(ref _rightOutput);

            _leftBuffer = null;
            _rightBuffer = null;
        }

        private static void DisposeOutput(ref WasapiOut output)
        {
            if (output == null)
            {
                return;
            }

            try
            {
                output.Stop();
            }
            catch
            {
            }

            output.Dispose();
            output = null;
        }
    }
}
