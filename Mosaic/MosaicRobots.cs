using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic
{
    internal class MosaicManager
    {
        private IEnumerable<ConcurrentBitmap> _images;
        private IReadOnlyCollection<MosaicBot> _bots;
        private int _width;
        private int _height;

        public MosaicManager()
        {
        }

        internal void LoadImages(string folder)
        {
            var images = new ConcurrentBag<ConcurrentBitmap>();
            Parallel.ForEach(
                System.IO.Directory.GetFiles(folder, "*.jpg"),
                file =>
                {
                    var bitmap = new ConcurrentBitmap(file);
                    images.Add(bitmap);
                });
            _images = images;
        }

        internal void CreateBots()
        {
            var first = _images.First();
            _width = first.Width;
            _height = first.Height;

            _bots = GenerateBots().ToArray();
        }

        private IEnumerable<MosaicBot> GenerateBots()
        {
            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    yield return new MosaicBot(x, y, _images);
                }
            }
        }

        internal void Process()
        {
            decimal total = _bots.Count();
            Console.WriteLine($"count: {total}");

            var index = 0;
            void ProcessItem(MosaicBot bot)
            {
                bot.Process();
                index++;
                if (index % 200 == 0)
                {
                    Console.WriteLine($"{(index / total):p2}");
                }
            }

            foreach (var bot in _bots) ProcessItem(bot);
            //Parallel.ForEach(_bots, ProcessItem);
        }

        internal void SaveToFile(string filename)
        {
            using (var bmp = new Bitmap(_width, _height))
            {
                foreach (var bot in _bots)
                {
                    bot.Build(bmp);
                }
                bmp.Save(filename);
            }
        }
    }
}