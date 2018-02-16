﻿using System.Drawing;
using System.Threading.Tasks;
using Mosaic.Layers;

namespace Mosaic.Creators {
    internal sealed class RenderCreator : ICreator {
        private readonly ISize _size;
        private readonly Color[,] _pixels;

        public RenderCreator(ISize size) {
            _size = size;
            _pixels = new Color[size.Width, size.Height];
        }

        public Broadcast Broadcast { get; set; }

        public async Task Set(ILayerResult input) => await Task.Factory.StartNew(() => {
            Parallel.For(0, input.Width, x => {
                for (var y = 0; y < input.Height; y++) {
                    _pixels[input.Left + x, input.Top + y] = input.Colors[x, y];
                }
            });
        });

        public async Task Flush(string filename) => await Task.Factory.StartNew(() => {
            Broadcast.Start(this, $"Saving {filename}...");
            try {
                using (var bmp = new Bitmap(_size.Width, _size.Height)) {
                    for (var x = 0; x < _size.Width; x++) {
                        for (var y = 0; y < _size.Height; y++) {
                            bmp.SetPixel(x, y, _pixels[x, y]);
                        }
                    }
                    bmp.Save(filename);
                }
            }
            finally {
                Broadcast.End(this);
            }
        });
    }
}