using System;
using System.IO;
using System.Threading.Tasks;
using PowerArgs;

namespace MosaicCmd {
    static class Program {
        static async Task Main(string[] args) {
            var programArgs = Args.Parse<ProgramArgs>(args);

            if (programArgs.DestinyDirectory == null) {
                var destinyDirectory = $@"{programArgs.SearchDirectory}\Merged-{DateTime.Now:yyyMMdd}\";
                var path = Path.GetDirectoryName(destinyDirectory);
                Directory.CreateDirectory(path);

                programArgs.DestinyDirectory = destinyDirectory;
            }

            await new Execute(programArgs).Exec();
        }
    }
}