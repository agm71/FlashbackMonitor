﻿using FlashbackMonitor.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashbackMonitor.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FlashbackMonitorSettings.json");

        private async Task CreateSettingsFile()
        {
            Settings defaultSettings = new()
            {
                Forums = [],
                Topics = [],
                Users = [],
                Interval = 10
            };

            using FileStream fs = new(SettingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(fs, defaultSettings);
        }

        public async Task<Settings> GetSettingsAsync()
        {
            Settings settings = new();

            if (!File.Exists(SettingsFilePath))
            {
                await CreateSettingsFile();
            }

            using (FileStream fs = new(SettingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            settings = await JsonSerializer.DeserializeAsync<Settings>(fs);

            return settings;
        }

        public async Task SaveSettingsAsync(MainWindowViewModel viewModel)
        {
            Settings settings = new()
            {
                Forums = viewModel.ForumItems.Where(x => x.IsChecked).Select(x => x.Name).ToList(),
                Topics = [.. viewModel.Topics.Where(t => !string.IsNullOrWhiteSpace(t.TopicName))],
                Users = [.. viewModel.Users.Where(u => !string.IsNullOrWhiteSpace(u.UserName))],
                Interval = viewModel.Interval
            };

            using FileStream fs = new(SettingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(fs, settings);
        }
    }
}
