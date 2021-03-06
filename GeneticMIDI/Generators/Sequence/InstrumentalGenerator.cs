﻿using DotNetLearn.Markov;
using GeneticMIDI.Representation;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GeneticMIDI.Generators.Sequence
{
    [Serializable]
    [ProtoContract]
    public class InstrumentalGenerator : INoteGenerator
    {
        const string SAVE_FILE = "instrumental2.dat";

        [ProtoMember(1)]
        Dictionary<PatchNames, MarkovChain<Note>> instruments = new Dictionary<PatchNames, MarkovChain<Note>>();

        [ProtoMember(2)]
        Dictionary<PatchNames, int> instrument_tracker = new Dictionary<PatchNames, int>();

        int last_next = 0;

        public event OnFitnessUpdate OnPercentage;

        CompositionCategory category;

        private PatchNames lastInstrument;

        public int MaxNotes { get; set; }

        public IEnumerable<PatchNames> Instruments
        {
            get
            {
                return instruments.Keys;
            }
        }

        public bool IsInitialized { get; private set; }
        private string SavePath
        {
            get
            {
                return "save" + System.IO.Path.DirectorySeparatorChar + category.CategoryName + System.IO.Path.DirectorySeparatorChar + SAVE_FILE;
            }
        }

        public InstrumentalGenerator(CompositionCategory category)
        {
            this.category = category;
            this.MaxNotes = 100;
        }

        public InstrumentalGenerator()
        {
            this.MaxNotes = 100;
        }

        public void AssignCategory(CompositionCategory category)
        {
            this.category = category;
        }

        public void Initialize()
        {
            if (System.IO.File.Exists(SavePath))
            {
                Console.WriteLine("Loading from file");
                LoadFromFile();
            }
            else
            {
                Console.WriteLine("Generating from library");
                GenerateFromFiles(category.Compositions);
            }
            IsInitialized = true;
        }

        public void GenerateFromFiles(string path)
        {
            var comps = Utils.LoadCompositionsParallel(path);
            GenerateFromFiles(comps);
        }

        public PatchNames[] GetInstruments(int n)
        {
            var myList = instrument_tracker.ToList();

            myList.Sort((firstPair, nextPair) =>
            {
                return firstPair.Value.CompareTo(nextPair.Value);
            }
            );

            List<PatchNames> instrs = new List<PatchNames>();
            for(int i = 0; i < n; i++)
            {
                instrs.Add(myList[myList.Count - i - 1].Key);
            }
            return instrs.ToArray();
        }


        public void GenerateFromFiles(Composition[] compositions)
        {
            instruments = new Dictionary<PatchNames, MarkovChain<Note>>();
            instrument_tracker = new Dictionary<PatchNames, int>();

            int i = 0;

            int percentage = compositions.Length / 100;
            //foreach (string f in files)
            for(int j = 0; j < compositions.Length; j++)//Parallel.For(0, compositions.Length, j =>
            {
                Composition comp = compositions[j];
                Console.WriteLine("{0:00.00}%\t Adding {1}", (float)i / compositions.Length * 100.0f, comp.NameTag);

                List<int> instruments_ints = new List<int>();

                foreach (var track in comp.Tracks)
                {
                    instruments_ints.Add((int)track.Instrument);
                    var mel = track.GetMainSequence() as MelodySequence;
                    if (!instruments.ContainsKey(track.Instrument))
                    {
                        instruments[track.Instrument] = new MarkovChain<Note>(4);
                        instrument_tracker[track.Instrument] = 1;
                    }
                    lock (instruments[track.Instrument])
                    {
                        try
                        {
                            instrument_tracker[track.Instrument]++;
                        }
                        catch
                        { }
                        instruments[track.Instrument].Add(mel.ToArray());
                    }
                }


                //Report progress
                if (i > percentage)
                {
                    if (OnPercentage != null)
                        OnPercentage(this, i, 0);
                    percentage += compositions.Length / 100;
                }

                i++;
            }//);

            foreach (var k in instrument_tracker.Keys)
            {
                Console.WriteLine("Instrument {0} with count {1}", k, instrument_tracker[k]);
            }

            Save();

            Console.WriteLine("Done");
        }

        private void Save()
        {
            string root = System.IO.Path.GetDirectoryName(SavePath);
            if (!System.IO.Directory.Exists(root))
                System.IO.Directory.CreateDirectory(root);

            System.IO.FileStream fs = new System.IO.FileStream(SavePath, System.IO.FileMode.Create);

            ProtoBuf.Serializer.PrepareSerializer<InstrumentalGenerator>();

            ProtoBuf.Serializer.Serialize(fs, this);

            fs.Close();
        }

        public void LoadFromFile()
        {
            InstrumentalGenerator gen = new InstrumentalGenerator();

            ProtoBuf.Serializer.PrepareSerializer<InstrumentalGenerator>();

            System.IO.FileStream fs = new System.IO.FileStream(SavePath, System.IO.FileMode.Open);

            gen = ProtoBuf.Serializer.Deserialize<InstrumentalGenerator>(fs);

            instruments = gen.instruments;
            //Might give problems
            instrument_tracker = gen.instrument_tracker;

            fs.Close();
        }

        public MelodySequence GenerateInstrument(PatchNames patch, int seed)
        {
            lastInstrument = patch;
            var chain = instruments[patch];
            return new MelodySequence(chain.Chain(MaxNotes,seed));
        }

        public MelodySequence GenerateInstrument(PatchNames patch)
        {
            Random r = new Random();
            int randomSeed = r.Next();
            return GenerateInstrument(patch, randomSeed);
        }

        public void SetInstrument(PatchNames instrument)
        {
            this.lastInstrument = instrument;
        }




      /*  public Composition Generate(IEnumerable<PatchNames> instrs, int seed = 0)
        {
            lastInstruments = instrs;
            int i = 1;
            Composition comp = new Composition();
            foreach (PatchNames instrument in instrs)
            {
                if (instruments.ContainsKey(instrument))
                {
                    Track t = new Track(instrument, (byte)(i++));
                    t.AddSequence(GenerateInstrument(instrument, seed));
                    comp.Add(t);
                }
            }
            return comp;
        }
        */


        public bool HasNext
        {
            get { return true; }
        }

        MelodySequence Generate()
        {
            return this.GenerateInstrument(lastInstrument,last_next++);
        }

        MelodySequence Next()
        {
            return Generate();
        }

        MelodySequence INoteGenerator.Generate()
        {
            return this.GenerateInstrument(lastInstrument, last_next++);
        }

        MelodySequence INoteGenerator.Next()
        {
            return Generate();
        }


        public PatchNames Instrument
        {
            get { return lastInstrument; }
        }
    }
}
