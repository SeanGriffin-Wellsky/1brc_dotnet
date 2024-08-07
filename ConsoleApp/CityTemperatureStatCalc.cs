namespace ConsoleApp;

public sealed class RunningStats
{
    private int _min;
    private int _max;
    private int _temperatureSum;
    private int _numTemperatures;

    public float Min { get; private set; }
    public float Max { get; private set; }
    public float TemperatureAvg { get; private set; }

    public void AddTemperature(int temperature)
    {
        if (temperature < _min) _min = temperature;
        if (temperature > _max) _max = temperature;

        _temperatureSum += temperature;
        _numTemperatures++;
    }

    public void Merge(RunningStats other)
    {
        if (other._min < _min) _min = other._min;
        if (other._max > _max) _max = other._max;

        _temperatureSum += other._temperatureSum;
        _numTemperatures += other._numTemperatures;
    }

    public RunningStats FinalizeStats()
    {
        Min = _min / 10.0f;
        Max = _max / 10.0f;
        TemperatureAvg = _temperatureSum / 10.0f / _numTemperatures;
        return this;
    }
}

public sealed class CityTemperatureStatCalc(int capacity)
{
    private readonly RunningStatsDictionary _stats = new(capacity);

    public int NumCities => _stats.Count;

    public void AddCityTemp(CityTemp cityTemp)
    {
        var cityName = cityTemp.City;
        var hashCode = _stats.GetHashCode(cityName);

        if (!_stats.TryGetValue(hashCode, cityName, out var runningStats))
        {
            runningStats = new RunningStats();
            _stats.Add(hashCode, cityName, runningStats);
        }

        runningStats.AddTemperature(cityTemp.Temperature);
    }

    public void Merge(CityTemperatureStatCalc other)
    {
        foreach (var otherKv in other._stats)
        {
            var keySpan = otherKv.Key.Span;
            var hashCode = _stats.GetHashCode(keySpan);

            if (this._stats.TryGetValue(hashCode, keySpan, out var thisRunningStats))
            {
                thisRunningStats.Merge(otherKv.Value);
            }
            else
            {
                this._stats.Add(hashCode, keySpan, otherKv.Value);
            }
        }
    }

    public SortedDictionary<string, RunningStats> FinalizeStats()
    {
        return _stats.ToFinalDictionary();
    }
}
