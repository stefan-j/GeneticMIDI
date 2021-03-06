﻿// AForge Genetic Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2006-2011
// contacts@aforgenet.com
//

namespace AForge.Genetic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using AForge;
    using GeneticMIDI;
    using GeneticMIDI.Representation;
    using GeneticMIDI.Generators;

/// <summary>
/// Reimplementation of GPCustomTree from Aforge for simple series monophonic note sequences
/// </summary>
    public class GPCustomTree : ChromosomeBase
    {
        // tree root
        protected GPCustomTreeNode root = new GPCustomTreeNode();

        // maximum initial level of the tree
        private static int maxInitialLevel = 6;
        // maximum level of the tree
        private static int maxLevel = 12;

        public static INoteGenerator generator = null;

        /// <summary>
        /// Random generator used for chromosoms' generation.
        /// </summary>
        protected static ThreadSafeRandom rand = new ThreadSafeRandom();

        /// <summary>
        /// Maximum initial level of genetic trees, [1, 25].
        /// </summary>
        /// 
        /// <remarks><para>The property sets maximum possible initial depth of new
        /// genetic programming tree. For example, if it is set to 1, then largest initial
        /// tree may have a root and one level of children.</para>
        /// 
        /// <para>Default value is set to <b>3</b>.</para>
        /// </remarks>
        ///
        public static int MaxInitialLevel
        {
            get { return maxInitialLevel; }
            set { maxInitialLevel = Math.Max(1, Math.Min(25, value)); }
        }

        /// <summary>
        /// Maximum level of genetic trees, [1, 50].
        /// </summary>
        /// 
        /// <remarks><para>The property sets maximum possible depth of 
        /// genetic programming tree, which may be created with mutation and crossover operators.
        /// This property guarantees that genetic programmin tree will never have
        /// higher depth, than the specified value.</para>
        /// 
        /// <para>Default value is set to <b>5</b>.</para>
        /// </remarks>
        ///
        public static int MaxLevel
        {
            get { return maxLevel; }
            set { maxLevel = Math.Max(1, Math.Min(50, value)); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GPTreeChromosome"/> class.
        /// </summary>
        /// 
        /// <param name="ancestor">A gene, which is used as generator for the genetic tree.</param>
        /// 
        /// <remarks><para>This constructor creates a randomly generated genetic tree,
        /// which has all genes of the same type and properties as the specified <paramref name="ancestor"/>.
        /// </para></remarks>
        /// 
        public GPCustomTree(NoteGene ancestor)
        {
            // make the ancestor gene to be as temporary root of the tree
            root.Gene = ancestor.Clone();
            // call tree regeneration function
            Generate();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="GPTreeChromosome"/> class.
        /// </summary>
        /// 
        /// <param name="source">Source genetic tree to clone from.</param>
        /// 
        /// <remarks><para>This constructor creates new genetic tree as a copy of the
        /// specified <paramref name="source"/> tree.</para></remarks>
        /// 
        protected GPCustomTree(GPCustomTree source)
        {
            root = (GPCustomTreeNode)source.root.Clone();
            fitness = source.fitness;
        }

        /// <summary>
        /// Get string representation of the chromosome by providing its expression in
        /// reverse polish notation (postfix notation).
        /// </summary>
        /// 
        /// <returns>Returns string representation of the genetic tree.</returns>
        /// 
        /// <remarks><para>The method returns string representation of the tree's root node
        /// (see <see cref="GPTreeNode.ToString"/>).</para></remarks>
        ///
        public override string ToString()
        {
            return root.ToString();
        }



        public void Generate(IEnumerable <Note> notes)
        {
            
            var node = root;
            foreach(Note n in notes)
            {
                node.Gene = new NoteGene(0, 0, 0, NoteGene.FunctionTypes.Concatenation, GPGeneType.Function);
                node.Children = new List<GPCustomTreeNode>(2);
                node.Children.Add(new GPCustomTreeNode(new NoteGene(n.NotePitch, (Durations)n.Duration, n.Octave, NoteGene.FunctionTypes.Concatenation, GPGeneType.Argument)));
                node.Children.Add(new GPCustomTreeNode(new NoteGene(0, 0, 0, NoteGene.FunctionTypes.Concatenation, GPGeneType.Function)));
                node = node.Children[1];
            }

        }

        /// <summary>
        /// Generate random chromosome value.
        /// </summary>
        /// 
        /// <remarks><para>Regenerates chromosome's value using random number generator.</para>
        /// </remarks>
        ///
        public override void Generate()
        {
            // randomize the root
            root.Gene.Generate();
            // create children
            if (root.Gene.ArgumentsCount != 0)
            {
                root.Children = new List<GPCustomTreeNode>();
                for (int i = 0; i < root.Gene.ArgumentsCount; i++)
                {
                    // create new child

                    GPCustomTreeNode child = new GPCustomTreeNode();
                    Generate(child, rand.Next(maxInitialLevel));
                    // add the new child
                    root.Children.Add(child);
                }
            }
        }

        /// <summary>
        /// Generate chromosome's subtree of specified level.
        /// </summary>
        /// 
        /// <param name="node">Sub tree's node to generate.</param>
        /// <param name="level">Sub tree's level to generate.</param>
        /// 
        protected void Generate(GPCustomTreeNode node, int level)
        {
            if(generator != null)
            {
                if (level == 0)
                {
                    node.Gene = root.Gene.CreateNew(GPGeneType.Argument);
                }
                else
                {
                    var notes = generator.Generate();
                    List<Note> notes_cut = new List<Note>();
                    int j = 0;
                    int max = (int)Math.Pow(2,level-1);
                    foreach(Note n in notes.Notes)
                    {
                        if(j>max)
                        {
                            if (n.Velocity <= 0 || n.Pitch < 0)
                                break;
                        }
                        notes_cut.Add(n);
                    }
                    node.Generate(notes_cut);
                }
                return;
            }

            /// OLD !!!
            // create gene for the node
            if (level == 0)
            {
                // the gene should be an argument
                node.Gene = root.Gene.CreateNew(GPGeneType.Argument);
            }
            else
            {
                // the gene can be function or argument
                node.Gene = root.Gene.CreateNew();
            }

            // add children
            if (node.Gene.ArgumentsCount != 0)
            {
                node.Children = new List<GPCustomTreeNode>();
                for (int i = 0; i < node.Gene.ArgumentsCount; i++)
                {
                    // create new child
                    GPCustomTreeNode child = new GPCustomTreeNode();
                    Generate(child, level - 1);
                    // add the new child
                    node.Children.Add(child);
                }
            }
        }

        /// <summary>
        /// Create new random chromosome with same parameters (factory method).
        /// </summary>
        /// 
        /// <remarks><para>The method creates new chromosome of the same type, but randomly
        /// initialized. The method is useful as factory method for those classes, which work
        /// with chromosome's interface, but not with particular chromosome type.</para></remarks>
        /// 
        public override IChromosome CreateNew()
        {
            return new GPCustomTree(root.Gene.Clone() as NoteGene);
        }

        /// <summary>
        /// Clone the chromosome.
        /// </summary>
        /// 
        /// <returns>Return's clone of the chromosome.</returns>
        /// 
        /// <remarks><para>The method clones the chromosome returning the exact copy of it.</para>
        /// </remarks>
        ///
        public override IChromosome Clone()
        {
            return new GPCustomTree(this);
        }

        /// <summary>
        /// Mutation operator.
        /// </summary>
        /// 
        /// <remarks><para>The method performs chromosome's mutation by regenerating tree's
        /// randomly selected node.</para></remarks>
        ///
        public override void Mutate()
        {
            // current tree level
            int currentLevel = 0;
            // current node
            GPCustomTreeNode node = root;

            for (; ; )
            {
                // regenerate node if it does not have children
                if (node.Children == null)
                {
                    if (currentLevel == maxLevel)
                    {
                        // we reached maximum possible level, so the gene
                        // can be an argument only
                        node.Gene.Generate(GPGeneType.Argument);
                    }
                    else
                    {
                        // generate subtree
                        int level = maxLevel - currentLevel;
                        if (level < 0)
                            level = 0;
                        Generate(node, rand.Next(level));
                    }
                    break;
                }

                // if it is a function node, than we need to get a decision, about
                // mutation point - the node itself or one of its children
                int r = rand.Next(node.Gene.ArgumentsCount + 1);

                if (r == node.Gene.ArgumentsCount)
                {
                    // node itself should be regenerated
                    node.Gene.Generate();

                    // check current type
                    if (node.Gene.GeneType == GPGeneType.Argument)
                    {
                        node.Children = null;
                    }
                    else
                    {
                        // create children's list if it was absent
                        if (node.Children == null)
                            node.Children = new List<GPCustomTreeNode>();

                        // check for missing or extra children
                        if (node.Children.Count != node.Gene.ArgumentsCount)
                        {
                            if (node.Children.Count > node.Gene.ArgumentsCount)
                            {
                                // remove extra children
                                node.Children.RemoveRange(node.Gene.ArgumentsCount, node.Children.Count - node.Gene.ArgumentsCount);
                            }
                            else
                            {
                                // add missing children
                                for (int i = node.Children.Count; i < node.Gene.ArgumentsCount; i++)
                                {
                                    // create new child
                                    GPCustomTreeNode child = new GPCustomTreeNode();
                                    Generate(child, rand.Next(maxLevel - currentLevel));
                                    // add the new child
                                    node.Children.Add(child);
                                }
                            }
                        }
                    }
                    break;
                }

                // mutation goes further to one of the children
                node = (GPCustomTreeNode)node.Children[r];
                currentLevel++;
            }
        }

        /// <summary>
        /// Crossover operator.
        /// </summary>
        /// 
        /// <param name="pair">Pair chromosome to crossover with.</param>
        /// 
        /// <remarks><para>The method performs crossover between two chromosomes – interchanging
        /// randomly selected sub trees.</para></remarks>
        ///
        public override void Crossover(IChromosome pair)
        {
            GPCustomTree p = (GPCustomTree)pair;

            // check for correct pair
            if (p != null)
            {
                // do we need to use root node for crossover ?
                if ((root.Children == null) || (rand.Next(maxLevel) == 0))
                {
                    // give the root to the pair and use pair's part as a new root
                    root = p.RandomSwap(root);
                }
                else
                {
                    GPCustomTreeNode node = root;

                    for (; ; )
                    {
                        // choose random child
                        int r = rand.Next(node.Gene.ArgumentsCount);
                        GPCustomTreeNode child = (GPCustomTreeNode)node.Children[r];

                        // swap the random node, if it is an end node or
                        // random generator "selected" this node
                        if ((child.Children == null) || (rand.Next(maxLevel) == 0))
                        {
                            // swap the node with pair's one
                            node.Children[r] = p.RandomSwap(child);
                            break;
                        }

                        // go further by tree
                        node = child;
                    }
                }
                // trim both of them
                Trim(root, maxLevel);
                Trim(p.root, maxLevel);
            }
        }

        /// <summary>
        /// Crossover helper routine - selects random node of chromosomes tree and
        /// swaps it with specified node.
        /// </summary>
        private GPCustomTreeNode RandomSwap(GPCustomTreeNode source)
        {
            GPCustomTreeNode retNode = null;

            // swap root node ?
            if ((root.Children == null) || (rand.Next(maxLevel) == 0))
            {
                // replace current root and return it
                retNode = root;
                root = source;
            }
            else
            {
                GPCustomTreeNode node = root;

                for (; ; )
                {
                    // choose random child
                    int r = rand.Next(node.Gene.ArgumentsCount);
                    GPCustomTreeNode child = (GPCustomTreeNode)node.Children[r];

                    // swap the random node, if it is an end node or
                    // random generator "selected" this node
                    if ((child.Children == null) || (rand.Next(maxLevel) == 0))
                    {
                        // swap the node with pair's one
                        retNode = child;
                        node.Children[r] = source;
                        break;
                    }

                    // go further by tree
                    node = child;
                }
            }
            return retNode;
        }

        /// <summary>
        /// Trim tree node, so its depth does not exceed specified level.
        /// </summary>
        private static void Trim(GPCustomTreeNode node, int level)
        {
            // check if the node has children
            if (node.Children != null)
            {
                if (level == 0)
                {
                    // remove all children
                    node.Children = null;
                    // and make the node of argument type
                    node.Gene.Generate(GPGeneType.Argument);
                }
                else
                {
                    // go further to children
                    foreach (GPCustomTreeNode n in node.Children)
                    {
                        Trim(n, level - 1);
                    }
                }
            }
        }

        public List<Note> GenerateNotes()
        {
            return this.root.GenerateNotes();
        }
    }
}
