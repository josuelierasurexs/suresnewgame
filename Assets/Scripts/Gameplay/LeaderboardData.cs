using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Surexs.DanceOff.Gameplay
{
    [Serializable]
    public sealed class LeaderboardEntry
    {
        public string initials;
        public string mode;
        public int score;
        public int prizeScore;
        public int perfects;
        public int greats;
        public int goods;
        public int misses;
        public int maxCombo;
        public string createdAtUtc;

        public GameMode GameMode => string.Equals(mode, nameof(GameMode.LocalVersus),
            StringComparison.Ordinal) ? GameMode.LocalVersus : GameMode.Solo;
    }

    [Serializable]
    internal sealed class LeaderboardDocument
    {
        public int version = 1;
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    public static class LeaderboardStore
    {
        private const string FileName = "surexs_leaderboard.json";
        private const int MaximumStoredEntries = 200;

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static void SaveResult(GameResult result, string player1Initials, string player2Initials = null)
        {
            var document = LoadDocument();
            document.entries.Add(CreateEntry(player1Initials, result.Mode, result.Player1));
            if (result.Mode == GameMode.LocalVersus)
                document.entries.Add(CreateEntry(player2Initials, result.Mode, result.Player2));

            Sort(document.entries);
            if (document.entries.Count > MaximumStoredEntries)
                document.entries.RemoveRange(MaximumStoredEntries,
                    document.entries.Count - MaximumStoredEntries);
            SaveDocument(document);
        }

        public static IReadOnlyList<LeaderboardEntry> GetTop(int maximum = 10)
        {
            var document = LoadDocument();
            Sort(document.entries);
            var result = new List<LeaderboardEntry>();
            for (var index = 0; index < document.entries.Count && result.Count < Mathf.Max(1, maximum); index++)
            {
                var entry = document.entries[index];
                if (entry != null) result.Add(entry);
            }
            return result;
        }

        private static LeaderboardEntry CreateEntry(string initials, GameMode mode, PlayerResult result)
        {
            return new LeaderboardEntry
            {
                initials = NormalizeInitials(initials),
                mode = mode.ToString(),
                score = result.Score,
                prizeScore = result.PrizeScore,
                perfects = result.Perfects,
                greats = result.Greats,
                goods = result.Goods,
                misses = result.Misses,
                maxCombo = result.MaxCombo,
                createdAtUtc = DateTime.UtcNow.ToString("O")
            };
        }

        private static string NormalizeInitials(string value)
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? "AAA" : value.Trim().ToUpperInvariant();
            var chars = new char[3];
            for (var index = 0; index < chars.Length; index++)
            {
                var candidate = index < normalized.Length ? normalized[index] : 'A';
                chars[index] = candidate >= 'A' && candidate <= 'Z' ? candidate : 'A';
            }
            return new string(chars);
        }

        private static LeaderboardDocument LoadDocument()
        {
            if (!File.Exists(FilePath)) return new LeaderboardDocument();
            try
            {
                var document = JsonUtility.FromJson<LeaderboardDocument>(File.ReadAllText(FilePath));
                if (document == null) return new LeaderboardDocument();
                if (document.entries == null) document.entries = new List<LeaderboardEntry>();
                return document;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Leaderboard] No se pudo leer '{FilePath}'. Se usará un ranking vacío. {exception.Message}");
                return new LeaderboardDocument();
            }
        }

        private static void SaveDocument(LeaderboardDocument document)
        {
            try
            {
                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(FilePath, JsonUtility.ToJson(document, true));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Leaderboard] No se pudo guardar '{FilePath}'. {exception.Message}");
            }
        }

        private static void Sort(List<LeaderboardEntry> entries)
        {
            entries.RemoveAll(entry => entry == null);
            entries.Sort((left, right) =>
            {
                var score = right.score.CompareTo(left.score);
                if (score != 0) return score;
                var prize = right.prizeScore.CompareTo(left.prizeScore);
                if (prize != 0) return prize;
                var combo = right.maxCombo.CompareTo(left.maxCombo);
                if (combo != 0) return combo;
                return string.CompareOrdinal(right.createdAtUtc, left.createdAtUtc);
            });
        }
    }
}
