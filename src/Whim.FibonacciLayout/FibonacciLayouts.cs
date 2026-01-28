namespace Whim.FibonacciLayout;

/// <summary>
/// Methods to create Fibonacci layout engines.
/// </summary>
public static class FibonacciLayouts
{
	/// <summary>
	/// Creates a Fibonacci spiral layout where windows are arranged in a clockwise spiral pattern.
	/// Each new window takes approximately half of the remaining space.
	/// </summary>
	/// <example>
	/// Usage:
	/// <code>
	/// FibonacciLayoutPlugin fibonacciLayoutPlugin = new(context);
	/// context.PluginManager.AddPlugin(fibonacciLayoutPlugin);
	///
	/// context.Store.Dispatch(
	/// 	new SetCreateLayoutEnginesTransform(
	/// 		() => new CreateLeafLayoutEngine[]
	/// 		{
	/// 			(id) => FibonacciLayouts.CreateFibonacciLayout(context, fibonacciLayoutPlugin, id)
	/// 		}
	/// 	)
	/// );
	/// </code>
	///
	/// Layout with 5 windows:
	/// <code>
	/// ┌──────────┬──────────┐
	/// │          │    2     │
	/// │    1     ├─────┬────┤
	/// │          │  5  │ 3  │
	/// │          ├─────┤    │
	/// │          │  4  │    │
	/// └──────────┴─────┴────┘
	/// </code>
	/// </example>
	/// <param name="plugin">The Fibonacci layout plugin.</param>
	/// <param name="identity">The identity of the layout engine.</param>
	/// <param name="splitRatio">
	/// The ratio of space given to the first window in each split (0.1 to 0.9).
	/// Defaults to 0.5.
	/// </param>
	/// <returns>A new Fibonacci layout engine.</returns>
	public static ILayoutEngine CreateFibonacciLayout(
		IFibonacciLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		double splitRatio = 0.5
	) =>
		new FibonacciLayoutEngine(identity, plugin, clockwise: true, FibonacciLayoutMode.Spiral, splitRatio)
		{
			Name = "Fibonacci",
		};

	/// <summary>
	/// Creates a counter-clockwise Fibonacci spiral layout.
	/// </summary>
	/// <example>
	/// Usage:
	/// <code>
	/// FibonacciLayoutPlugin fibonacciLayoutPlugin = new(context);
	/// context.PluginManager.AddPlugin(fibonacciLayoutPlugin);
	///
	/// context.Store.Dispatch(
	/// 	new SetCreateLayoutEnginesTransform(
	/// 		() => new CreateLeafLayoutEngine[]
	/// 		{
	/// 			(id) => FibonacciLayouts.CreateReverseFibonacciLayout(context, fibonacciLayoutPlugin, id)
	/// 		}
	/// 	)
	/// );
	/// </code>
	///
	/// Layout with 5 windows (counter-clockwise):
	/// <code>
	/// ┌──────────┬──────────┐
	/// │          │    5     │
	/// │    1     ├──────────┤
	/// │          │    4     │
	/// │          ├────┬─────┤
	/// │          │ 3  │  2  │
	/// └──────────┴────┴─────┘
	/// </code>
	/// </example>
	/// <param name="plugin">The Fibonacci layout plugin.</param>
	/// <param name="identity">The identity of the layout engine.</param>
	/// <param name="splitRatio">
	/// The ratio of space given to the first window in each split (0.1 to 0.9).
	/// Defaults to 0.5.
	/// </param>
	/// <returns>A new reverse Fibonacci layout engine.</returns>
	public static ILayoutEngine CreateReverseFibonacciLayout(
		IFibonacciLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		double splitRatio = 0.5
	) =>
		new FibonacciLayoutEngine(identity, plugin, clockwise: false, FibonacciLayoutMode.Spiral, splitRatio)
		{
			Name = "Fibonacci (Reverse)",
		};

	/// <summary>
	/// Creates a Fibonacci layout optimized for ultrawide displays.
	/// The first window takes the top portion, and remaining windows are arranged
	/// horizontally in the bottom portion. This prevents windows from becoming too wide.
	/// </summary>
	/// <example>
	/// Layout with 4 windows on ultrawide:
	/// <code>
	/// ┌─────────────────────────────────────┐
	/// │                 1                   │
	/// ├───────────┬───────────┬─────────────┤
	/// │     2     │     3     │      4      │
	/// └───────────┴───────────┴─────────────┘
	/// </code>
	/// </example>
	/// <param name="plugin">The Fibonacci layout plugin.</param>
	/// <param name="identity">The identity of the layout engine.</param>
	/// <param name="splitRatio">
	/// The ratio of space given to the primary (top) window (0.1 to 0.9).
	/// Defaults to 0.5. Use higher values like 0.6-0.7 to give more space to the main window.
	/// </param>
	/// <returns>A new ultrawide Fibonacci layout engine.</returns>
	public static ILayoutEngine CreateUltrawideFibonacciLayout(
		IFibonacciLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		double splitRatio = 0.5
	) =>
		new FibonacciLayoutEngine(identity, plugin, clockwise: true, FibonacciLayoutMode.Ultrawide, splitRatio)
		{
			Name = "Fibonacci (Ultrawide)",
		};

	/// <summary>
	/// Creates a Fibonacci layout with columns mode.
	/// The first window takes the left portion, and remaining windows are stacked
	/// vertically in the right portion. This is similar to a master-stack layout.
	/// </summary>
	/// <example>
	/// Layout with 4 windows:
	/// <code>
	/// ┌──────────────┬───────────────┐
	/// │              │       2       │
	/// │              ├───────────────┤
	/// │      1       │       3       │
	/// │              ├───────────────┤
	/// │              │       4       │
	/// └──────────────┴───────────────┘
	/// </code>
	/// </example>
	/// <param name="plugin">The Fibonacci layout plugin.</param>
	/// <param name="identity">The identity of the layout engine.</param>
	/// <param name="splitRatio">
	/// The ratio of space given to the primary (left) window (0.1 to 0.9).
	/// Defaults to 0.5.
	/// </param>
	/// <returns>A new columns Fibonacci layout engine.</returns>
	public static ILayoutEngine CreateColumnsFibonacciLayout(
		IFibonacciLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		double splitRatio = 0.5
	) =>
		new FibonacciLayoutEngine(identity, plugin, clockwise: true, FibonacciLayoutMode.Columns, splitRatio)
		{
			Name = "Fibonacci (Columns)",
		};
}
