using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    internal static class SuggestionHelper
    {
        public static IReadOnlyList<string> FindSuggestions(string value, IEnumerable<Candidate> candidates, int maximumSuggestions = 3)
        {
            string normalizedValue = Normalize(value);
            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                return new string[0];
            }

            var suggestions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (Candidate candidate in candidates ?? Enumerable.Empty<Candidate>())
            {
                int? score = GetSuggestionScore(normalizedValue, candidate.Terms);
                if (!score.HasValue)
                {
                    continue;
                }

                int currentScore;
                if (!suggestions.TryGetValue(candidate.DisplayName, out currentScore) || score.Value < currentScore)
                {
                    suggestions[candidate.DisplayName] = score.Value;
                }
            }

            return suggestions
                .OrderBy(suggestion => suggestion.Value)
                .ThenBy(suggestion => suggestion.Key, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, maximumSuggestions))
                .Select(suggestion => suggestion.Key)
                .ToArray();
        }

        public static string AppendSuggestion(string message, IReadOnlyList<string> suggestions)
        {
            if (suggestions == null || suggestions.Count == 0)
            {
                return message;
            }

            if (suggestions.Count == 1)
            {
                return message + " Did you mean '" + suggestions[0] + "'?";
            }

            return message + " Did you mean one of these? " + string.Join(", ", suggestions.Select(suggestion => "'" + suggestion + "'")) + "?";
        }

        private static int? GetSuggestionScore(string value, IReadOnlyList<string> terms)
        {
            int? bestScore = null;
            foreach (string term in terms ?? new string[0])
            {
                string candidate = Normalize(term);
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                int distance = CalculateEditDistance(value, candidate);
                if (!IsSuggestedDistance(value, candidate, distance))
                {
                    continue;
                }

                if (!bestScore.HasValue || distance < bestScore.Value)
                {
                    bestScore = distance;
                }
            }

            return bestScore;
        }

        private static bool IsSuggestedDistance(string value, string candidate, int distance)
        {
            if (distance == 0)
            {
                return true;
            }

            int maxLength = Math.Max(value.Length, candidate.Length);
            int allowedDistance = maxLength <= 5
                ? 1
                : maxLength <= 10
                    ? 2
                    : 3;

            return distance <= allowedDistance;
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static int CalculateEditDistance(string value, string candidate)
        {
            int valueLength = value.Length;
            int candidateLength = candidate.Length;
            var distances = new int[valueLength + 1, candidateLength + 1];

            for (int valueIndex = 0; valueIndex <= valueLength; valueIndex++)
            {
                distances[valueIndex, 0] = valueIndex;
            }

            for (int candidateIndex = 0; candidateIndex <= candidateLength; candidateIndex++)
            {
                distances[0, candidateIndex] = candidateIndex;
            }

            for (int valueIndex = 1; valueIndex <= valueLength; valueIndex++)
            {
                for (int candidateIndex = 1; candidateIndex <= candidateLength; candidateIndex++)
                {
                    int cost = value[valueIndex - 1] == candidate[candidateIndex - 1] ? 0 : 1;
                    distances[valueIndex, candidateIndex] = Math.Min(
                        Math.Min(
                            distances[valueIndex - 1, candidateIndex] + 1,
                            distances[valueIndex, candidateIndex - 1] + 1),
                        distances[valueIndex - 1, candidateIndex - 1] + cost);
                }
            }

            return distances[valueLength, candidateLength];
        }

        internal sealed class Candidate
        {
            public Candidate(string displayName, IEnumerable<string> terms)
            {
                DisplayName = displayName ?? string.Empty;
                Terms = new[] { DisplayName }
                    .Concat(terms ?? Enumerable.Empty<string>())
                    .Where(term => !string.IsNullOrWhiteSpace(term))
                    .ToArray();
            }

            public string DisplayName { get; }

            public IReadOnlyList<string> Terms { get; }
        }
    }
}
