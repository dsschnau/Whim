namespace Whim;

/// <summary>
/// Reloads the layout engines for all workspaces using the current <see cref="IWorkspaceSector.CreateLayoutEngines"/> function.
/// Windows are migrated from old layout engines to the new ones.
/// Only active workspaces (visible on monitors) are queued for layout.
/// </summary>
public record ReloadLayoutEnginesTransform : Transform<Unit>
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		if (ctx.Store.IsDisposing)
		{
			return Result.FromError<Unit>(new WhimError("Whim is shutting down"));
		}

		WorkspaceSector sector = rootSector.WorkspaceSector;

		CreateLeafLayoutEngine[] engineCreators = sector.CreateLayoutEngines();
		if (engineCreators.Length == 0)
		{
			return Result.FromError<Unit>(new WhimError("No layout engine creators configured"));
		}

		// Get the set of active workspace IDs (visible on monitors)
		HashSet<WorkspaceId> activeWorkspaceIds = [];
		foreach (WorkspaceId wsId in rootSector.MapSector.MonitorWorkspaceMap.Values)
		{
			activeWorkspaceIds.Add(wsId);
		}

		// Reload layout engines for each workspace
		foreach (WorkspaceId workspaceId in sector.WorkspaceOrder)
		{
			if (!sector.Workspaces.TryGetValue(workspaceId, out Workspace? workspace))
			{
				continue;
			}

			// Get all windows currently in this workspace
			List<IWindow> windows = [];
			foreach (HWND hwnd in workspace.WindowPositions.Keys)
			{
				if (rootSector.WindowSector.Windows.TryGetValue(hwnd, out IWindow? window))
				{
					windows.Add(window);
				}
			}

			// Create new layout engines
			ImmutableList<ILayoutEngine>.Builder newLayoutEngines = ImmutableList.CreateBuilder<ILayoutEngine>();
			for (int i = 0; i < engineCreators.Length; i++)
			{
				ILayoutEngine currentEngine = engineCreators[i](new LayoutEngineIdentity());

				// Apply proxy layout engine creators
				foreach (ProxyLayoutEngineCreator createProxyLayoutEngineFn in sector.ProxyLayoutEngineCreators)
				{
					currentEngine = createProxyLayoutEngineFn(currentEngine);
				}

				// Add all windows to this layout engine
				foreach (IWindow window in windows)
				{
					currentEngine = currentEngine.AddWindow(window);
				}

				newLayoutEngines.Add(currentEngine);
			}

			// Update the workspace with new layout engines
			// Clamp active index if there are fewer engines than before
			int newActiveIndex = Math.Min(workspace.ActiveLayoutEngineIndex, engineCreators.Length - 1);
			newActiveIndex = Math.Max(0, newActiveIndex);

			sector.Workspaces = sector.Workspaces.SetItem(
				workspaceId,
				workspace with
				{
					LayoutEngines = newLayoutEngines.ToImmutable(),
					ActiveLayoutEngineIndex = newActiveIndex,
					PreviousLayoutEngineIndex = newActiveIndex,
				}
			);

			// Only queue active workspaces (visible on monitors) for layout
			if (activeWorkspaceIds.Contains(workspaceId))
			{
				sector.WorkspacesToLayout = sector.WorkspacesToLayout.Add(workspaceId);
			}
		}

		return Unit.Result;
	}
}
