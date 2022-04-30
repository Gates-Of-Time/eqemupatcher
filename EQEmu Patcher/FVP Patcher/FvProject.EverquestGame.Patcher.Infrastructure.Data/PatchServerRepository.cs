using CSharpFunctionalExtensions;
using FvProject.EverquestGame.Patcher.Application;
using FvProject.EverquestGame.Patcher.Domain;
using FvProject.EverquestGame.Patcher.Domain.Contracts;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FvProject.EverquestGame.Patcher.Infrastructure.Data {
    public class PatchServerRepository : IGetRepository<ServerPatchListQuery, Result<ServerPatchFileList>>, IGetRepository<string, Result<Stream>> {
        public PatchServerRepository(HttpClient httpClient) {
            HttpClient = httpClient;
            Deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        }

        private HttpClient HttpClient { get; }
        private IDeserializer Deserializer { get; }

        public async Task<Result<ServerPatchFileList>> Get(ServerPatchListQuery query) {
            var patchFilesList = CreateUrl(query.Expansion.ShortName, query.GameClient.ShortName);
            if (!Uri.IsWellFormedUriString(patchFilesList, UriKind.Absolute)) {
                return Result.Failure<ServerPatchFileList>($"Malformed URL <{patchFilesList}>");
            }

            try {
                var response = await HttpClient.GetAsync(patchFilesList);
                if (response.IsSuccessStatusCode) {
                    var filesList = await response.Content.ReadAsStringAsync();
                    return Result.Success(Deserializer.Deserialize<ServerPatchFileList>(filesList));
                }
                else {
                    return Result.Failure<ServerPatchFileList>($"Download filed: <{response.StatusCode}> {response}");
                }
            }
            catch (Exception ex) {
                return Result.Failure<ServerPatchFileList>($"Download filed: {ex}");
            }
        }

        public async Task<Result<Stream>> Get(string url) {
            try {
                var response = await HttpClient.GetAsync(url);
                if (response.IsSuccessStatusCode) {
                    var stream = await response.Content.ReadAsStreamAsync();
                    return Result.Success(stream);
                }
                else {
                    return Result.Failure<Stream>($"Download filed: <{response.StatusCode}> {response}");
                }
            }
            catch (Exception ex) {
                return Result.Failure<Stream>($"Download filed: {ex}");
            }
        }

        private string CreateUrl(string expansion, string clientVersion) => $"https://{expansion}.fvproject.com/{clientVersion}/filelist_{clientVersion}.yml";
    }
}
