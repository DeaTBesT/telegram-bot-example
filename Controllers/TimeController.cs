using System;
using System.IO;
using System.Text.Json;
using FantasyKingdom.Services;

namespace FantasyKingdom.Controllers;

public class TimeController : IDisposable
{
    // События
    public static Action OnTick { get; set; } // Вызывается при наступлении нового игрового дня (текущий час)
    public static Action<int> OnMissedTicks { get; set; } // Вызывается при запуске, если пропущены дни (количество пропущенных дней)

    // Настройки
    public bool IsPaused { get; set; }

    private DateTime _lastRealTime;
    private DateTime _lastGameDayTime; // Время последнего игрового дня
    private readonly string _saveFilePath;
    private readonly System.Timers.Timer _checkTimer;

    public TimeController()
    {
        _saveFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FantasyKingdom",
            "real_time_tracker.json");

        Logger.Log($"Сохранения времени по пути: {_saveFilePath}");

        Directory.CreateDirectory(Path.GetDirectoryName(_saveFilePath));

        _checkTimer = new System.Timers.Timer(60000); // Проверка каждую минуту
        _checkTimer.Elapsed += (s, e) => CheckTime();
        _checkTimer.AutoReset = true;

        LoadTime();
        _checkTimer.Start();
    }

    // Принудительный переход на следующий игровой день без изменения реального времени
    public void AdvanceToNextDay()
    {
        _lastGameDayTime = _lastGameDayTime.AddHours(1);
        OnTick?.Invoke();
        SaveTime();

        Logger.Log($"Принудительный переход на новый игровой день. Текущее время: {_lastGameDayTime}");
    }

    private void CheckTime()
    {
        if (IsPaused) return;

        var now = DateTime.Now;
        var lastHour = _lastRealTime.Hour;
        var currentHour = now.Hour;

        // Если перешли на новый час (новый игровой день)
        if (currentHour != lastHour)
        {
            // Если это непрерывный переход (без пропуска часов)
            if (currentHour == (lastHour + 1) % 24 || (lastHour == 23 && currentHour == 0))
            {
                _lastGameDayTime = now;
                OnTick?.Invoke();
            }
            else
            {
                // Вычисляем сколько дней (часов) пропущено
                int missedDays;
                if (currentHour > lastHour)
                {
                    missedDays = currentHour - lastHour;
                }
                else
                {
                    missedDays = (24 - lastHour) + currentHour;
                }

                _lastGameDayTime = now;
                OnMissedTicks?.Invoke(missedDays);
                OnTick?.Invoke();
            }

            _lastRealTime = now;
            SaveTime();
        }
    }

    private void LoadTime()
    {
        if (!File.Exists(_saveFilePath))
        {
            _lastRealTime = DateTime.Now;
            _lastGameDayTime = DateTime.Now;
            Logger.Log("Сохранение времени не найдено, начинаем новый отсчет");
            return;
        }

        try
        {
            var saveData = JsonSerializer.Deserialize<TimeSaveData>(File.ReadAllText(_saveFilePath));
            _lastRealTime = saveData.LastRealTime;
            _lastGameDayTime = saveData.LastGameDayTime;

            var now = DateTime.Now;
            var timePassed = now - _lastRealTime;

            // Если прошло больше часа
            if (timePassed.TotalHours >= 1)
            {
                int fullHoursPassed = (int)timePassed.TotalHours;

                // Если прошло больше суток, вызываем OnMissedTicks один раз с общим количеством
                if (fullHoursPassed >= 24)
                {
                    int fullDaysPassed = fullHoursPassed / 24;
                    OnMissedTicks?.Invoke(fullDaysPassed);
                }
                else
                {
                    // Если пропущено несколько часов (но меньше суток)
                    OnMissedTicks?.Invoke(fullHoursPassed);
                }
            }

            // Вызываем OnTick для текущего часа, если он изменился
            if (now.Hour != _lastRealTime.Hour)
            {
                OnTick?.Invoke();
            }

            _lastRealTime = now;
            _lastGameDayTime = now;
            Logger.Log($"Время загружено. Последний отсчет: {saveData.LastRealTime}, текущий час: {now.Hour}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка загрузки времени: {ex.Message}");
            _lastRealTime = DateTime.Now;
            _lastGameDayTime = DateTime.Now;
        }
    }

    private void SaveTime()
    {
        try
        {
            var saveData = new TimeSaveData
            {
                LastRealTime = _lastRealTime,
                LastGameDayTime = _lastGameDayTime
            };

            File.WriteAllText(_saveFilePath, JsonSerializer.Serialize(saveData));
            Logger.Log($"Время сохранено. Реальное: {_lastRealTime}, Игровое: {_lastGameDayTime}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка сохранения времени: {ex.Message}");
        }
    }

    // Очистка сохраненного времени
    public void ClearSavedTime()
    {
        try
        {
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
            }
            _lastRealTime = DateTime.Now;
            _lastGameDayTime = DateTime.Now;
            Logger.Log("Сохранение времени очищено");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка при очистке сохранения: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _checkTimer?.Stop();
        _checkTimer?.Dispose();
        SaveTime();
    }

    private class TimeSaveData
    {
        public DateTime LastRealTime { get; set; }
        public DateTime LastGameDayTime { get; set; }
    }
}