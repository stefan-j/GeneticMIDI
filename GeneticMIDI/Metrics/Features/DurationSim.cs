﻿using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics.Features
{
    class DurationSim : IMetric
    {
        public Dictionary<object, float> Generate(Representation.Note[] notes)
        {
            Dictionary<object, float> dic = new Dictionary<object, float>();
            float time = 0;
            foreach(Note n in notes)
            {
                dic[time] = n.Duration;
                time += n.Duration;
            }
            return dic;
        }
    }
}
