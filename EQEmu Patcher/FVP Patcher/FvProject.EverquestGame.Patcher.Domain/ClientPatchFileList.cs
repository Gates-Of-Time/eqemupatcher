namespace FvProject.EverquestGame.Patcher.Domain
{
    public class ClientPatchFileList
    {
        public ExpansionsEnum? Expansion { get; set; }
        public string Downloadprefix { get; set; } = "";
        public IEnumerable<FileEntry> Deletes { get; set; } = new List<FileEntry>();
        public IEnumerable<FileEntry> Downloads { get; set; } = new List<FileEntry>();

        public bool HasChanges => Deletes.Any() || Downloads.Any();
        public string DownloadSize => FormatBytes(Downloads.Sum(x => x.size < 1 ? 1 : x.size));

        private string FormatBytes(long bytes) {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders) {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }
    }
}
