using System.Linq;
using System.Text.RegularExpressions;

namespace Whim;

internal class FilterManager : IFilterManager
{
	#region Filters for specific properties
	// Use case-insensitive comparison to avoid allocating lowercase strings on every ShouldBeIgnored check
	private readonly HashSet<string> _ignoreWindowClasses = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> _ignoreProcessFileNames = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> _ignoreTitles = new(StringComparer.OrdinalIgnoreCase);
	#endregion

	/// <summary>
	/// Generic filter for windows.
	/// </summary>
	private readonly List<Filter> _filters = [];

	public void Add(Filter filter)
	{
		_filters.Add(filter);
	}

	public void Clear()
	{
		Logger.Debug($"Clearing filters");
		_ignoreWindowClasses.Clear();
		_ignoreProcessFileNames.Clear();
		_ignoreTitles.Clear();
		_filters.Clear();
	}

	public bool ShouldBeIgnored(IWindow window) =>
		_ignoreWindowClasses.Contains(window.WindowClass)
		|| (window.ProcessFileName is string processFileName && _ignoreProcessFileNames.Contains(processFileName))
		|| _ignoreTitles.Contains(window.Title)
		|| _filters.Any(f => f(window));

	public IFilterManager AddWindowClassFilter(string windowClass)
	{
		_ignoreWindowClasses.Add(windowClass);
		return this;
	}

	public IFilterManager AddProcessFileNameFilter(string processFileName)
	{
		_ignoreProcessFileNames.Add(processFileName);
		return this;
	}

	public IFilterManager AddTitleFilter(string title)
	{
		_ignoreTitles.Add(title);
		return this;
	}

	public IFilterManager AddTitleMatchFilter(string title)
	{
		Regex regex = new(title);
		_filters.Add(window => regex.IsMatch(window.Title));
		return this;
	}
}
