namespace FvProject.EverquestGame.Patcher.Domain
{
    public class ServerPatchFileList
    {
        public string version { get; set; } = "";
        public List<FileEntry> deletes { get; set; } = new List<FileEntry>();
        public string downloadprefix { get; set; } = "";
        public List<FileEntry> downloads { get; set; } = new List<FileEntry>();
        public List<FileEntry> unpacks { get; set; } = new List<FileEntry>();
    }
}
