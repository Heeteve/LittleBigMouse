using System;

namespace LittleBigMouse.Ui.Avalonia.Remote;

public interface ISessionLockService : IDisposable
{
    event EventHandler<bool>? SessionLockChanged;
    bool IsSessionLocked { get; }
    void StartMonitoring();
    void StopMonitoring();
}