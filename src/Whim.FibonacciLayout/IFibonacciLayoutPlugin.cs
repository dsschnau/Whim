namespace Whim.FibonacciLayout;

/// <summary>
/// Plugin for the Fibonacci layout engine.
/// </summary>
public interface IFibonacciLayoutPlugin : IPlugin
{
	/// <summary>
	/// The name of the action to reverse the spiral direction.
	/// </summary>
	string ReverseSpiralActionName { get; }

	/// <summary>
	/// The name of the action to cycle through layout modes (Spiral → Ultrawide → Columns).
	/// </summary>
	string CycleLayoutModeActionName { get; }
}
