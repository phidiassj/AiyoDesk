using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiyoCoveX.Shared;

/// <summary>
/// Class to represent an audio device
/// </summary>
public class DeviceInfo
{
    public int DeviceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Channels { get; set; }

    public override string ToString()
    {
        return $"{DeviceId}: {Name} ({Channels} channels)";
    }
}
