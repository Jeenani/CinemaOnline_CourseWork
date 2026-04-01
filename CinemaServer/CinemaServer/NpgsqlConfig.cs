namespace CinemaServer;

/// <summary>
/// Статический класс для инициализации Npgsql настроек.
/// Должен быть вызван ДО создания любых подключений к БД.
/// </summary>
public static class NpgsqlConfig
{
    private static bool _initialized = false;
    
    static NpgsqlConfig()
    {
        Initialize();
    }
    
    public static void Initialize()
    {
        if (_initialized) return;
        
        // Включаем legacy timestamp behavior для совместимости с timestamp without time zone
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        
        _initialized = true;
    }
}
