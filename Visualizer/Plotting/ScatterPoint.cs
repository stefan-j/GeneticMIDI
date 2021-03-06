﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScatterPoint.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Represents a point in a <see cref="ScatterSeries" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using OxyPlot;
namespace Visualizer.Plotting
{
    /// <summary>
    /// Represents a point in a <see cref="ScatterSeries" />.
    /// </summary>
    public class ScatterPoint : ICodeGenerating
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterPoint" /> class.
        /// </summary>
        public ScatterPoint()
        {
            this.Size = double.NaN;
            this.Value = double.NaN;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterPoint" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="size">The size.</param>
        /// <param name="value">The value.</param>
        /// <param name="tag">The tag.</param>
        public ScatterPoint(double x, double y, double size = double.NaN, double value = double.NaN, object tag = null)
        {
            this.X = x;
            this.Y = y;
            this.Size = size;
            this.Value = value;
            this.Tag = tag;
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public double Size { get; set; }


        public OxyColor Color { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>The tag.</value>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        /// <value>The X.</value>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        /// <value>The Y.</value>
        public double Y { get; set; }

        /// <summary>
        /// Returns C# code that generates this instance.
        /// </summary>
        /// <returns>C# code.</returns>
        public string ToCode()
        {
            if (double.IsNaN(this.Size) && double.IsNaN(this.Value))
            {
                return CodeGenerator.FormatConstructor(this.GetType(), "{0}, {1}", this.X, this.Y);
            }

            if (double.IsNaN(this.Value))
            {
                return CodeGenerator.FormatConstructor(this.GetType(), "{0}, {1}, {2}", this.X, this.Y, this.Size);
            }

            return CodeGenerator.FormatConstructor(
                this.GetType(), "{0}, {1}, {2}, {3}", this.X, this.Y, this.Size, this.Value);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this.X + " " + this.Y;
        }
    }
}