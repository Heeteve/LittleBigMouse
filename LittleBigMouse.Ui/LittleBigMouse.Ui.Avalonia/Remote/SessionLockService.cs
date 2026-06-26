using System;
using System.Threading;
using Microsoft.Win32;

namespace LittleBigMouse.Ui.Avalonia.Remote;

public class SessionLockService : ISessionLockService
{
    private bool _isSessionLocked;
    private bool _isMonitoring;
    private readonly object _lockObject = new();

    public event EventHandler<bool>? SessionLockChanged;

    public bool IsSessionLocked
    {
        get
        {
            lock (_lockObject)
            {
                return _isSessionLocked;
            }
        }
    }

    public void StartMonitoring()
    {
        lock (_lockObject)
        {
            if (_isMonitoring) return;
            _isMonitoring = true;
            SystemEvents.SessionSwitch += OnSessionSwitch;
        }
    }

    public void StopMonitoring()
    {
        lock (_lockObject)
        {
            if (!_isMonitoring) return;
            _isMonitoring = false;
            SystemEvents.SessionSwitch -= OnSessionSwitch;
        }
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        bool locked;

        switch (e.Reason)
        {
            case SessionSwitchReason.SessionLock:
                locked = true;
                break;
            case SessionSwitchReason.SessionUnlock:
                locked = false;
                break;
            default:
                return;
        }

        lock (_lockObject)
        {
            if (_isSessionLocked == locked) return;
            _isSessionLocked = locked;
        }

        SessionLockChanged?.Invoke(this, locked);
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}