namespace Mocca.Helpers;

/// <summary>
/// Helper functions for url paths.
/// </summary>
public static class UrlPathHelper
{
    public static bool Matches(ReadOnlySpan<char> path, ReadOnlySpan<char> pattern)
    {
        if (path.Length == 0 && pattern.Length == 0)
        {
            return true;
        }
        else if (path.Length == 0 && pattern.Length > 0)
        {
            return pattern[0] == '*' 
                   && (pattern.Length == 1  || (pattern.Length == 2  && pattern[1] == '*'));
        }
        else if (pattern.Length > 0 && pattern[0] == '*')
        {
            var pathTail = path.Slice(1);
            var patternTail = pattern.Slice(1);
            
            return Matches(path, patternTail) || Matches(pathTail, pattern);
        }
        else if (pattern.Length > 0)
        {
            var pathTail = path.Slice(1);
            var patternTail = pattern.Slice(1);

            return path[0] == pattern[0] && Matches(pathTail, patternTail);
        }
        else
        {
            return false;
        }
    }

}