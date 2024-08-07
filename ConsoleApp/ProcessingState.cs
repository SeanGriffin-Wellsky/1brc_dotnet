using Microsoft.Win32.SafeHandles;

namespace ConsoleApp;

public readonly record struct ProcessingState(int ExpectedCityCount, SafeFileHandle FileHandle, Partition Block);