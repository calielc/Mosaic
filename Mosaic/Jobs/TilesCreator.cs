﻿using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Mosaic.Layers;
using Mosaic.Queue;

namespace Mosaic.Jobs {
    internal sealed class TilesCreator : ICreator, IActivity, IDisposable {
        private readonly string _filename;
        private readonly Broadcast _broadcast;
        private readonly Bitmap _bitmap;
        private readonly Graphics _graphics;
        private static readonly Pen PenBlack = new Pen(Color.Black);
        private static readonly Brush BrushBlack = new SolidBrush(Color.Black);
        private static readonly Font DefaultFont = new Font("Arial", 8);

        public TilesCreator(ISize size, string filename, Broadcast broadcast) {
            _filename = AdjustFilename();
            _broadcast = broadcast;

            _bitmap = new Bitmap(size.Width, size.Height);
            _graphics = Graphics.FromImage(_bitmap);

            string AdjustFilename() {
                var directory = Path.GetDirectoryName(filename);
                var name = Path.GetFileNameWithoutExtension(filename);

                return Path.Combine(directory, $"{name}-layers.jpg");
            }
        }

        public async Task Set(ILayerResult input) => await Task.Run(() => {
            var rect = new Rectangle(input.Left, input.Top, input.Width, input.Height);

            var brush = new SolidBrush(GetColor());
            var odds = $"{GetOdds():##0.0}";

            lock (_graphics) {
                _graphics.FillRectangle(brush, rect);
                _graphics.DrawRectangle(PenBlack, rect);
                _graphics.DrawString(odds, DefaultFont, BrushBlack, rect);
            }

            double GetOdds() {
                var result = 0d;

                for (var x = 0; x < input.Width; x++) {
                    for (var y = 0; y < input.Height; y++) {
                        result += input.Odds[x, y] * 100d;
                    }
                }
                return result / (input.Width * input.Height);
            }

            Color GetColor() {
                var col = Convert.ToInt32(Math.Round(1d * input.Left / input.Width)) % 2;
                var row = Convert.ToInt32(Math.Round(1d * input.Top / input.Height)) % 2;

                if (col == 0) {
                    return row == 0
                        ? Color.FromArgb(170, 255, 130)
                        : Color.FromArgb(215, 130, 255);
                }

                return row == 0
                    ? Color.FromArgb(255, 130, 170)
                    : Color.FromArgb(130, 255, 215);
            }
        });

        public async Task Run() => await Task.Factory.StartNew(() => {
            _broadcast.Start(this, $"Saving {_filename}...");
            try {
                _bitmap.Save(_filename);
            }
            finally {
                _broadcast.End(this);
            }
        });

        public void Dispose() {
            _graphics?.Dispose();
            _bitmap?.Dispose();
        }
    }
}