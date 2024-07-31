namespace ConsoleApp;

public sealed class RunningStats
{
    private int _min;
    private int _max;
    private int _temperatureSum;
    private int _numTemperatures;

    public void AddTemperature(int temperature)
    {
        if (temperature < _min) _min = temperature;
        if (temperature > _max) _max = temperature;

        _temperatureSum += temperature;
        _numTemperatures++;
    }

    public override string ToString()
    {
        var min = _min / 10.0f;
        var max = _max / 10.0f;
        var avg = _temperatureSum / 10.0f / _numTemperatures;
        return $"{min:F1}/{avg:F1}/{max:F1}";
    }
}