/*
  LittleBigMouse.Screen.Config
  Copyright (c) 2021 Mathieu GRENET.  All rights reserved.

  This file is part of LittleBigMouse.Screen.Config.

    LittleBigMouse.Screen.Config is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LittleBigMouse.Screen.Config is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/

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