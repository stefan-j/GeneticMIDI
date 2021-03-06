﻿using AForge;
using AForge.Genetic;
using GeneticMIDI.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI
{
    public class NoteGene : IGPGene 
    {
        public enum FunctionTypes
        {
            Concatenation,
            PitchShift,
            DurationShift,
            Swap,
            Repeat
        }
        public const int FUNCTIONS_NUM = 4;
        public FunctionTypes Function{get; set;}
        public int Pitch { get; private set; }
        public int Octave { get; private set; }

        public int FuncArg { get; private set; }
        public Durations Duration { get; private set; }

        protected static ThreadSafeRandom rand = new ThreadSafeRandom();

        public static Durations[] AllowedDurations = null;
        public static int[] AllowedFullPitches = null;

        public NoteGene(int pitch, Durations duration, int octave, FunctionTypes function, GPGeneType type)
        {
            this.Function = function;
            this.GeneType = type;
            this.Octave = octave;
            this.Pitch = pitch;
            this.Duration = duration;

        }

        public NoteGene(GPGeneType type)
        {
            
            Generate(type);
        }

        public static Durations GetClosestDuration(int dur)
        {
   
            var dur_arr = Enum.GetValues(typeof(Durations)).Cast<Durations>().ToList().ToArray();
            int index = 0;
            int closest_dist = int.MaxValue - 1;
            for(int i = 0; i < dur_arr.Length; i++)
            {
                int distance = (int)Math.Abs((int)dur_arr[i] - dur);
                if(distance < closest_dist)
                {
                    closest_dist = distance;
                    index = i;
                }
            }
            return dur_arr[index];
        }

        public static Durations GetClosestDuration(Durations[] durs, int dur)
        {
            int index = 0;
            int closest_dist = int.MaxValue - 1;
            for (int i = 0; i < durs.Length; i++)
            {
                int distance = (int)Math.Abs((int)durs[i] - dur);
                if (distance < closest_dist)
                {
                    closest_dist = distance;
                    index = i;
                }
            }
            return durs[index];
        }

        public static int GetClosestInt(int val, int[] values)
        {
            int index = 0;
            int closest_dist = int.MaxValue - 1;
            for (int i = 0; i < values.Length; i++)
            {
                int distance = (int)Math.Abs(values[i] - val);
                if(distance < closest_dist)
                {
                    closest_dist = distance;
                    index = i;
                }
            }
            return values[index];
        }

        public int ArgumentsCount
        {
            get 
            {
                if (GeneType == GPGeneType.Function)
                {
                    switch (Function)
                    {
                        case FunctionTypes.Concatenation:
                            return 2;
                        case FunctionTypes.PitchShift:
                            return 1;
                        case FunctionTypes.DurationShift:
                            return 1;
                        case FunctionTypes.Swap:
                            return 1;
                        case FunctionTypes.Repeat:
                            return 1;
                        default:
                            return 1;
                    }
                }
                else return 0;
            }
        }

        public GPGeneType GeneType {get; set;}
       

        public IGPGene Clone()
        {
            var NoteGene = new NoteGene(Pitch, Duration, Octave, Function, GeneType);
            return NoteGene;
        }

        public IGPGene CreateNew(GPGeneType type)
        {
            return new NoteGene(type);
        }

        public IGPGene CreateNew()
        {
            return CreateNew((rand.Next(5) == 1) ? GPGeneType.Argument : GPGeneType.Function);
        }

        public void ShiftPitch(int val)
        {
            int p = this.Pitch + 12 * this.Octave;
            p += val;

            if (AllowedFullPitches != null)
            {
                p = GetClosestInt(p, AllowedFullPitches);
            }

            this.Pitch = p % 12;
            this.Octave = p / 12;
        }

        public void ShiftDuration(int val)
        {
            int dur = (int)this.Duration;
            dur = (int)(dur * Math.Pow(2, val));
            if (dur < 1)
                dur = 1;
            if (dur > 64)
                dur = 64;
            this.Duration = (Durations)dur;

            if (AllowedDurations != null)
                this.Duration = GetClosestDuration(AllowedDurations, dur);
        }


        public void Generate(GPGeneType type)
        {
            this.GeneType = type;
            Pitch = rand.Next(0, 12);
            Octave = rand.Next(4, 7);
            Duration = (Durations)(int)Math.Pow(2, rand.Next(0, 7));
            FuncArg = 0;
            if (type == GPGeneType.Function)
            {
                Function = (FunctionTypes)(rand.Next(FUNCTIONS_NUM));
            }
            if (Function == FunctionTypes.DurationShift)
            {
                FuncArg = rand.Next(-2, 3);
            }
            if (Function == FunctionTypes.PitchShift)
            {
                FuncArg = rand.Next(-12, 13);
            }
            if (Function == FunctionTypes.Swap)
            {
                FuncArg = 0;
            }
            if (Function == FunctionTypes.Repeat)
            {
                FuncArg = rand.Next(0, 6);
            }


            if (AllowedDurations != null)
            {
                Duration = AllowedDurations[rand.Next(0, AllowedDurations.Length)];
            }
            if (AllowedFullPitches != null)
            {
                int pitch = AllowedFullPitches[rand.Next(0, AllowedFullPitches.Length)];
                Pitch = pitch % 12;
                Octave = pitch / 12;
            }

        }

        public void Generate()
        {
            Generate((rand.Next(5) == 1) ? GPGeneType.Argument : GPGeneType.Function);
        }

        public int MaxArgumentsCount
        {
            get { return 2; }
        }

        public Note GenerateNote()
        {
            if (this.Octave == 0)
                this.Pitch = -1;
            if (GeneType == GPGeneType.Argument)
                return new Note(this.Pitch + this.Octave*12, (int)(this.Duration));
            else
                throw new Exception("Not an argument");
        }
    }
}
