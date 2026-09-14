// net48 doesn't ship the marker types the C# compiler needs to emit `init`
// accessors and `required` members (they were added to the BCL alongside
// .NET 5/7). The compiler only checks that these types *exist* with the
// right shape — it doesn't care that they come from this project instead of
// the runtime — so declaring them here lets the rest of the codebase keep
// using `init`/`required` unchanged while still targeting net48.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        public string FeatureName { get; }
        public bool IsOptional { get; set; }
    }
}

// net48's KeyValuePair<TKey, TValue> predates the Deconstruct method (added
// in .NET Core 2.0), which `foreach ((k, v) in dictionary)` relies on.
// Declaring the extension in KeyValuePair's own namespace means every file
// that already sees the type (which, with dictionaries in play, is all of
// them) picks it up with no extra `using`.
namespace System.Collections.Generic
{
    internal static class KeyValuePairPolyfillExtensions
    {
        public static void Deconstruct<TKey, TValue>(
            this KeyValuePair<TKey, TValue> pair,
            out TKey key,
            out TValue value
        )
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}
