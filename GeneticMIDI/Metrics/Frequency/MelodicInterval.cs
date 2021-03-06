﻿using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Frequency
{
    /// <summary>
    /// |pi - p_{i-1}|
    /// </summary>
    public class MelodicInterval : MetricFrequency
    {
        public override void GenerateFrequencies(Note[] notes)
        {
            for(int i = 0; i < notes.Length-1; i++)
            {
                int interval = Math.Abs(notes[i].Pitch - notes[i+1].Pitch);
                Add(new Pair(interval));
            }
        }
    }
}
