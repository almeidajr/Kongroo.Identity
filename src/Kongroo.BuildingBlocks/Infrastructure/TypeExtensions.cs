namespace Kongroo.BuildingBlocks.Infrastructure;

public static class TypeExtensions
{
    extension(Type type)
    {
        public string ToDisplayName()
        {
            var typeName = type.GetTypeNameWithoutGenericArity();

            return type.IsGenericType ? $"{typeName}<{type.FormatGenericArguments()}>" : typeName;
        }

        public string GetTypeNameWithoutGenericArity()
        {
            var arityIndex = type.Name.IndexOf('`', StringComparison.Ordinal);

            return arityIndex >= 0 ? type.Name[..arityIndex] : type.Name;
        }

        public string FormatGenericArguments()
        {
            var genericArguments = type.GetGenericArguments().Select(argument => argument.ToDisplayName());

            return string.Join(", ", genericArguments);
        }
    }
}
