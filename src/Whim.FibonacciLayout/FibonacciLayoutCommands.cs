namespace Whim.FibonacciLayout;

/// <summary>
/// Commands for the <see cref="FibonacciLayoutPlugin"/>.
/// </summary>
public class FibonacciLayoutCommands : PluginCommands
{
	/// <summary>
	/// Creates a new instance of the Fibonacci layout commands.
	/// </summary>
	/// <param name="plugin"></param>
	public FibonacciLayoutCommands(IFibonacciLayoutPlugin plugin)
		: base(plugin.Name)
	{
		_ = Add(
			identifier: "reverse_spiral",
			title: "Reverse Fibonacci spiral direction",
			callback: () => ((FibonacciLayoutPlugin)plugin).ReverseSpiral()
		);

		_ = Add(
			identifier: "cycle_layout_mode",
			title: "Cycle Fibonacci layout mode (Spiral/Ultrawide/Columns)",
			callback: () => ((FibonacciLayoutPlugin)plugin).CycleLayoutMode()
		);
	}
}
