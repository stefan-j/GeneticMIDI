﻿using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Metrics
{
    public abstract class MetricFrequency : IMetric
    {
        private Dictionary<Pair, int> frequencies;
        protected const float p = 0.02f;

        public MetricFrequency()
        {
            frequencies = new Dictionary<Pair, int>();
        }
        public Dictionary<Pair, int> Frequencies
        {
            get { return frequencies; }
        }

        public Dictionary<Pair, int> Generate(Note[] notes)
        {
            frequencies = new Dictionary<Pair, int>();
            frequencies.Clear();
            GenerateFrequencies(notes);
            Filter();
            return frequencies;
        }

        public void Filter()
        {
            int N = frequencies.Keys.Count;
            var keys = frequencies.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++ )
            {
                var k = keys[i];
                if (frequencies[k] / (float)N < p)
                {
                    frequencies.Remove(k);
                }
            }
        }

        public void Add(Pair p)
        {
            if (frequencies.ContainsKey(p))
                frequencies[p]++;
            else
                frequencies[p] = 1;
        }

        public abstract void GenerateFrequencies(Note[] notes);
    }
}
