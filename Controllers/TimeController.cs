using System;
using System.IO;
using System.Text.Json;
using FantasyKingdom.Services;

namespace FantasyKingdom.Controllers;

public class TimeController : IDisposable
{
    // Константы
    private const int SecondsPerGameHour = 3600;
    private const int SecondsPerGameDay = 86400; // 24 игровых часа
    
    // События
    public Action<int> OnHourPassed { get; set; } // Вызывается при каждом новом часе (параметр - номер часа)
    public Action<int> OnDayPassed { get; set; }  // Вызывается при каждом новом дне (параметр - номер дня)
    
    // Свойства
    public double CurrentGameTimeSeconds { get; private set; }
    public double TimeScale { get; set; } = 60.0; // 1 реальная секунда = 1 игровая минута
    
    public int CurrentHour => (int)(CurrentGameTimeSeconds % SecondsPerGameDay) / SecondsPerGameHour;
    public int CurrentDay => (int)(CurrentGameTimeSeconds / SecondsPerGameDay);
    public double SecondsUntilNextDay => SecondsPerGameDay - (CurrentGameTimeSeconds % SecondsPerGameDay);
    
    private DateTime _lastUpdateTime;
    private readonly string _saveFilePath;
    private readonly System.Timers.Timer _updateTimer;
    
    public TimeController()
    {
        _saveFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FantasyKingdom",
            "game_time.json");
        
        Logger.Log($"Сохранения времени по пути: {_saveFilePath}");
        
        Directory.CreateDirectory(Path.GetDirectoryName(_saveFilePath));
        
        _updateTimer = new System.Timers.Timer(1000);
        _updateTimer.Elapsed += (s, e) => Update();
        _updateTimer.AutoReset = true;
        
        LoadTime();
        _updateTimer.Start();
    }
    
    public void Update()
    {
        var now = DateTime.Now;
        var elapsedRealTime = now - _lastUpdateTime;
        _lastUpdateTime = now;
        
        if (elapsedRealTime.TotalSeconds <= 0)
            return;
        
        // Сохраняем предыдущее состояние
        var previousGameTime = CurrentGameTimeSeconds;
        var previousDay = CurrentDay;
        var previousHour = CurrentHour;
        
        // Обновляем игровое время
        CurrentGameTimeSeconds += elapsedRealTime.TotalSeconds * TimeScale;
        
        // Проверяем, сколько полных часов прошло
        var hoursPassed = (int)(CurrentGameTimeSeconds / SecondsPerGameHour) - (int)(previousGameTime / SecondsPerGameHour);
        
        // Если прошли целые часы, вызываем события
        if (hoursPassed > 0)
        {
            var currentFullHour = (int)(CurrentGameTimeSeconds / SecondsPerGameHour);
            var firstHourToNotify = (int)(previousGameTime / SecondsPerGameHour) + 1;
            
            for (var hour = firstHourToNotify; hour <= currentFullHour; hour++)
            {
                var hourInDay = hour % 24;
                var day = hour / 24;
                
                OnHourPassed?.Invoke(hourInDay);
                
                // Если это первый час нового дня
                if (hourInDay == 0 && hour > 0)
                {
                    OnDayPassed?.Invoke(day);
                }
            }
        }
        
        // Сохраняем каждую минуту игрового времени или при смене дня/часа
        if (hoursPassed > 0 || (int)(CurrentGameTimeSeconds / 60) > (int)(previousGameTime / 60))
        {
            SaveTime();
        }
    }
    
    // Переход к следующему игровому дню
    public void AdvanceToNextDay()
    {
        var currentDay = CurrentDay;
        var secondsToAdd = SecondsUntilNextDay;
        
        CurrentGameTimeSeconds += secondsToAdd;
        _lastUpdateTime = DateTime.Now;
        
        // Вызываем события для всех оставшихся часов текущего дня
        for (var hour = CurrentHour + 1; hour < 24; hour++)
        {
            OnHourPassed?.Invoke(hour);
        }
        
        // Вызываем событие нового дня (0 час нового дня)
        OnHourPassed?.Invoke(0);
        OnDayPassed?.Invoke(currentDay + 1);
        
        SaveTime();
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
            CurrentGameTimeSeconds = 0;
            _lastUpdateTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка при очистке сохранения: {ex.Message}");
        }
    }
    
    public void SetGameTime(double gameTimeSeconds)
    {
        CurrentGameTimeSeconds = gameTimeSeconds;
        _lastUpdateTime = DateTime.Now;
        SaveTime();
    }
    
    public void SetTimeScale(double gameSecondsPerRealSecond)
    {
        TimeScale = gameSecondsPerRealSecond;
        SaveTime();
    }
    
    private void SaveTime()
    {
        try
        {
            var saveData = new TimeSaveData
            {
                GameTimeSeconds = CurrentGameTimeSeconds,
                TimeScale = TimeScale,
                LastSaveRealTime = DateTime.Now,
                NextDayInRealSeconds = SecondsUntilNextDay / TimeScale
            };
            
            File.WriteAllText(_saveFilePath, JsonSerializer.Serialize(saveData));
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка сохранения времени: {ex.Message}");
        }
    }
    
    private void LoadTime()
    {
        if (!File.Exists(_saveFilePath))
        {
            CurrentGameTimeSeconds = 0;
            _lastUpdateTime = DateTime.Now;
            return;
        }
        
        try
        {
            var saveData = JsonSerializer.Deserialize<TimeSaveData>(File.ReadAllText(_saveFilePath));
            TimeScale = saveData.TimeScale;
            
            var realTimeSinceSave = DateTime.Now - saveData.LastSaveRealTime;
            
            // Если с момента сохранения прошло больше времени, чем оставалось до следующего дня
            if (realTimeSinceSave.TotalSeconds >= saveData.NextDayInRealSeconds)
            {
                // Вычисляем сколько полных дней прошло
                var fullDaysPassed = (int)((realTimeSinceSave.TotalSeconds - saveData.NextDayInRealSeconds) * TimeScale / SecondsPerGameDay) + 1;
                CurrentGameTimeSeconds = saveData.GameTimeSeconds + (fullDaysPassed * SecondsPerGameDay);
                
                // Корректируем с учетом оставшегося времени
                var remainingRealTime = realTimeSinceSave.TotalSeconds - saveData.NextDayInRealSeconds;
                remainingRealTime %= (SecondsPerGameDay / TimeScale);
                CurrentGameTimeSeconds += remainingRealTime * TimeScale;
            }
            else
            {
                // Просто добавляем прошедшее время
                CurrentGameTimeSeconds = saveData.GameTimeSeconds + (realTimeSinceSave.TotalSeconds * TimeScale);
            }
            
            _lastUpdateTime = DateTime.Now;
            
            Logger.Log($"Время загружено. Текущий день: {CurrentDay}, час: {CurrentHour}, до следующего дня: {SecondsUntilNextDay/3600:F1} часов");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка загрузки времени: {ex.Message}");
            CurrentGameTimeSeconds = 0;
            _lastUpdateTime = DateTime.Now;
        }
    }
    
    public void Dispose()
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
        SaveTime();
    }
    
    private class TimeSaveData
    {
        public double GameTimeSeconds { get; set; }
        public double TimeScale { get; set; }
        public DateTime LastSaveRealTime { get; set; }
        public double NextDayInRealSeconds { get; set; } // Реальное время до следующего дня
    }
}