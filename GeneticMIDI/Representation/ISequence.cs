﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Representation
{
    public interface ISequence
    {
        int Duration { get; }

        int Length { get; }

        PlaybackInfo GeneratePlaybackInfo(byte channel, int time = 0);
    }
}
