namespace Whim;

/// <summary>
/// The sector containing monitors.
/// </summary>
public interface IMonitorSector
{
	/// <summary>
	/// All the monitors currently tracked by Whim. The monitors are ordered by their x-coordinate
	/// and then by their y-coordinate.
	///
	/// Your primary monitor will have the top-left coordinate be (0, 0).
	/// Accordingly, monitors to the left of the primary monitor will have negative x-coordinates,
	/// and monitors above the primary monitor will have negative y-coordinates.
	/// </summary>
	ImmutableArray<IMonitor> Monitors { get; }

	/// <summary>
	/// The handle of the monitor which is currently active, in <see cref="Monitors"/>.
	/// </summary>
	HMONITOR ActiveMonitorHandle { get; }

	/// <summary>
	/// The handle of the primary monitor, in <see cref="Monitors"/>.
	/// </summary>
	HMONITOR PrimaryMonitorHandle { get; }

	/// <summary>
	/// The handle of the last monitor which received an event sent by Windows which Whim did not ignore.
	/// </summary>
	HMONITOR LastWhimActiveMonitorHandle { get; }

	/// <summary>
	/// Cached flag indicating if any monitor has non-100% DPI scaling.
	/// This is updated when monitors change to avoid checking on every layout operation.
	/// </summary>
	bool HasNonStandardScaling { get; }
}
