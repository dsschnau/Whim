using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim.FibonacciLayout;

/// <summary>
/// The direction of the next split in the Fibonacci spiral.
/// </summary>
internal enum SplitDirection
{
	/// <summary>
	/// Split to the right.
	/// </summary>
	Right,

	/// <summary>
	/// Split downward.
	/// </summary>
	Down,

	/// <summary>
	/// Split to the left.
	/// </summary>
	Left,

	/// <summary>
	/// Split upward.
	/// </summary>
	Up,
}

/// <summary>
/// Specifies how the Fibonacci layout should behave, particularly for different monitor aspect ratios.
/// </summary>
public enum FibonacciLayoutMode
{
	/// <summary>
	/// Standard Fibonacci spiral pattern (right → down → left → up, repeating).
	/// Works well on standard monitors.
	/// </summary>
	Spiral,

	/// <summary>
	/// Optimized for ultrawide displays. Creates rows first (top/bottom splits), then
	/// stacks windows horizontally within the bottom row. This makes windows narrower,
	/// which is better when you have excessive horizontal space.
	/// <code>
	/// 2 windows:           3 windows:           4+ windows:
	/// ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
	/// │        1        │  │        1        │  │        1        │
	/// ├─────────────────┤  ├────────┬────────┤  ├────────┬────────┤
	/// │        2        │  │   2    │   3    │  │   2    │ 3,4... │
	/// └─────────────────┘  └────────┴────────┘  └────────┴────────┘
	/// </code>
	/// </summary>
	Ultrawide,

	/// <summary>
	/// Creates columns first (left/right splits), then stacks windows vertically
	/// within the rightmost column. Useful for tall or portrait monitors where
	/// you want to maximize horizontal usage.
	/// <code>
	/// 2 windows:           3 windows:           4+ windows:
	/// ┌────────┬────────┐  ┌────────┬────────┐  ┌────────┬────────┐
	/// │        │        │  │        │   2    │  │        │   2    │
	/// │   1    │   2    │  │   1    ├────────┤  │   1    ├────────┤
	/// │        │        │  │        │   3    │  │        │ 3,4... │
	/// └────────┴────────┘  └────────┴────────┘  └────────┴────────┘
	/// </code>
	/// </summary>
	Columns,
}

/// <summary>
/// A layout engine that arranges windows in a Fibonacci spiral pattern.
/// Each new window takes approximately 50% of the remaining space, spiraling inward.
/// </summary>
/// <remarks>
/// <code>
/// 1 window:
/// ┌─────────────────────┐
/// │                     │
/// │          1          │
/// │                     │
/// └─────────────────────┘
///
/// 2 windows:
/// ┌──────────┬──────────┐
/// │          │          │
/// │    1     │    2     │
/// │          │          │
/// └──────────┴──────────┘
///
/// 3 windows:
/// ┌──────────┬──────────┐
/// │          │    2     │
/// │    1     ├──────────┤
/// │          │    3     │
/// └──────────┴──────────┘
///
/// 4 windows:
/// ┌──────────┬──────────┐
/// │          │    2     │
/// │    1     ├─────┬────┤
/// │          │  4  │ 3  │
/// └──────────┴─────┴────┘
///
/// 5 windows:
/// ┌──────────┬──────────┐
/// │          │    2     │
/// │    1     ├─────┬────┤
/// │          │  5  │ 3  │
/// │          ├─────┤    │
/// │          │  4  │    │
/// └──────────┴─────┴────┘
/// </code>
/// </remarks>
public record FibonacciLayoutEngine : ILayoutEngine
{
	private readonly ImmutableList<IWindow> _windows;
	private readonly bool _clockwise;
	private readonly IFibonacciLayoutPlugin _plugin;
	private readonly FibonacciLayoutMode _layoutMode;
	private readonly double _splitRatio;

	/// <inheritdoc/>
	public string Name { get; init; } = "Fibonacci";

	/// <inheritdoc/>
	public int Count => _windows.Count;

	/// <inheritdoc/>
	public LayoutEngineIdentity Identity { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="FibonacciLayoutEngine"/> class.
	/// </summary>
	/// <param name="identity">The identity of the layout engine.</param>
	/// <param name="plugin">The Fibonacci layout plugin.</param>
	/// <param name="clockwise">Whether the spiral goes clockwise (default) or counter-clockwise.</param>
	/// <param name="layoutMode">The layout mode to use. Defaults to <see cref="FibonacciLayoutMode.Spiral"/>.</param>
	/// <param name="splitRatio">
	/// The ratio of space given to the first window in each split (0.0 to 1.0).
	/// Defaults to 0.5. Use higher values (e.g., 0.6-0.7) to give more space to primary windows on ultrawide displays.
	/// </param>
	public FibonacciLayoutEngine(
		LayoutEngineIdentity identity,
		IFibonacciLayoutPlugin plugin,
		bool clockwise = true,
		FibonacciLayoutMode layoutMode = FibonacciLayoutMode.Spiral,
		double splitRatio = 0.5
	)
	{
		Identity = identity;
		_plugin = plugin;
		_clockwise = clockwise;
		_layoutMode = layoutMode;
		_splitRatio = Math.Clamp(splitRatio, 0.1, 0.9);
		_windows = [];
	}

	private FibonacciLayoutEngine(
		FibonacciLayoutEngine layoutEngine,
		ImmutableList<IWindow> windows,
		bool clockwise,
		FibonacciLayoutMode layoutMode,
		double splitRatio
	)
	{
		Name = layoutEngine.Name;
		Identity = layoutEngine.Identity;
		_plugin = layoutEngine._plugin;
		_windows = windows;
		_clockwise = clockwise;
		_layoutMode = layoutMode;
		_splitRatio = splitRatio;
	}

	/// <inheritdoc/>
	public ILayoutEngine AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		if (_windows.IndexOf(window) >= 0)
		{
			Logger.Debug($"Window {window} already exists in layout engine {Name}");
			return this;
		}

		return new FibonacciLayoutEngine(this, _windows.Add(window), _clockwise, _layoutMode, _splitRatio);
	}

	/// <inheritdoc/>
	public ILayoutEngine RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window} from layout engine {Name}");

		int index = _windows.IndexOf(window);
		if (index < 0)
		{
			Logger.Debug($"Window {window} does not exist in layout engine {Name}");
			return this;
		}

		return new FibonacciLayoutEngine(this, _windows.RemoveAt(index), _clockwise, _layoutMode, _splitRatio);
	}

	/// <inheritdoc/>
	public bool ContainsWindow(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window}");
		return _windows.Contains(window);
	}

	/// <inheritdoc/>
	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window in layout engine {Name}");
		return _windows.Count == 0 ? null : _windows[0];
	}

	/// <inheritdoc/>
	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		Logger.Debug($"Performing Fibonacci layout with {_windows.Count} windows in mode {_layoutMode}");

		if (_windows.Count == 0)
		{
			yield break;
		}

		if (_windows.Count == 1)
		{
			yield return new WindowState { Window = _windows[0], Rectangle = rectangle, WindowSize = WindowSize.Normal };
			yield break;
		}

		// Calculate the layout rectangles for all windows based on mode
		IRectangle<int>[] rectangles = _layoutMode switch
		{
			FibonacciLayoutMode.Ultrawide => CalculateUltrawideModeRectangles(rectangle, _windows.Count),
			FibonacciLayoutMode.Columns => CalculateColumnsModeRectangles(rectangle, _windows.Count),
			_ => CalculateFibonacciRectangles(rectangle, _windows.Count),
		};

		for (int i = 0; i < _windows.Count; i++)
		{
			yield return new WindowState
			{
				Window = _windows[i],
				Rectangle = rectangles[i],
				WindowSize = WindowSize.Normal,
			};
		}
	}

	private IRectangle<int>[] CalculateFibonacciRectangles(IRectangle<int> area, int windowCount)
	{
		IRectangle<int>[] rectangles = new IRectangle<int>[windowCount];

		// Current working area that gets subdivided
		int x = area.X;
		int y = area.Y;
		int width = area.Width;
		int height = area.Height;

		// Initial split direction (Right means the new window goes to the right)
		SplitDirection direction = SplitDirection.Right;

		for (int i = 0; i < windowCount; i++)
		{
			if (i == windowCount - 1)
			{
				// Last window gets the remaining space
				rectangles[i] = new Rectangle<int>(x, y, width, height);
			}
			else
			{
				// Split the current area
				(IRectangle<int> currentRect, int newX, int newY, int newWidth, int newHeight) = SplitArea(
					x,
					y,
					width,
					height,
					direction
				);

				rectangles[i] = currentRect;

				// Update working area for next window
				x = newX;
				y = newY;
				width = newWidth;
				height = newHeight;

				// Rotate to next direction
				direction = GetNextDirection(direction);
			}
		}

		return rectangles;
	}

	private (IRectangle<int> rect, int newX, int newY, int newWidth, int newHeight) SplitArea(
		int x,
		int y,
		int width,
		int height,
		SplitDirection direction
	)
	{
		return direction switch
		{
			SplitDirection.Right =>
				// Current window on left, remaining space on right
				(
					new Rectangle<int>(x, y, (int)(width * _splitRatio), height),
					x + (int)(width * _splitRatio),
					y,
					width - (int)(width * _splitRatio),
					height
				),

			SplitDirection.Down =>
				// Current window on top, remaining space on bottom
				(
					new Rectangle<int>(x, y, width, (int)(height * _splitRatio)),
					x,
					y + (int)(height * _splitRatio),
					width,
					height - (int)(height * _splitRatio)
				),

			SplitDirection.Left =>
				// Current window on right, remaining space on left
				(
					new Rectangle<int>(x + width - (int)(width * _splitRatio), y, (int)(width * _splitRatio), height),
					x,
					y,
					width - (int)(width * _splitRatio),
					height
				),

			SplitDirection.Up =>
				// Current window on bottom, remaining space on top
				(
					new Rectangle<int>(x, y + height - (int)(height * _splitRatio), width, (int)(height * _splitRatio)),
					x,
					y,
					width,
					height - (int)(height * _splitRatio)
				),

			_ => throw new InvalidOperationException($"Unknown split direction: {direction}"),
		};
	}

	/// <summary>
	/// Calculates rectangles for ultrawide mode: first window on top, then remaining windows
	/// split horizontally in the bottom portion using Fibonacci-style subdivision.
	/// </summary>
	private IRectangle<int>[] CalculateUltrawideModeRectangles(IRectangle<int> area, int windowCount)
	{
		IRectangle<int>[] rectangles = new IRectangle<int>[windowCount];

		if (windowCount == 2)
		{
			// Simple top/bottom split
			int topHeight = (int)(area.Height * _splitRatio);
			rectangles[0] = new Rectangle<int>(area.X, area.Y, area.Width, topHeight);
			rectangles[1] = new Rectangle<int>(area.X, area.Y + topHeight, area.Width, area.Height - topHeight);
			return rectangles;
		}

		// First window gets the top portion
		int primaryHeight = (int)(area.Height * _splitRatio);
		rectangles[0] = new Rectangle<int>(area.X, area.Y, area.Width, primaryHeight);

		// Remaining windows share the bottom portion, split horizontally with Fibonacci subdivision
		int bottomY = area.Y + primaryHeight;
		int bottomHeight = area.Height - primaryHeight;

		int x = area.X;
		int width = area.Width;

		for (int i = 1; i < windowCount; i++)
		{
			if (i == windowCount - 1)
			{
				// Last window gets remaining space
				rectangles[i] = new Rectangle<int>(x, bottomY, width, bottomHeight);
			}
			else
			{
				// Split horizontally
				int windowWidth = (int)(width * _splitRatio);
				rectangles[i] = new Rectangle<int>(x, bottomY, windowWidth, bottomHeight);
				x += windowWidth;
				width -= windowWidth;
			}
		}

		return rectangles;
	}

	/// <summary>
	/// Calculates rectangles for columns mode: first window on left, then remaining windows
	/// stacked vertically in the right portion using Fibonacci-style subdivision.
	/// </summary>
	private IRectangle<int>[] CalculateColumnsModeRectangles(IRectangle<int> area, int windowCount)
	{
		IRectangle<int>[] rectangles = new IRectangle<int>[windowCount];

		if (windowCount == 2)
		{
			// Simple left/right split
			int leftWidth = (int)(area.Width * _splitRatio);
			rectangles[0] = new Rectangle<int>(area.X, area.Y, leftWidth, area.Height);
			rectangles[1] = new Rectangle<int>(area.X + leftWidth, area.Y, area.Width - leftWidth, area.Height);
			return rectangles;
		}

		// First window gets the left portion
		int primaryWidth = (int)(area.Width * _splitRatio);
		rectangles[0] = new Rectangle<int>(area.X, area.Y, primaryWidth, area.Height);

		// Remaining windows share the right portion, stacked vertically with Fibonacci subdivision
		int rightX = area.X + primaryWidth;
		int rightWidth = area.Width - primaryWidth;

		int y = area.Y;
		int height = area.Height;

		for (int i = 1; i < windowCount; i++)
		{
			if (i == windowCount - 1)
			{
				// Last window gets remaining space
				rectangles[i] = new Rectangle<int>(rightX, y, rightWidth, height);
			}
			else
			{
				// Split vertically
				int windowHeight = (int)(height * _splitRatio);
				rectangles[i] = new Rectangle<int>(rightX, y, rightWidth, windowHeight);
				y += windowHeight;
				height -= windowHeight;
			}
		}

		return rectangles;
	}

	private SplitDirection GetNextDirection(SplitDirection current)
	{
		if (_clockwise)
		{
			return current switch
			{
				SplitDirection.Right => SplitDirection.Down,
				SplitDirection.Down => SplitDirection.Left,
				SplitDirection.Left => SplitDirection.Up,
				SplitDirection.Up => SplitDirection.Right,
				_ => SplitDirection.Right,
			};
		}
		else
		{
			return current switch
			{
				SplitDirection.Right => SplitDirection.Up,
				SplitDirection.Up => SplitDirection.Left,
				SplitDirection.Left => SplitDirection.Down,
				SplitDirection.Down => SplitDirection.Right,
				_ => SplitDirection.Right,
			};
		}
	}

	/// <inheritdoc/>
	public ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window in direction {direction} from {window} in layout engine {Name}");

		int index = _windows.IndexOf(window);
		if (index < 0)
		{
			Logger.Debug($"Window {window} does not exist in layout engine {Name}");
			return this;
		}

		// Simple linear navigation based on window order
		int newIndex = direction switch
		{
			Direction.Left or Direction.Up => index - 1,
			_ => index + 1,
		};
		newIndex = newIndex.Mod(_windows.Count);

		_windows[newIndex].Focus();
		return this;
	}

	/// <inheritdoc/>
	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in direction {direction} in layout engine {Name}");

		int index = _windows.IndexOf(window);
		if (index < 0)
		{
			Logger.Debug($"Window {window} does not exist in layout engine {Name}");
			return this;
		}

		int newIndex = direction switch
		{
			Direction.Left or Direction.Up => index - 1,
			_ => index + 1,
		};
		newIndex = newIndex.Mod(_windows.Count);

		IWindow oldWindow = _windows[index];
		IWindow newWindow = _windows[newIndex];

		ImmutableList<IWindow> newList = _windows.SetItem(index, newWindow).SetItem(newIndex, oldWindow);
		return new FibonacciLayoutEngine(this, newList, _clockwise, _layoutMode, _splitRatio);
	}

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window) => this;

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point)
	{
		Logger.Debug($"Moving window {window} to point {point} in layout engine {Name}");

		// If window exists, just return this
		if (_windows.Contains(window))
		{
			return this;
		}

		// Add the window
		return AddWindow(window);
	}

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowStart(IWindow window)
	{
		Logger.Debug($"Minimizing window {window} in layout engine {Name}");
		return RemoveWindow(window);
	}

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowEnd(IWindow window)
	{
		Logger.Debug($"Restoring window {window} in layout engine {Name}");
		return AddWindow(window);
	}

	/// <inheritdoc/>
	public ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action)
	{
		if (action.Name == _plugin.ReverseSpiralActionName)
		{
			return new FibonacciLayoutEngine(this, _windows, !_clockwise, _layoutMode, _splitRatio);
		}

		if (action.Name == _plugin.CycleLayoutModeActionName)
		{
			FibonacciLayoutMode nextMode = _layoutMode switch
			{
				FibonacciLayoutMode.Spiral => FibonacciLayoutMode.Ultrawide,
				FibonacciLayoutMode.Ultrawide => FibonacciLayoutMode.Columns,
				FibonacciLayoutMode.Columns => FibonacciLayoutMode.Spiral,
				_ => FibonacciLayoutMode.Spiral,
			};
			return new FibonacciLayoutEngine(this, _windows, _clockwise, nextMode, _splitRatio);
		}

		return this;
	}
}
