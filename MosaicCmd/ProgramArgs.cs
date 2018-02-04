using PowerArgs;

namespace MosaicCmd {
    public sealed class ProgramArgs {
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("Search Directory")]
        [ArgExistingDirectory]
        public string SearchDirectory { get; set; }

        [ArgRequired(PromptIfMissing = true)]
        [ArgDefaultValue("*.jpg")]
        [ArgDescription("Search Pattern")]
        public string SearchPattern { get; set; }

        [ArgDescription("Destiny Directory")]
        [ArgExistingDirectory]
        public string DestinyDirectory { get; set; }

        [ArgRequired(PromptIfMissing = true)]
        [ArgDefaultValue("mosaic.jpg")]
        [ArgDescription("Destiny Filename")]
        public string DestinyFileName { get; set; }

        [ArgDefaultValue(false)]
        [ArgDescription("How many bots")]
        public int Parallel { get; set; }

        [ArgDefaultValue(false)]
        [ArgDescription("Will generate a heatmap file")]
        public bool Heatmap { get; set; }

        [ArgDefaultValue(false)]
        [ArgDescription("Will generate a animated gif")]
        public bool AnimatedGif { get; set; }
    }
}