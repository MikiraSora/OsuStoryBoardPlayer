﻿using System;

namespace ReOsuStoryboardPlayer.Core.Serialization.FileInfo
{
    [Flags]
    public enum Feature : byte
    {
        //IsCompression=1, //FUCK 'System.IO.InvalidDataException' : "Block length does not match with its complement.
        ContainStatistics = 2
    }
}