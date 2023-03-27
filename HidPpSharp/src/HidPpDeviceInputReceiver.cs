using HidSharp;
using log4net;

namespace HidPpSharp;

public class HidPpDeviceInputReceiver : IDisposable {
    private readonly byte[]           _readBuffer;
    private readonly object           _syncRoot;
    private readonly ManualResetEvent _waitHandle;

    private readonly ILog       Log;
    private          byte[]     _buffer;
    private          int        _bufferCount;
    private          int        _bufferOffset;
    private volatile bool       _running;
    private          HidStream? _stream;

    public HidPpDeviceInputReceiver(IHidPpDevice device) {
        Device        = device;
        Log           = LogManager.GetLogger($"{device.VendorId:X4}:{device.ProductId:X4}:{device.DeviceIndex:X2}:IR");
        _buffer       = new byte[2048];
        _readBuffer   = new byte[2048];
        _bufferOffset = 0;
        _bufferCount  = 0;
        _running      = false;
        _syncRoot     = new object();
        _waitHandle   = new ManualResetEvent(true);
    }

    public bool IsRunning {
        get => _running;
    }

    public WaitHandle WaitHandle {
        get => _waitHandle;
    }

    public IHidPpDevice Device { get; }

    public void Dispose() {
        Stop();
        _stream?.Dispose();
        _waitHandle.Dispose();
        Device.Dispose();
    }

    public event EventHandler Started;
    public event EventHandler Received;
    public event EventHandler Stopped;

    public void Start() {
        lock (_syncRoot) {
            _stream = Device.Stream;
            if (_stream == null) {
                throw new InvalidOperationException("Device is not open");
            }

            _stream.ReadTimeout = -1;
            _running = !_running ? true : throw new InvalidOperationException("The receiver is already running.");
            BeginRead();
            _waitHandle.Reset();
        }

        Started?.Invoke(this, EventArgs.Empty);
    }

    private void BeginRead() {
        try {
            _stream!.BeginRead(_readBuffer, 0, _readBuffer.Length, EndRead, null);
        }
        catch {
            Stop();
        }
    }

    private void EndRead(IAsyncResult ar) {
        int count;
        try {
            count = _stream!.EndRead(ar);
        }
        catch {
            Stop();
            return;
        }

        if (count == 0) {
            Stop();
        } else {
            ProvideReceivedData(_readBuffer, 0, count);
            BeginRead();
        }
    }

    private void Stop() {
        lock (_syncRoot) {
            _running = false;
            _stream  = null;
            _waitHandle.Set();
        }

        Stopped?.Invoke(this, EventArgs.Empty);
    }

    private void ClearReceivedData() {
        lock (_syncRoot) {
            _bufferOffset = 0;
            _bufferCount  = 0;
            _waitHandle.Reset();
        }
    }

    private void ProvideReceivedData(byte[] buffer, int offset, int count) {
        Log.DebugFormat("received: {0}", BitConverter.ToString(buffer, offset, count));
        lock (_syncRoot) {
            var num    = checked(_bufferCount + count);
            var length = _buffer.Length;

            if (length < num) {
                while (length < num) {
                    checked {
                        length *= 2;
                    }
                }

                Array.Resize(ref _buffer, length);
            }

            Buffer.BlockCopy(buffer, offset, _buffer, _bufferCount, count);
            _bufferCount += count;
            _waitHandle.Set();
        }

        Received?.Invoke(this, EventArgs.Empty);
    }

    public bool TryRead(out IHidPpReport? report) {
        return TryRead(new byte[512], 0, out report);
    }

    public bool TryRead(byte[] buffer, int offset, out IHidPpReport? report) {
        lock (_syncRoot) {
            report = null;
            if (!_running) {
                return false;
            }

            if (_bufferOffset >= _bufferCount) {
                _waitHandle.Reset();
                return false;
            }

            var length = HidPpReport.GetSize(_buffer[_bufferOffset]);
            if (length == 0) {
                ClearReceivedData();
                return false;
            }

            var num = _bufferOffset + length;
            if (num > _bufferCount) {
                _waitHandle.Reset();
                return false;
            }

            Buffer.BlockCopy(_buffer, _bufferOffset, buffer, offset, length);

            if (!HidPpReport.TryParse(buffer[offset..(offset + length)], out report)) {
                ClearReceivedData();
                return false;
            }

            if (num == _bufferCount) {
                ClearReceivedData();
            } else {
                _bufferOffset = num;
            }

            return true;
        }
    }
}