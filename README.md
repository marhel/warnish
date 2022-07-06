# Warnish
Warnish tries to parse the output of a dotnet build and outputs a sorted and grouped list of warnings.

Run it from your project folder using something like the following;

    BUILD_WARNINGS_LOG_FILE=project-warnings.log
    dotnet build -consoleloggerparameters:"Summary;Verbosity=normal" -m -p:"WarnLevel=5;EnforceCodeStyleInBuild=true" -t:"clean,build" -fl1 "/flp1:logFile=$BUILD_WARNINGS_LOG_FILE;warningsonly"
    dotnet run --project path/to/warnish/ $BUILD_WARNINGS_LOG_FILE

As an example of one code base I tried this on, instead of getting 465 lines of warnings, we get 97 lines, in warning frequency order. The last couple of lines were the following;

      5 SA1610             2P Property documentation should have value text
      5 SA1618             4P The documentation for type parameter '...' is missing
      6 CA1849             6P  '...' synchronously blocks. Use await instead.
      6 CA2000             0P Call System.IDisposable.Dispose on object created by '...' before all references to it are out of scope
      6 CA2201             2P Exception type '...' is not sufficiently specific
      6 CS1573             5P Parameter '...' has no matching param tag in the XML comment for '...' (but other parameters do)
      6 VSTHRD103          6P Result synchronously blocks. Use await instead.
      7 CA2225         1   1P Provide a method named 'ToT' or 'FromValueProviderBase' as an alternate for operator op_Implicit
      7 CA2225         1   1P Provide a method named 'Subtract' as a friendly alternate for operator op_Subtraction
      7 CA2225         1   1P Provide a method named 'Add' as a friendly alternate for operator op_Addition
      7 CA2225         4   4P Provide a method named '...' as a friendly alternate for operator op_Multiply
      7 CS8629             0P Nullable value type may be null.
      7 SA1517             6P Code should not contain blank lines at start of file
      8 RCS1194            6P Implement exception constructors.
     11 CA1002            10P Change 'List<...>' in '...' to use 'Collection<T>', 'ReadOnlyCollection<T>' or 'KeyedCollection<K,V>'
     12 CA1711         1   1P Either replace the suffix 'New' in type name New with the suggested numeric alternate '2' or provide a more meaningful suffix that distinguishes it from the type it replaces
     12 CA1711         5   2P Rename type '...' so that it does not end in 'Collection'
     12 CA1711         6   6P Rename type '...' so that it does not end in 'Enum'
     16 CA1032         4   2P Add the following constructor to '...Exception': public ...Exception(string message)
     16 CA1032         4   3P Add the following constructor to '...Exception': public ...Exception(string message, Exception innerException)
     16 CA1032         8   6P Add the following constructor to '...Exception': public ...Exception()
     16 SA1011             0P Closing square bracket should be followed by a space
     16 SA1012             0P Opening brace should be preceded by a space
     24 SA1304             0P Non-private readonly fields should begin with upper-case letter
     41 CA1034            34P Do not nest type '...' Alternatively, change its accessibility so that it is not externally visible.
     67 SA1307             0P Field '...' should begin with upper-case letter
     68 SA1401             0P Field should be private

So the last line says we had 68 cases of SA1401, fields that should have been private according to analysis. Of those, however, 0P means that none of those were in production code.
Some warning codes, like CA1711, have multiple variant texts, and the additional number after the warning code says how many there were of each variant.
