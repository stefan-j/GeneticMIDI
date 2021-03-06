﻿using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators
{
    public interface IPlaybackGenerator
    {
        IPlayable Generate();

        IPlayable Next();

        bool HasNext { get; }
    }
}
