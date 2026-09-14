using System;
using System.Collections.Generic;
using System.Linq;
using Surexs.DanceOff.Data;
using UnityEngine;

namespace Surexs.DanceOff.Rhythm
{
    public sealed class ChartManager : MonoBehaviour
    {
        [SerializeField] private TextAsset chartAsset;

        private readonly List<ChartEvent> events = new List<ChartEvent>();

        public string SongId { get; private set; } = string.Empty;
        public float VisualSpeed { get; private set; } = 1f;
        public IReadOnlyList<ChartEvent> Events => events;

        public void Configure(TextAsset asset)
        {
            chartAsset = asset;
        }

        public bool LoadChart()
        {
            events.Clear();
            SongId = string.Empty;
            VisualSpeed = 1f;

            if (chartAsset == null)
            {
                Debug.LogError("[ChartManager] No hay un chart JSON asignado.", this);
                return false;
            }

            ChartJson chart;
            try
            {
                chart = JsonUtility.FromJson<ChartJson>(chartAsset.text);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[ChartManager] JSON malformado en '{chartAsset.name}': {exception.Message}", this);
                return false;
            }

            if (chart == null)
            {
                Debug.LogError($"[ChartManager] No se pudo interpretar '{chartAsset.name}'.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(chart.song))
            {
                Debug.LogError("[ChartManager] El campo 'song' es obligatorio.", this);
                return false;
            }

            if (!IsFinite(chart.visualSpeed) || chart.visualSpeed <= 0f)
            {
                Debug.LogError("[ChartManager] 'visualSpeed' debe ser un número finito mayor que cero.", this);
                return false;
            }

            if (chart.tiles == null || chart.tiles.Length == 0)
            {
                Debug.LogError("[ChartManager] El chart debe contener al menos un tile.", this);
                return false;
            }

            var validatedEvents = new List<ChartEvent>(chart.tiles.Length);
            for (var index = 0; index < chart.tiles.Length; index++)
            {
                var tile = chart.tiles[index];
                if (tile == null)
                {
                    Debug.LogError($"[ChartManager] Tile {index} es nulo.", this);
                    return false;
                }

                if (!IsFinite(tile.time) || tile.time < 0f)
                {
                    Debug.LogError($"[ChartManager] Tile {index} tiene un tiempo inválido: {tile.time}.", this);
                    return false;
                }

                if (!Enum.TryParse(tile.direction, true, out RhythmDirection direction))
                {
                    Debug.LogError($"[ChartManager] Tile {index} tiene una dirección inválida: '{tile.direction}'.", this);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(tile.pose))
                {
                    Debug.LogError($"[ChartManager] Tile {index} no tiene pose.", this);
                    return false;
                }

                validatedEvents.Add(new ChartEvent(tile.time, direction, tile.pose.Trim()));
            }

            var wasOrdered = validatedEvents.Zip(validatedEvents.Skip(1), (first, second) => first.Time <= second.Time).All(value => value);
            if (!wasOrdered)
            {
                Debug.LogWarning("[ChartManager] Los eventos no estaban ordenados; se ordenaron por timestamp.", this);
            }

            events.AddRange(validatedEvents.OrderBy(chartEvent => chartEvent.Time));
            SongId = chart.song.Trim();
            VisualSpeed = chart.visualSpeed;

            Debug.Log($"[ChartManager] Chart '{SongId}' cargado: {events.Count} eventos, velocidad visual {VisualSpeed:F2}.", this);
            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
