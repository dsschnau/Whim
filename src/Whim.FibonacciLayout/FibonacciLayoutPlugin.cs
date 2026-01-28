using System.Text.Json;

namespace Whim.FibonacciLayout;

/// <inheritdoc />
/// <summary>
/// Create a new <see cref="FibonacciLayoutPlugin"/>.
/// </summary>
/// <param name="context"></param>
public class FibonacciLayoutPlugin(IContext context) : IFibonacciLayoutPlugin
{
	private readonly IContext _context = context;

	/// <summary>
	/// <c>whim.fibonacci_layout</c>
	/// </summary>
	public string Name => "whim.fibonacci_layout";

	/// <inheritdoc />
	public string ReverseSpiralActionName => $"{Name}.reverse_spiral";

	/// <inheritdoc />
	public string CycleLayoutModeActionName => $"{Name}.cycle_layout_mode";

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new FibonacciLayoutCommands(this);

	/// <inheritdoc />
	public void PreInitialize() { }

	/// <inheritdoc />
	public void PostInitialize() { }

	/// <inheritdoc />
	public void LoadState(JsonElement state) { }

	/// <inheritdoc />
	public JsonElement? SaveState() => null;

	/// <summary>
	/// Reverses the spiral direction of the Fibonacci layout on the active workspace.
	/// </summary>
	public void ReverseSpiral()
	{
		IWorkspace workspace = _context.Store.Pick(Pickers.PickActiveWorkspace());
		LayoutEngineCustomAction action = new() { Name = ReverseSpiralActionName, Window = null };
		_context.Store.Dispatch(new LayoutEngineCustomActionTransform(workspace.Id, action));
	}

	/// <summary>
	/// Cycles through the Fibonacci layout modes (Spiral → Ultrawide → Columns) on the active workspace.
	/// </summary>
	public void CycleLayoutMode()
	{
		IWorkspace workspace = _context.Store.Pick(Pickers.PickActiveWorkspace());
		LayoutEngineCustomAction action = new() { Name = CycleLayoutModeActionName, Window = null };
		_context.Store.Dispatch(new LayoutEngineCustomActionTransform(workspace.Id, action));
	}
}
