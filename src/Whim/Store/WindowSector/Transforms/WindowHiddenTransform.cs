namespace Whim;

/// <summary>
/// Handles when a window is hidden.
/// This will be called when a workspace is deactivated, or when a process hides a window.
/// For example, Discord will hide its window when it is minimized.
/// We only care about the hide event if the workspace is active and monitors are not changing.
/// During monitor changes, Whim deactivates workspaces which hides windows - we must ignore
/// those hide events to avoid incorrectly removing windows from tracking.
/// </summary>
/// <param name="Window"></param>
internal record WindowHiddenTransform(IWindow Window) : WindowRemovedTransform(Window)
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		// During monitor changes, Whim deactivates workspaces which calls HideWindow() on all windows.
		// We must ignore these hide events to avoid removing windows from tracking.
		if (mutableRootSector.MonitorSector.MonitorsChangingTasks > 0)
		{
			Logger.Debug($"Window {Window} hidden during monitor change, ignoring event");
			return Unit.Result;
		}

		if (!ctx.Store.Pick(PickMonitorByWindow(Window.Handle)).IsSuccessful)
		{
			Logger.Debug($"Window {Window} is not on a monitor, ignoring event");
			return Unit.Result;
		}

		return base.Execute(ctx, internalCtx, mutableRootSector);
	}
}
