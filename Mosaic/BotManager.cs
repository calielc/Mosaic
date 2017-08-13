﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Mosaic
{
    internal class BotManager
    {
        private ConcurrentBitmapCollection _images;
        private IReadOnlyCollection<PixelBot> _bots;
        private string _baseFile;

        public BotManager() { }

        public bool UseParallel { get; set; }
        public bool Heatmap { get; set; }

        public int Width => _images?.Width ?? 0;
        public int Height => _images?.Height ?? 0;

        public string SearchDirectory { get; internal set; }
        public string SearchPattern { get; internal set; }
        public string DestinyDirectory { get; internal set; }
        public string DestinyFilename { get; internal set; }

        public event EventHandler<string> OnText;
        public event EventHandler<double> OnProgress;

        protected void SendText(string text)
        {
            OnText?.Invoke(this, text);
        }

        protected void SendProgress(double percentual)
        {
            OnProgress?.Invoke(this, percentual);
        }

        public void LoadImages()
        {
            SendText("Loading images...");

            var filenames = System.IO.Directory.GetFiles(SearchDirectory, SearchPattern);
            _images = new ConcurrentBitmapCollection(filenames);

            SendText($"Loaded {_images.Count} files.");
            SendText($"Dimensions: {Width} x {Height}.");

            _baseFile = filenames.First();
        }

        public void CreateBots()
        {
            SendText("Creating bots...");

            IEnumerable<PixelBot> Execute()
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        yield return new PixelBot(x, y, _images)
                        {
                            GetNeighbours = GetNeighbours
                        };
                    }
                }
            }
            _bots = Execute().ToArray();

            SendText($"Created {_bots.Count} bots.");
        }

        private IEnumerable<PixelBot> GetNeighbours(PixelBot source) => _bots
            .AsParallel()
            .Where(bot => bot != source)
            .Select(bot => new
            {
                Bot = bot,
                Distance = bot.NeighbourDistance(source)
            })
            .OrderBy(tuple => tuple.Distance)
            .Select(tuple => tuple.Bot);

        public void LoadBots()
        {
            SendText("Loading bots...");
            double total = _bots.Count();

            var index = 0;

            void Execute(PixelBot bot)
            {
                bot.Load();

                index++;
                if (index % 750 == 0)
                {
                    SendProgress(index / total);
                }
            }
            ForEach(_bots, Execute);

            if (Heatmap)
            {
                using (var bmp = new Bitmap(_baseFile))
                {
                    foreach (var bot in _bots)
                    {
                        var color = Extensions.Interpolate(Color.Red, Color.Green, bot.Chance);
                        bmp.SetPixel(bot.X, bot.Y, color);
                    }

                    bmp.Save(Path.Combine(DestinyDirectory, $"{DestinyFilename}-heat.jpg"));
                };
            }

            SendProgress(index / total);
            SendText("Done");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ForEach<T>(IEnumerable<T> enumerable, Action<T> execute)
        {
            if (UseParallel)
            {
                Parallel.ForEach(enumerable, item => execute(item));
            }
            else
            {
                foreach (var item in enumerable)
                {
                    execute(item);
                }
            }
        }

        internal void SaveToFile()
        {
            SendText("Saving colors...");
            double total = _bots.Count();

            var step = 1d;

            var index = 0;
            void Execute(Bitmap bmp, PixelBot bot)
            {
                if (bot.Chance < step)
                {
                    var filename = Path.Combine(DestinyDirectory, $"{DestinyFilename}-{step:0.00}.jpg");
                    bmp.Save(filename);

                    step -= 0.05d;
                }

                bot.Save(bmp);

                index++;
                if (index % 750 == 0)
                {
                    SendProgress(index / total);
                }
            }

            using (var bmp = new Bitmap(_baseFile))
            {
                bmp.Save(Path.Combine(DestinyDirectory, $"{DestinyFilename}-{2d:0.00}.jpg"));

                var orderedBots = _bots.OrderByDescending(bot => bot.Chance);
                foreach (var bot in orderedBots)
                {
                    Execute(bmp, bot);
                }

                bmp.Save(Path.Combine(DestinyDirectory, DestinyFilename));
            };

            SendProgress(index / total);
            SendText("Done");
        }
    }
}