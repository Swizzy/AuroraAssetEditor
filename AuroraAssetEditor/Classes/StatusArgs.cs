namespace AuroraAssetEditor.Classes {
    using System;

    internal class StatusArgs: EventArgs {
        public readonly string StatusMessage;

        public StatusArgs(string statusMessage) { StatusMessage = statusMessage; }
    }
}