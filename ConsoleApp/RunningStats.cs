namespace ConsoleApp;

public sealed class RunningStats
{
    private float _min;
    private float _max;
    private float _temperatureSum;
    private int _numTemperatures;

    public void AddTemperature(float temperature)
    {
        if (temperature < _min) _min = temperature;
        if (temperature > _max) _max = temperature;

        _temperatureSum += temperature;
        _numTemperatures++;
    }

    public override string ToString()
    {
        var avg = _temperatureSum / _numTemperatures;
        return $"{_min:F1}/{avg:F1}/{_max:F1}";
    }
}