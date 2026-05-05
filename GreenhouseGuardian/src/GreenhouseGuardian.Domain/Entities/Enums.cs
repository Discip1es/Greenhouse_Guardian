namespace GreenhouseGuardian.Domain.Entities;

public enum SensorType
{
    TemperatureAir,      // °C
    HumidityAir,         // %
    SoilMoisture,        // %
    Luminosity,          // lux
    CO2,                 // ppm
    pH,                  // pH units
    EC                   // mS/cm
}

public enum ActuatorType
{
    IrrigationValve,     // Полив
    VentilationFan,      // Вентиляция
    GrowLight,           // Досветка
    Heater,              // Обогрев
    ServoDamper,         // Сервопривод заслонки
    MistingSystem        // Система туманообразования
}

public enum UserRole
{
    Administrator,  // Полный доступ
    Agronomist,     // Настройка профилей, сценариев, порогов
    Operator,       // Просмотр дашбордов, ручное управление
    Observer        // Только чтение (инвестор)
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public enum ActuatorState
{
    Off,
    On,
    InProgress,
    Error
}
