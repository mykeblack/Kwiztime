using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Kwiztime
{
    public enum QuestionCategory
    {
        General,
        Animals,
        Food,
        Science,
        Geography,
        Music,
        Shapes,
        Words
    }

    public enum QuestionDifficulty
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    public struct QuestionData
    {
        public string prompt;
        public string[] answers;        // length 4 for prototype
        public int correctIndex;        // 0..3
        public float timeLimit;         // seconds

        public QuestionCategory category;
        public QuestionDifficulty difficulty;
    }

    public class QuestionService : NetworkBehaviour
    {
        [Header("Prototype Question Settings")]
        [SerializeField] private float defaultTimeLimitEasy = 10f;
        [SerializeField] private float defaultTimeLimitMedium = 9f;
        [SerializeField] private float defaultTimeLimitHard = 8f;

        [Tooltip("If true, question order will be shuffled each time the server starts.")]
        [SerializeField] private bool shuffleOnServerStart = true;

        // Master list
        private readonly List<QuestionData> _questions = new();

        // Cursor per filter key (so cycling works independently per filter)
        private readonly Dictionary<int, int> _cursorsByFilter = new();

        public override void OnStartServer()
        {
            base.OnStartServer();

            BuildTestQuestions();

            if (_questions.Count == 0)
            {
                Debug.LogError("[QuestionService] No questions available!");
                return;
            }

            if (shuffleOnServerStart)
                Shuffle(_questions);

            _cursorsByFilter.Clear();

            Debug.Log($"[QuestionService] Loaded {_questions.Count} test questions.");
        }

        /// <summary>
        /// Returns next question, optionally filtered by category and minimum/maximum difficulty.
        /// Cycles through matching questions.
        /// </summary>
        [Server]
        public QuestionData GetNextQuestion(
            QuestionCategory? categoryFilter = null,
            QuestionDifficulty? minDifficulty = null,
            QuestionDifficulty? maxDifficulty = null)
        {
            if (_questions.Count == 0)
                return GetFallbackQuestion();

            // Build a filtered list of indices (cheap at prototype scale)
            List<int> matches = new List<int>(64);
            for (int i = 0; i < _questions.Count; i++)
            {
                var q = _questions[i];

                if (categoryFilter.HasValue && q.category != categoryFilter.Value)
                    continue;

                if (minDifficulty.HasValue && q.difficulty < minDifficulty.Value)
                    continue;

                if (maxDifficulty.HasValue && q.difficulty > maxDifficulty.Value)
                    continue;

                matches.Add(i);
            }

            if (matches.Count == 0)
            {
                // If no matches, fall back to any question
                return GetAnyQuestion();
            }

            // Create a stable key for this filter so cycling is consistent
            int filterKey = MakeFilterKey(categoryFilter, minDifficulty, maxDifficulty);

            if (!_cursorsByFilter.TryGetValue(filterKey, out int cursor))
                cursor = 0;

            if (cursor >= matches.Count)
                cursor = 0;

            int pickIndex = matches[cursor];
            cursor++;

            _cursorsByFilter[filterKey] = cursor;

            return _questions[pickIndex];
        }

        /// <summary>
        /// Compatibility with older room code calling GetTestQuestion().
        /// Returns an unfiltered next question.
        /// </summary>
        [Server]
        public QuestionData GetTestQuestion()
        {
            return GetNextQuestion();
        }

        [Server]
        private QuestionData GetAnyQuestion()
        {
            // Simple cycling across all questions (reuse a fixed filter key)
            const int key = 0;
            if (!_cursorsByFilter.TryGetValue(key, out int cursor))
                cursor = 0;

            if (cursor >= _questions.Count)
                cursor = 0;

            var q = _questions[cursor];
            cursor++;

            _cursorsByFilter[key] = cursor;
            return q;
        }

        [Server]
        private QuestionData GetFallbackQuestion()
        {
            return new QuestionData
            {
                prompt = "Kwiztime fallback: 2 + 2 = ?",
                answers = new[] { "3", "4", "5", "22" },
                correctIndex = 1,
                timeLimit = defaultTimeLimitEasy,
                category = QuestionCategory.General,
                difficulty = QuestionDifficulty.Easy
            };
        }

        [Server]
        private void BuildTestQuestions()
        {
            _questions.Clear();

            // All-ages, safe prototype content. Categories + difficulty included.

            Add(QuestionCategory.Animals, QuestionDifficulty.Easy,
                "Which animal is a marsupial?",
                "Koala", "Penguin", "Giraffe", "Gorilla", correct: 0);

            Add(QuestionCategory.Words, QuestionDifficulty.Easy,
                "What is the opposite of 'hot'?",
                "Warm", "Cold", "Fast", "Bright", correct: 1);

            Add(QuestionCategory.Shapes, QuestionDifficulty.Easy,
                "Which shape has three sides?",
                "Triangle", "Square", "Circle", "Rectangle", correct: 0);

            Add(QuestionCategory.Science, QuestionDifficulty.Easy,
                "What is H2O commonly known as?",
                "Salt", "Water", "Oxygen", "Sugar", correct: 1);

            Add(QuestionCategory.Geography, QuestionDifficulty.Medium,
                "Which is the largest ocean on Earth?",
                "Atlantic", "Indian", "Arctic", "Pacific", correct: 3);

            Add(QuestionCategory.Science, QuestionDifficulty.Medium,
                "Which planet is known as the Red Planet?",
                "Venus", "Mars", "Jupiter", "Mercury", correct: 1);

            Add(QuestionCategory.Food, QuestionDifficulty.Easy,
                "Which one is a fruit?",
                "Carrot", "Potato", "Apple", "Onion", correct: 2);

            Add(QuestionCategory.Food, QuestionDifficulty.Easy,
                "What do bees make?",
                "Milk", "Honey", "Bread", "Cheese", correct: 1);

            Add(QuestionCategory.Music, QuestionDifficulty.Easy,
                "Which is a musical instrument?",
                "Hammer", "Violin", "Spoon", "Brush", correct: 1);

            Add(QuestionCategory.General, QuestionDifficulty.Easy,
                "How many days are in a week?",
                "5", "6", "7", "8", correct: 2);

            Add(QuestionCategory.Animals, QuestionDifficulty.Medium,
                "Which animal is known for black and white stripes?",
                "Zebra", "Lion", "Panda", "Elephant", correct: 0);

            Add(QuestionCategory.Geography, QuestionDifficulty.Hard,
                "Which country is famous for the fjords?",
                "Spain", "Norway", "Egypt", "Mexico", correct: 1);

            Add(QuestionCategory.Science, QuestionDifficulty.Hard,
                "Which gas do plants absorb from the air?",
                "Oxygen", "Carbon dioxide", "Helium", "Nitrogen", correct: 1);

            Add(QuestionCategory.General, QuestionDifficulty.Medium,
                "Which one is used to write?",
                "Pencil", "Plate", "Socks", "Soap", correct: 0);

            Add(QuestionCategory.Animals, QuestionDifficulty.Hard,
                "Which animal is known for echolocation?",
                "Bat", "Camel", "Rabbit", "Horse", correct: 0);
        }

        [Server]
        private void Add(
            QuestionCategory category,
            QuestionDifficulty difficulty,
            string prompt,
            string a0, string a1, string a2, string a3,
            int correct)
        {
            float timeLimit = difficulty switch
            {
                QuestionDifficulty.Easy => defaultTimeLimitEasy,
                QuestionDifficulty.Medium => defaultTimeLimitMedium,
                QuestionDifficulty.Hard => defaultTimeLimitHard,
                _ => defaultTimeLimitEasy
            };

            _questions.Add(new QuestionData
            {
                prompt = prompt,
                answers = new[] { a0, a1, a2, a3 },
                correctIndex = Mathf.Clamp(correct, 0, 3),
                timeLimit = timeLimit,
                category = category,
                difficulty = difficulty
            });
        }

        private static int MakeFilterKey(
            QuestionCategory? category,
            QuestionDifficulty? minD,
            QuestionDifficulty? maxD)
        {
            // Small stable key: category in low bits, difficulties in higher bits
            int c = category.HasValue ? (int)category.Value + 1 : 0;
            int min = minD.HasValue ? (int)minD.Value : 0;
            int max = maxD.HasValue ? (int)maxD.Value : 0;

            return c | (min << 8) | (max << 16);
        }

        private static void Shuffle<T>(IList<T> list)
        {
            // Fisher–Yates shuffle
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}