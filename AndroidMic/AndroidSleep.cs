using EspSpectrum.Core.Recording;
using System;
using System.Collections.Generic;
using System.Text;

namespace AndroidMic;

public class AndroidSleep : IPreciseSleep
{
    public void Wait(TimeSpan waitFor, CancellationToken cancellationToken)
    {
        Task.Delay(waitFor, cancellationToken).Wait(cancellationToken);
    }
}
