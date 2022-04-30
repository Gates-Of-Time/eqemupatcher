using System;
using System.Collections.Generic;
using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Domain;

namespace FvProject.EverquestGame.Patcher.Presentation.Client {
    public class ApplicationData : IApplicationConfig {
        public ApplicationData(string gameDirectory = null, IEnumerable<ExpansionsEnum> supportedExpansions = null) {
            GameDirectory = gameDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
            SupportedExpansions = supportedExpansions ?? Array.Empty<ExpansionsEnum>();
        }

        public IEnumerable<ExpansionsEnum> SupportedExpansions { get; }

        public string GameDirectory { get; set; }
        public ExpansionsEnum PreferredExpansion { get; set; }
    }
}
